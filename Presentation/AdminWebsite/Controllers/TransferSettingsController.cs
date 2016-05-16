using System;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using AFT.RegoV2.AdminWebsite.Common;
using AFT.RegoV2.AdminWebsite.Common.jqGrid;
using AFT.RegoV2.ApplicationServices.Player;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Payment.ApplicationServices;
using AFT.RegoV2.Core.Security.ApplicationServices;
using AFT.RegoV2.Domain.Payment.ApplicationServices;
using AFT.RegoV2.Domain.Payment.ApplicationServices.Data;
using AFT.RegoV2.Domain.Payment.Data;
using AFT.RegoV2.Shared;
using ServiceStack.Validation;

namespace AFT.RegoV2.AdminWebsite.Controllers
{
    [Authorize]
    public class TransferSettingsController : BaseController
    {
        private readonly IPaymentQueries _paymentQueries;
        private readonly TransferSettingsCommands _settingsCommands;
        private readonly UserService _userService;
        private readonly BrandQueries _brandQueries;

        public TransferSettingsController(
            IPaymentQueries paymentQueries,
            TransferSettingsCommands settingsCommands,
            BrandQueries brandQueries,
            UserService userService)
        {
            _paymentQueries = paymentQueries;
            _settingsCommands = settingsCommands;
            _brandQueries = brandQueries;
            _userService = userService;
        }

        // TODO Permissions

        public string GetById(Guid id)
        {
            var transferSettings = _paymentQueries.GetTransferSettings(id);
            var obj = new
            {
                Brand = new
                {
                    transferSettings.Brand.Id,
                    transferSettings.Brand.Name,
                    Licensee = new
                    {
                        id = transferSettings.Brand.LicenseeId
                    }
                },
                TransferType = LabelHelper.LabelTransferType(transferSettings.TransferType),
                transferSettings.CurrencyCode,
                VipLevel = _paymentQueries.GetVipLevel(transferSettings.VipLevelId).Name,
                Wallet = _brandQueries.GetWalletTemplate(new Guid(transferSettings.WalletId)).Name,
                transferSettings.MinAmountPerTransaction,
                transferSettings.MaxAmountPerTransaction,
                transferSettings.MaxAmountPerDay,
                transferSettings.MaxTransactionPerDay,
                transferSettings.MaxTransactionPerWeek,
                transferSettings.MaxTransactionPerMonth,
            };
            return SerializeJson(obj);
        }

        [SearchPackageFilter("searchPackage")]
        public object List(SearchPackage searchPackage)
        {
            var brandFilterSelections = _userService.GetBrandFilterSelections(CurrentUser.UserId);

            var transferSettings = _paymentQueries.GetTransferSettings()
                .Where(x => brandFilterSelections.Contains(x.BrandId));

            var dataBuilder = new SearchPackageDataBuilder<TransferSettings>(searchPackage, transferSettings);

            dataBuilder
                .SetFilterRule(x => x.Brand, value => p => p.Brand.Id == new Guid(value))
                .Map(level => level.Id,
                    obj =>
                        new[]
                        {
                            obj.Brand.LicenseeName,
                            obj.Brand.Name,
                            LabelHelper.LabelTransferType(obj.TransferType),
                            obj.VipLevel.Name,
                            obj.CurrencyCode,
                            _brandQueries.GetWalletTemplate(new Guid(obj.WalletId)).Name,
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
                            Format.FormatDate(obj.DisabledDate, false),
                            obj.Enabled.ToString()
                        }
                );
            var data = dataBuilder.GetPageData(level => level.Brand.Name);
            return new JsonResult { Data = data, MaxJsonLength = int.MaxValue, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public string GetVipLevels(Guid? brandId)
        {
            var vipLevels = brandId.HasValue
                ? _paymentQueries.VipLevels().Where(x => x.BrandId == brandId).Select(x => new { x.Id, x.Name })
                : _paymentQueries.VipLevels().Select(x => new { x.Id, x.Name });

            return SerializeJson(new { vipLevels });
        }

        [HttpPost]
        public string Save(SaveTransferSettingsCommand model)
        {
            model.TimezoneId = _brandQueries.GetBrandOrNull(model.Brand).TimezoneId;

            try
            {
                string message;

                Guid transferSettingsId = model.Id;
                if (model.Id == Guid.Empty)
                {
                    transferSettingsId = _settingsCommands.AddSettings(model);
                    message = "CreatedSuccessfully";
                }
                else
                {
                    _settingsCommands.UpdateSettings(model);
                    message = "UpdatedSuccessfully";
                }

                return SerializeJson(new { Result = "success", Data = message, Id = transferSettingsId });
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
                var timezoneId = _brandQueries.GetBrandOrNull(_paymentQueries.GetTransferSettings(id).BrandId).TimezoneId;
                _settingsCommands.Enable(id, timezoneId, remarks);
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
                var timezoneId = _brandQueries.GetBrandOrNull(_paymentQueries.GetTransferSettings(id).BrandId).TimezoneId;
                _settingsCommands.Disable(id, timezoneId, remarks);
                return this.Success();
            }
            catch (Exception ex)
            {
                return this.Failed(ex);
            }
        }
    }
}