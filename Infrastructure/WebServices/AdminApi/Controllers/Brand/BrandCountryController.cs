using System;
using System.Linq;
using System.Web.Http;
using AFT.RegoV2.AdminApi.Controllers.Base;
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
    public class CountryViewModel
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public Guid BrandId { get; set; }
        public string BrandName { get; set; }
        public DateTimeOffset DateAdded { get; set; }
        public string AddedBy { get; set; }
    }

    [Authorize]
    public class BrandCountryController : BaseApiController
    {
        private readonly BrandQueries _brandQueries;
        private readonly BrandCommands _brandCommands;
        private readonly UserService _userService;

        public BrandCountryController(
            BrandQueries queries, 
            BrandCommands commands, 
            UserService userService, 
            IPermissionService permissionService)
            : base(permissionService)
        {
            _brandQueries = queries;
            _brandCommands = commands;
            _userService = userService;
        }

        [HttpGet]
        [Route("BrandCountry/List")]
        [Filters.SearchPackageFilter("searchPackage")]
        public object List([FromUri] SearchPackage searchPackage)
        {
            VerifyPermission(Permissions.View, Modules.SupportedCountries);
            return SearchData(searchPackage);
        }

        [HttpGet]
        [Route("BrandCountry/GetAssignData")]
        public IHttpActionResult GetAssignData(Guid brandId)
        {
            var brand = _brandQueries.GetBrandOrNull(brandId);
            var allowedCountries = _brandQueries.GetAllowedCountriesByBrand(brandId);
            var assignedCountries = _brandQueries.GetCountriesByBrand(brandId).ToArray();
            var availableCountries = allowedCountries.Except(assignedCountries);

            return Ok(new { 
                result = "success",
                data = new
                {
                    AvailableCountries = availableCountries.Select(x => new { x.Code, x.Name }),
                    AssignedCountries = assignedCountries.Select(x => new { x.Code, x.Name }),
                    IsActive = brand.Status == BrandStatus.Active
                } 
            });
        }

        [HttpPost]
        [Route("BrandCountry/Assign")]
        public IHttpActionResult Assign(AssignBrandCountryData data)
        {
            VerifyPermission(Permissions.Add, Modules.SupportedCountries);
            var validationResult = _brandCommands.ValidateThatBrandCountryCanBeAssigned(data);
            if (!validationResult.IsValid)
                return Ok(ValidationExceptionResponse(validationResult.Errors));

            _brandCommands.AssignBrandCountry(data);
            return Ok(new { result = "success" });
        }

        protected object SearchData(SearchPackage searchPackage)
        {
            var brandFilterSelections = _userService.GetBrandFilterSelections(UserId);
            var brandCountries = _brandQueries.GetAllBrandCountries()
                .Where(x => brandFilterSelections.Contains(x.BrandId));

            var countries = brandCountries.Select(bc => new CountryViewModel
            {
                Code = bc.Country.Code,
                Name = bc.Country.Name,
                BrandId = bc.BrandId,
                BrandName = bc.Brand.Name,
                DateAdded = bc.DateAdded,
                AddedBy = bc.AddedBy
            }).AsQueryable();

            var dataBuilder = new SearchPackageDataBuilder<CountryViewModel>(searchPackage, countries);

            dataBuilder.Map(record => record.BrandId + "," + record.Code, record => new[]
            {
                record.Code,
                record.Name,
                record.BrandName,
                Format.FormatDate(record.DateAdded, false),
                record.AddedBy
            });

            return dataBuilder.GetPageData(record => record.Code);
        }
    }
}