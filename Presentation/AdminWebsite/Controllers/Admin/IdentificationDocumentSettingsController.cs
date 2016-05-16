using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using AFT.RegoV2.AdminWebsite.Common;
using AFT.RegoV2.AdminWebsite.Common.jqGrid;
using AFT.RegoV2.AdminWebsite.ViewModels;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Brand.Data;
using AFT.RegoV2.Core.Common.Data.Payment;
using AFT.RegoV2.Core.Player.ApplicationServices;
using AFT.RegoV2.Core.Player.Data;
using AFT.RegoV2.Core.Security.ApplicationServices;
using AFT.RegoV2.Core.Services.Security;
using AFT.RegoV2.Shared;
using AFT.RegoV2.Shared.Constants;
using AutoMapper;

namespace AFT.RegoV2.AdminWebsite.Controllers
{
    public class IdentificationDocumentSettingsController : BaseController
    {
        private readonly UserService _userService;
        private readonly BrandQueries _brandQueries;
        private readonly IdentificationDocumentSettingsService _service;
        private readonly SecurityRepository _securityRepository;

        public IdentificationDocumentSettingsController(UserService userService,
            BrandQueries brandQueries,
            IdentificationDocumentSettingsService service,
            SecurityRepository securityRepository)
        {
            _userService = userService;
            _brandQueries = brandQueries;
            _service = service;
            _securityRepository = securityRepository;
        }

        [HttpGet, SearchPackageFilter("searchPackage")]
        public ActionResult Data(SearchPackage searchPackage)
        {
            return new JsonResult
            {
                Data = SearchData(searchPackage),
                MaxJsonLength = int.MaxValue,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public ActionResult CreateSetting(IdentificationDocumentSettingsModel model)
        {
            try
            {
                var setting = Mapper.DynamicMap<IdentificationDocumentSettings>(model);
                _service.CreateSetting(setting);

                return this.Success();
            }
            catch (RegoException exception)
            {
                return this.Failed(exception);
            }
        }

        public string GetSetting(Guid id)
        {
            var role = _service.GetSettingById(id);
            var roleData = Mapper.DynamicMap<IdentificationDocumentSettingsModel>(role);

            return SerializeJson(roleData);
        }

        public string GetEditData(Guid? id = null)
        {
            if (!id.HasValue)
            {
                return SerializeJson(new
                {
                    Licensees = GetLicensees(),
                    TransactionTypes = Enum.GetNames(typeof(TransactionType)),
                    PaymentMethods = Enum.GetNames(typeof(PaymentMethod))
                });
            }

            var setting = _service.GetSettingById(id.Value);

            return SerializeJson(new
            {
                Licensees = GetLicensees(),
                TransactionTypes = Enum.GetNames(typeof(TransactionType)),
                PaymentMethods = Enum.GetNames(typeof(PaymentMethod)),
                Setting = new
                {
                    LicenseeId = setting.LicenseeId.ToString(),
                    BrandId = setting.BrandId.ToString(),
                    TransactionType = setting.TransactionType.HasValue ? setting.TransactionType.Value.ToString() : null,
                    PaymentMethod = setting.PaymentMethod.HasValue ? setting.PaymentMethod.Value.ToString() : null,
                    setting.IdBack,
                    setting.IdFront,
                    setting.CreditCardFront,
                    setting.CreditCardBack,
                    setting.POA,
                    setting.DCF,
                    setting.Remark
                }
            });
        }

        public ActionResult UpdateSetting(IdentificationDocumentSettingsModel data)
        {
            try
            {
                var setting = Mapper.DynamicMap<IdentificationDocumentSettings>(data);
                _service.UpdateSetting(setting);

                return this.Success(new
                {
                    Setting = setting
                });

            }
            catch (RegoException exception)
            {
                return this.Failed(exception);
            }
        }

        public IQueryable GetLicensees()
        {
            var licensees = _brandQueries.GetLicensees();

            var user = _securityRepository.Users
                .Include(x => x.Role)
                .Include(x => x.Licensees)
                .Single(u => u.Id == CurrentUser.UserId);

            if (user.Role.Id == RoleIds.LicenseeId || user.Role.Id == RoleIds.SingleBrandManagerId)
            {
                licensees = licensees
                    .Where(l => user.Licensees.Any(x => l.Id == x.Id) && l.Status == LicenseeStatus.Active)
                    .AsQueryable();
            }

            return licensees
                .OrderBy(l => l.Name)
                .Select(l => new { l.Name, l.Id });
        }

        public string GetLicenseeBrands(Guid licenseeId)
        {
            var brands = _brandQueries.GetBrands().Where(b => b.Licensee.Id == licenseeId);

            return SerializeJson(new
            {
                Brands = brands
                    .OrderBy(l => l.Name)
                    .Select(l => new { l.Name, l.Id })
            });
        }

        private object SearchData(SearchPackage searchPackage)
        {
            var settings = _service.GetSettings();
            var dataBuilder = new SearchPackageDataBuilder<IdentificationDocumentSettings>(searchPackage, settings);

            return dataBuilder
                .Map(setting => setting.Id, setting => GetRoleCell(setting))
                .GetPageData(setting => setting.CreatedBy);
        }

        private object GetRoleCell(IdentificationDocumentSettings setting)
        {
            var licensee = _brandQueries.GetLicensee(setting.LicenseeId);
            var brand = _brandQueries.GetBrand(setting.BrandId);
            return new[]
            {
                licensee.Name,
                brand.Name,
                setting.TransactionType.HasValue ? setting.TransactionType.Value.ToString() : "-",
                setting.PaymentMethod.HasValue ? setting.PaymentMethod.Value.ToString() : "-",
                setting.IdBack ? "Yes" : "No",
                setting.IdFront ? "Yes" : "No",
                setting.CreditCardFront ? "Yes" : "No",
                setting.CreditCardBack ? "Yes" : "No",
                setting.POA ? "Yes" : "No",
                setting.DCF ? "Yes" : "No"
            };
        }
    }
}