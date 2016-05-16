using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web.Mvc;
using AFT.RegoV2.AdminWebsite.Common;
using AFT.RegoV2.AdminWebsite.Common.jqGrid;
using AFT.RegoV2.ApplicationServices.Player;
using AFT.RegoV2.Core.Payment.ApplicationServices;
using AFT.RegoV2.Core.Security.ApplicationServices;
using AFT.RegoV2.Domain.BoundedContexts.Payment.ApplicationServices.Exceptions;
using AFT.RegoV2.Domain.BoundedContexts.Security.ApplicationServices;
using AFT.RegoV2.Domain.Payment.ApplicationServices;
using AFT.RegoV2.Domain.Payment.ApplicationServices.Data;
using AFT.RegoV2.Domain.Payment.Data;
using AFT.RegoV2.Shared;
using ServiceStack.Validation;

namespace AFT.RegoV2.AdminWebsite.Controllers
{
    [Authorize]
    public class PaymentSettingsController : BaseController
    {
        private readonly IPaymentQueries _paymentQueries;
        private readonly PaymentSettingsCommands _settingsCommands;
        private readonly PaymentSettingsQueries _paymentSettingsQueries;
        private readonly IPlayerQueries _playerQueries;
        private readonly UserService _userService;

        public string CurrentUserName
        {
            get { return Thread.CurrentPrincipal.Identity.Name; }
        }

        public PaymentSettingsController(IPaymentQueries paymentQueries,
            PaymentSettingsCommands settingsCommands,
            PaymentSettingsQueries paymentSettingsQueries,
            IPlayerQueries playerQueries, UserService userService)
        {
            _paymentQueries = paymentQueries;
            _settingsCommands = settingsCommands;
            _paymentSettingsQueries = paymentSettingsQueries;
            _playerQueries = playerQueries;
            _userService = userService;
        }

        // TODO Permissions
        public string GetById(Guid id)
        {
            var setting = _paymentQueries.GetPaymentSettingById(id);
            return SerializeJson(setting);
        }

        [SearchPackageFilter("searchPackage")]
        public object List(SearchPackage searchPackage)
        {
            var brandFilterSelections = _userService.GetBrandFilterSelections(CurrentUser.UserId);

            var paymentSettings = _paymentQueries.GetPaymentSettings()
                .Where(x => brandFilterSelections.Contains(x.BrandId));

            var dataBuilder = new SearchPackageDataBuilder<PaymentSettings>(searchPackage, paymentSettings);

            dataBuilder
                .SetFilterRule(x => x.Brand, value => p => p.Brand.Id == new Guid(value))
                .Map(level => level.Id,
                    obj =>
                        new[]
                        {
                            obj.Brand.LicenseeName,
                            obj.Brand.Name,
                            obj.CurrencyCode,
                            obj.PaymentType.ToString(),
                            _playerQueries.VipLevels.Single(x => x.Id == new Guid(obj.VipLevel)).Name,
                            "Offline - " + obj.PaymentGateway.BankAccount.AccountId,
                            obj.Enabled.ToString(),
                            LabelHelper.LabelStatus(obj.Enabled),
                            obj.MinAmountPerTransaction.ToString(CultureInfo.InvariantCulture),
                            obj.MaxAmountPerTransaction.ToString(CultureInfo.InvariantCulture),
                            obj.MaxAmountPerDay.ToString(CultureInfo.InvariantCulture),
                            obj.MaxTransactionPerDay.ToString(CultureInfo.InvariantCulture),
                            obj.MaxTransactionPerWeek.ToString(CultureInfo.InvariantCulture),
                            obj.MaxTransactionPerMonth.ToString(CultureInfo.InvariantCulture),
                            obj.CreatedBy,
                            Format.FormatDate(obj.CreatedDate, false),
                            obj.UpdatedBy,
                            Format.FormatDate(obj.UpdatedDate, false),
                            obj.EnabledBy,
                            Format.FormatDate(obj.EnabledDate, false),
                            obj.DisabledBy,
                            Format.FormatDate(obj.DisabledDate, false)
                        }
                );
            var data = dataBuilder.GetPageData(level => level.Brand.Name);
            return new JsonResult { Data = data, MaxJsonLength = int.MaxValue, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public string GetVipLevels(Guid? brandId)
        {
            var vipLevels = _paymentSettingsQueries
                .GetVipLevels(brandId);

            return SerializeJson(new { vipLevels });
        }

        public string PaymentGetways(Guid brandId, string currencyCode)
        {
            var bankAccounts = _paymentSettingsQueries
                .GetBankAccounts(brandId, currencyCode);

            return SerializeJson(bankAccounts);
        }

        [HttpPost]
        public string Save(SavePaymentSettingsCommand model)
        {
            try
            {
                var result = _paymentSettingsQueries
                    .SaveSetting(model);

                return SerializeJson(new
                {
                    Result = "success",
                    Data = result.Message,
                    Id = result.PaymentSettingsId
                });
            }
            catch (RegoException regoEx)
            {
                return SerializeJson(new { Result = "failed", Data = regoEx.Message });
            }
            catch (ValidationError e)
            {
                return SerializeJson(new { Result = "failed", Data = e.ErrorMessage });
            }
        }

        [HttpPost]
        public ActionResult Enable(Guid id, string remarks)
        {
            try
            {
                _settingsCommands.Enable(id, remarks);
                return this.Success();
            }
            catch (Exception ex)
            {
                return this.Failed(ex);
            }
        }

        [HttpPost]
        public ActionResult Disable(Guid id, string remarks)
        {
            try
            {
                _settingsCommands.Disable(id, remarks);
                return this.Success();
            }
            catch (Exception ex)
            {
                return this.Failed(ex);
            }
        }
    }
}