using System;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using AFT.RegoV2.AdminWebsite.Common;
using AFT.RegoV2.AdminWebsite.Common.jqGrid;
using AFT.RegoV2.ApplicationServices.Payment;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Payment.ApplicationServices;
using AFT.RegoV2.Core.Security.ApplicationServices;
using AFT.RegoV2.Domain.BoundedContexts.Security.ApplicationServices;
using AFT.RegoV2.Domain.Payment.Data;
using AutoMapper;
using ServiceStack.Validation;

namespace AFT.RegoV2.AdminWebsite.Controllers
{
    public class SaveBankCommand
    {
        public Guid Id { get; set; }

        public Guid Brand { get; set; }
        [Required, MinLength(1), MaxLength(20), RegularExpression(@"^[a-zA-Z0-9\-_]+$")]
        public string BankId { get; set; }
        [Required, MinLength(1), MaxLength(50)]
        public string BankName { get; set; }
        [Required, StringLength(3)]
        public string Country { get; set; }
        [Required, MinLength(1), MaxLength(200)]
        public string Remark { get; set; }
    }

    public class BanksController : BaseController
    {
        private readonly BrandQueries _brandQueries;
        private readonly PaymentQueries _paymentQueries;
        private readonly BankCommands _bankCommands;
        private readonly UserService _userService;

        public BanksController(
            PaymentQueries paymentQueries,
            BrandQueries brandQueries,
            BankCommands bankCommands,
            UserService userService)
        {
            _paymentQueries = paymentQueries;
            _brandQueries = brandQueries;
            _bankCommands = bankCommands;
            _userService = userService;
        }

        [HttpGet]
        public ActionResult GetBank(Guid id)
        {
            var bank = _paymentQueries.GetBank(id);
            return this.Success(new
            {
                bank.BankId,
                bankName = bank.Name,
                country = bank.CountryCode,
                bank.BrandId,
                bank.Remark,
                LicenseeId = _brandQueries.GetBrandOrNull(bank.BrandId).Licensee.Id
            });
        }

        [HttpGet]
        public ActionResult List()
        {
            return Json(_paymentQueries.GetBankAccounts(), JsonRequestBehavior.AllowGet);
        }

        // TODO Permissions
        [SearchPackageFilter("searchPackage")]
        public object GetBanks(SearchPackage searchPackage)
        {
            var brandFilterSelecttions = _userService.GetBrandFilterSelections(CurrentUser.UserId);
            var banks = _paymentQueries.GetBanks()
                .Include(x => x.Brand)
                .Where(x => brandFilterSelecttions.Contains(x.BrandId));

            var dataBuilder = new SearchPackageDataBuilder<Bank>(searchPackage, banks);

            dataBuilder
                .SetFilterRule(obj => obj.Brand, (value) => x => x.Brand.Id == new Guid(value))
                .Map(obj => obj.Id,
                    obj =>
                        new[]
                        {
                            obj.BankId,
                            obj.Name,
                            obj.CountryCode,
                            obj.Brand.Name,
                            obj.Brand.LicenseeName,
                            obj.Remark,
                            obj.Created.DateTime.GetNormalizedDateTime(),
                            obj.CreatedBy,
                            obj.Updated.HasValue ? obj.Updated.Value.DateTime.GetNormalizedDateTime() : string.Empty,
                            obj.UpdatedBy
                        }
                );
            var data = dataBuilder.GetPageData(obj => obj.Name);
            return new JsonResult { Data = data, MaxJsonLength = int.MaxValue, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [HttpPost]
        public string Save(SaveBankCommand command)
        {
            try
            {
                var data = Mapper.DynamicMap<SaveBankData>(command);
                var result = command.Id == Guid.Empty
                    ? _bankCommands.Save(data)
                    : _bankCommands.Edit(data);

                return SerializeJson(new { Result = "success", Data = result.Message, result.Id });
            }
            catch (ValidationError ex)
            {
                return SerializeJson(new
                {
                    Result = "failed",
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return SerializeJson(new
                {
                    Result = "failed",
                    Message = ex.Message
                });
            }
        }
    }
}