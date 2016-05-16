using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using AFT.RegoV2.AdminWebsite.Common;
using AFT.RegoV2.AdminWebsite.Common.jqGrid;
using AFT.RegoV2.ApplicationServices.Payment;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Payment.ApplicationServices;
using AFT.RegoV2.Core.Payment.Data;
using AFT.RegoV2.Core.Payment.Data.Commands;
using AFT.RegoV2.Core.Security.ApplicationServices;
using AFT.RegoV2.Domain.BoundedContexts.Security.ApplicationServices;
using AFT.RegoV2.Domain.Payment.Data;
using AFT.RegoV2.Shared;
using AutoMapper;
using ServiceStack.Validation;
using EditPaymentLevelModel = AFT.RegoV2.AdminWebsite.ViewModels.EditPaymentLevelModel;

namespace AFT.RegoV2.AdminWebsite.Controllers
{
    [Authorize]
    public class PaymentLevelController : BaseController
    {
        private readonly BrandQueries _brandQueries;
        private readonly PaymentQueries _paymentQueries;
        private readonly PaymentLevelQueries _paymentLevelQueries;
        private readonly PaymentLevelCommands _paymentLevelCommands;
        private readonly UserService _userService;

        public PaymentLevelController(
            BrandQueries brandQueries,
            PaymentQueries paymentQueries,
            PaymentLevelQueries paymentLevelQueries,
            PaymentLevelCommands paymentLevelCommands,
            UserService userService)
        {
            _brandQueries = brandQueries;
            _paymentQueries = paymentQueries;
            _paymentLevelQueries = paymentLevelQueries;
            _paymentLevelCommands = paymentLevelCommands;
            _userService = userService;
        }

        // TODO Permissions
        public string GetById(Guid id)
        {
            var level = _paymentLevelQueries
                .GetPaymentLevelById(id);
            return SerializeJson(level);
        }

        [SearchPackageFilter("searchPackage")]
        public object List(SearchPackage searchPackage)
        {
            var brandFilterSelections = _userService.GetBrandFilterSelections(CurrentUser.UserId);

            var paymentLevels = _paymentQueries.GetPaymentLevelsAsQueryable()
                .Where(x => brandFilterSelections.Contains(x.BrandId));

            var dataBuilder = new SearchPackageDataBuilder<PaymentLevel>(searchPackage, paymentLevels);

