using System;
using System.Linq;
using System.Web.Http;
using AFT.RegoV2.AdminApi.Controllers.Base;
using AFT.RegoV2.AdminApi.Interface;
using AFT.RegoV2.AdminWebsite.Common;
using AFT.RegoV2.AdminWebsite.Common.jqGrid;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Brand.Validators;
using AFT.RegoV2.Core.Common.Data.Brand;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Security.ApplicationServices;
using AFT.RegoV2.Core.Security.Common;

namespace AFT.RegoV2.AdminApi.Controllers.Brand
{
    [Authorize]
    public class BrandController : BaseApiController
    {
        private readonly BrandQueries _brandQueries;
        private readonly BrandCommands _brandCommands;
        private readonly UserService _userService;

        public BrandController(
            BrandQueries brandQueries,
            BrandCommands brandCommands, 
            UserService userService,
            IPermissionService permissionService)
            : base(permissionService)
        {
            _brandQueries = brandQueries;
            _brandCommands = brandCommands;
            _userService = userService;
        }

        [HttpGet]
        [Route("Brand/GetUserBrands")]
        public BrandsResponse GetUserBrands()
        {
            var brands = _brandQueries.GetBrands();
            var filteredBrands = _brandQueries.GetFilteredBrands(brands, UserId);
            var brandFilterSelections = _userService.GetBrandFilterSelections(UserId);

            return new BrandsResponse()
            {
                Brands = filteredBrands
                    .OrderBy(x => x.Licensee.Name)
                    .ThenBy(x => x.Name)
                    .Select(b => new Interface.Brand()
                    {
                        Id = b.Id,
                        Name = b.Name,
                        LicenseeId = b.Licensee.Id,
                        Currencies = b.BrandCurrencies.Select(c => new Currency
                        {
                            Code = c.Currency.Code,
                            Name = c.Currency.Name
                        }),
                        VipLevels = b.VipLevels.Select(v => new VipLevel
                        {
                            Code = v.Code, 
                            Name = v.Name
                        }),
                        IsSelectedInFilter = brandFilterSelections.Contains(b.Id)
                    }).ToList()
            };
        }

        [HttpGet]
        [Filters.SearchPackageFilter("searchPackage")]
        [Route("Brand/GetBrands")]
        public object GetBrands([FromUri] SearchPackage searchPackage)
        {
            var brandFilterSelections = _userService.GetBrandFilterSelections(UserId);

            var brands = _brandQueries.GetFilteredBrands(_brandQueries.GetAllBrands(), UserId)
                .Where(x => brandFilterSelections.Contains(x.Id));

            var dataBuilder = new SearchPackageDataBuilder<Core.Brand.Data.Brand>(searchPackage, brands);

            dataBuilder.Map(brand => brand.Id, brand => new[]
            {
                brand.Code,
                brand.Name,
                brand.Licensee.Name,
                brand.Type.ToString(),
                brand.Status.ToString(),
                brand.PlayerPrefix,
                Enum.GetName(typeof (PlayerActivationMethod), brand.PlayerActivationMethod),
                brand.DefaultCulture ?? string.Empty,
                brand.CreatedBy,
                Format.FormatDate(brand.DateCreated, false),
                brand.Remarks,
                brand.UpdatedBy,
                Format.FormatDate(brand.DateUpdated, false),
                brand.ActivatedBy,
                Format.FormatDate(brand.DateActivated, false),
                brand.DeactivatedBy,
                Format.FormatDate(brand.DateDeactivated, false),
            });

            return dataBuilder.GetPageData(brand => brand.Name);
        }

        [HttpGet]
        [Route("Brand/GetAddData")]
        public object GetAddData()
        {
            var licenseeFilterSelections = _userService.GetLicenseeFilterSelections(UserId);

            var licensees = _brandQueries.GetFilteredLicensees(_brandQueries.GetLicensees(), UserId)
                .Where(x => licenseeFilterSelections.Contains(x.Id))
                .Select(x => new { x.Id, x.Name })
                .OrderBy(x => x.Name)
                .ToArray();

            var brands = _brandQueries.GetFilteredBrands(_brandQueries.GetBrands(), UserId).ToArray();

            return new
            {
                licensees = licensees.Select(licensee => new
                {
                    licensee.Id,
                    licensee.Name,
                    prefixRequired = brands.Any(brand =>
                        brand.Licensee.Id == licensee.Id &&
                        brand.PlayerPrefix != null)
                }),
                types = Enum.GetNames(typeof (BrandType)).OrderBy(x => x),
                timeZones = TimeZoneInfo.GetSystemTimeZones().Select(x => new {x.Id, x.DisplayName}),
                playerActivationMethods = Enum.GetNames(typeof (PlayerActivationMethod)).OrderBy(x => x),
            };
        }

        [HttpGet]
        [Route("Brand/GetEditData")]
        public object GetEditData(Guid id)
        {
            var licensees = _brandQueries.GetFilteredLicensees(_brandQueries.GetLicensees(), UserId)
                .Select(x => new { x.Id, x.Name })
                .OrderBy(x => x.Name)
                .ToArray();

            var brands = _brandQueries.GetBrands().ToArray();

            var brand = brands.FirstOrDefault(x => x.Id == id);

            var brandData = brand == null
                ? null
                : new
                {
                    licensee = brand.Licensee.Id,
                    type = Enum.GetName(typeof(BrandType), brand.Type),
                    name = brand.Name,
                    code = brand.Code,
                    enablePlayerPrefix = brand.EnablePlayerPrefix,
                    playerPrefix = brand.PlayerPrefix,
                    playerActivationMethod = Enum.GetName(typeof(PlayerActivationMethod), brand.PlayerActivationMethod),
                    internalAccounts = brand.InternalAccountsNumber,
                    timeZoneId = brand.TimezoneId,
                    remarks = brand.Remarks
                };

            return new
            {
                licensees = licensees.Select(x => new
                {
                    x.Id,
                    x.Name,
                    prefixRequired = brands.Any(y =>
                            y.Licensee.Id == x.Id &&
                            y.PlayerPrefix != null)
                }),
                types = Enum.GetNames(typeof(BrandType)).OrderBy(x => x),
                timeZones = TimeZoneInfo.GetSystemTimeZones().Select(x => new { x.Id, x.DisplayName }),
                playerActivationMethods = Enum.GetNames(typeof(PlayerActivationMethod)).OrderBy(x => x),
                brand = brandData
            };
        }

        [HttpGet]
        [Route("Brand/GetViewData")]
        public object GetViewData(Guid id)
        {
            var brand = _brandQueries.GetBrandOrNull(id);

            return new
            {
                licensee = brand.Licensee.Name,
                type = Enum.GetName(typeof(BrandType), brand.Type),
                name = brand.Name,
                code = brand.Code,
                enablePlayerPrefix = brand.EnablePlayerPrefix,
                playerPrefix = brand.PlayerPrefix,
                playerActivationMethod = Enum.GetName(typeof(PlayerActivationMethod), brand.PlayerActivationMethod),
                internalAccounts = brand.InternalAccountsNumber,
                timeZone = TimeZoneInfo.FindSystemTimeZoneById(brand.TimezoneId).DisplayName,
                remarks = brand.Remarks,
                status = Enum.GetName(typeof(BrandStatus), brand.Status)
            };
        }

        [HttpPost]
        [Route("Brand/Add")]
        public IHttpActionResult Add(AddBrandData model)
        {
            VerifyPermission(Permissions.Add, Modules.BrandManager);
            var id = _brandCommands.AddBrand(model);
            return Ok(new {result = "success", data = new {id}});
        }

        [HttpPost]
        [Route("Brand/Edit")]
        public IHttpActionResult Edit(EditBrandData model)
        {
            VerifyPermission(Permissions.Edit, Modules.BrandManager);
            _brandCommands.EditBrand(model);
            return Ok(new { result = "success" });
        }

        [HttpGet]
        [Route("Brand/GetCountries")]
        public IHttpActionResult GetCountries(Guid brandId)
        {
            var response = new
            {
                Data = new { Countries = _brandQueries.GetCountriesByBrand(brandId).Select(x => new { x.Code, x.Name }) }
            };
            return Ok(response);
        }

        [HttpPost]
        [Route("Brand/Activate")]
        public IHttpActionResult Activate(ActivateBrandData data)
        {
            VerifyPermission(Permissions.Activate, Modules.BrandManager);
            var validationResult = _brandCommands.ValidateThatBrandCanBeActivated(data.BrandId, data.Remarks);
            if (!validationResult.IsValid)
                return Ok(ValidationExceptionResponse(validationResult.Errors));
            
            _brandCommands.ActivateBrand(data.BrandId, data.Remarks);
            return Ok(new { result = "success" });
        }

        [HttpPost]
        [Route("Brand/Deactivate")]
        public IHttpActionResult Deactivate(DeactivateBrandData data)
        {
            VerifyPermission(Permissions.Deactivate, Modules.BrandManager);
            _brandCommands.DeactivateBrand(data.BrandId, data.Remarks);
            return Ok(new { result = "success" });
        }

        [HttpGet]
        [Route("Brand/Brands")]
        public string Brands([FromUri] bool useFilter, Guid[] licensees)
        {
            var brands = _brandQueries.GetBrands();

            if (licensees != null)
                brands = brands.Where(x => licensees.Contains(x.Licensee.Id));

            brands = _brandQueries.GetFilteredBrands(brands, UserId);

            if (useFilter)
            {
                var brandFilterSelections = _userService.GetBrandFilterSelections(UserId);

                brands = brands.Where(x => brandFilterSelections.Contains(x.Id));
            }

            return SerializeJson(new
            {
                brands = brands.OrderBy(x => x.Name).Select(x => new { x.Id, x.Name })
            });
        }
    }
}