            dataBuilder
                .SetFilterRule(x => x.Brand, (value) => p => p.Brand.Id == new Guid(value))
                .Map(level => level.Id,
                    level =>
                        new object[]
                        {
                            level.Name,
                            level.Code,
                            level.CurrencyCode,
                            Enum.GetName(typeof(PaymentLevelStatus), level.Status),
                            level.CreatedBy,
                            Format.FormatDate(level.DateCreated, false),
                            level.UpdatedBy,
                            Format.FormatDate(level.DateUpdated, false),
                            level.ActivatedBy,
                            Format.FormatDate(level.DateActivated, false),
                            level.DeactivatedBy,
                            Format.FormatDate(level.DateDeactivated, false)
                        }
                );
            var data = dataBuilder.GetPageData(level => level.Name);
            return new JsonResult { Data = data, MaxJsonLength = int.MaxValue, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        // TODO Move to a higher level
        [HttpGet]
        public string GetBrandCurrencies(Guid brandId)
        {
            return SerializeJson(new
            {
                Currencies = _paymentLevelQueries.GetBrandCurrencies(brandId)
            });
        }

        [HttpPost]
        public dynamic Save(EditPaymentLevelModel model)
        {
            try
            {
                var editData = Mapper.DynamicMap<EditPaymentLevel>(model);
                var result = editData.Id.HasValue
                    ? _paymentLevelCommands.Edit(editData)
                    : _paymentLevelCommands.Save(editData);

                return SerializeJson(new { Result = "success", Data = result.Message, result.PaymentLevelId });
            }
            catch (ValidationError ex)
            {
                return ValidationErrorResponseActionResult(ex);
            }
            catch (RegoException ex)
            {
                return SerializeJson(new { Result = "failed", Data = ex.Message });
            }
            catch (Exception ex)
            {
                return this.Failed(ex);
            }
        }

        [SearchPackageFilter("searchPackage")]
        public object GetBankAccounts(SearchPackage searchPackage)
        {
            var dataBuilder = new SearchPackageDataBuilder<BankAccount>(searchPackage,
                _paymentQueries.GetBankAccounts()
                    .Where(x => x.Status == BankAccountStatus.Active)
                    .Include(ba => ba.Bank)
                    .Include(a => a.PaymentLevels)
                    );
            dataBuilder
                .SetFilterRule(x => x.PaymentLevels, (value) => ba => (value == null) || ba.PaymentLevels.Any(x => x.Id == new Guid(value)))
                .SetFilterRule(x => x.Bank.Brand, (value) => x => x.Bank.Brand.Id == new Guid(value))
                .SetFilterRule(x => x.CurrencyCode, (value) => c => c.CurrencyCode == value)
                .Map(account => account.Id,
                    account =>
                        new[]
                        {
                            account.AccountId,
                            account.Bank.Name,
                            account.Branch,
                            account.AccountName,
                            account.AccountNumber
                        }
                );
            var data = dataBuilder.GetPageData(account => account.AccountId);
            return new JsonResult { Data = data, MaxJsonLength = int.MaxValue, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [HttpGet]
        public string Licensees()
        {
            var query = _brandQueries.GetLicensees().Where(l => l.Brands.Any(b => b.BrandCurrencies.Any()));
            var licenseeFilterSelections = _userService.GetLicenseeFilterSelections(CurrentUser.UserId);

            var licensees = _brandQueries.GetFilteredLicensees(query, CurrentUser.UserId)
                .Where(x => licenseeFilterSelections.Contains(x.Id));

            return SerializeJson(new
            {
                licensees = licensees.OrderBy(x => x.Name).Select(x => new { x.Name, x.Id })
            });
        }

        [HttpGet]
        public string Brands(Guid licensee)
        {
            var query = _brandQueries.GetBrands()
                .Where(x =>
                    x.Licensee.Id == licensee &&
                    x.BrandCurrencies.Any());

            var brandFilterSelections = _userService.GetBrandFilterSelections(CurrentUser.UserId);

            var brands = _brandQueries.GetFilteredBrands(query, CurrentUser.UserId)
                .Where(x => brandFilterSelections.Contains(x.Id));

            return SerializeJson(new
            {
                brands = brands.Select(x => new { x.Name, x.Id })
            });
        }

        [HttpPost]
        public ActionResult Activate(ActivatePaymentLevelCommand data)
        {
            var validationResult = _paymentLevelCommands.ValidatePaymentLevelCanBeActivated(data);

            if (!validationResult.IsValid)
                return ValidationErrorResponseActionResult(validationResult.Errors);

            _paymentLevelCommands.Activate(data);

            return this.Success();
        }

        [HttpGet]
        public string Deactivate(Guid id)
        {
            var status = _paymentLevelQueries.GetDeactivatePaymentLevelStatus(id);
            var statusName = Enum.GetName(typeof(DeactivatePaymentLevelStatus), status);

            var newLevelsRequired = status == DeactivatePaymentLevelStatus.CanDeactivateIsAssigned ||
                                    status == DeactivatePaymentLevelStatus.CanDeactivateIsDefault;

            var paymentLevels = newLevelsRequired
                ? _paymentLevelQueries.GetReplacementPaymentLevels(id).Select(x => new { x.Id, x.Name })
                : null;

            return SerializeJson(new
            {
                status = statusName,
                paymentLevels
            });
        }

        [HttpPost]
        public ActionResult Deactivate(DeactivatePaymentLevelCommand data)
        {
            var validationResult = _paymentLevelCommands.ValidatePaymentLevelCanBeDeactivated(data);

            if (!validationResult.IsValid)
                return ValidationErrorResponseActionResult(validationResult.Errors);

            _paymentLevelCommands.Deactivate(data);

            return this.Success();
        }
    }
}