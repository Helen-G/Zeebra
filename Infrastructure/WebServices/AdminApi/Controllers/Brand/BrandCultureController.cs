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
    public class CultureViewModel
    {
        public string Code { get; set; }
        public Guid BrandId { get; set; }
        public string BrandName { get; set; }
        public string Status { get; set; }
        public DateTimeOffset DateAdded { get; set; }
        public string AddedBy { get; set; }
    }

    [Authorize]
    public class BrandCultureController : BaseApiController
    {
        private readonly BrandQueries _queries;
        private readonly BrandCommands _brandCommands;
        private readonly UserService _userService;

        public BrandCultureController(
            BrandQueries queries, 
            BrandCommands brandCommands, 
            UserService userService, 
            IPermissionService permissionService)
            : base(permissionService)
        {
            _queries = queries;
            _brandCommands = brandCommands;
            _userService = userService;
        }

        [HttpGet]
        [Route("BrandCulture/List")]
        [Filters.SearchPackageFilter("searchPackage")]
        public object List([FromUri] SearchPackage searchPackage)
        {
            VerifyPermission(Permissions.View, Modules.SupportedLanguages);
            return SearchData(searchPackage);
        }

        [HttpGet]
        [Route("BrandCulture/GetAssignData")]
        public IHttpActionResult GetAssignData(Guid brandId)
        {
            var brand = _queries.GetBrandOrNull(brandId);
            var allowedCultures = _queries.GetAllowedCulturesByBrand(brandId);
            var assignedCultures = brand.BrandCultures.Select(x => x.Culture).ToList();
            var availableCultures = allowedCultures.Except(assignedCultures);
            
            return Ok(new
            {
                result = "success",
                data = new
                {
                    AvailableCultures = availableCultures.Select(x => x.Code),
                    AssignedCultures = assignedCultures.Select(x => x.Code),
                    brand.DefaultCulture,
                    IsActive = brand.Status == BrandStatus.Active
                }
            });
        }

        [HttpPost]
        [Route("BrandCulture/Assign")]
        public IHttpActionResult Assign(AssignBrandCultureData data)
        {
            VerifyPermission(Permissions.Add, Modules.SupportedLanguages);
            var validationResult = _brandCommands.ValidateThatBrandCultureCanBeAssigned(data);
            if (!validationResult.IsValid)
                return Ok(ValidationExceptionResponse(validationResult.Errors));

            _brandCommands.AssignBrandCulture(data);
            return Ok(new { result = "success" });
        }

        protected object SearchData(SearchPackage searchPackage)
        {
            var brandFilterSelections = _userService.GetBrandFilterSelections(UserId);
            var brandCultures = _queries.GetAllBrandCultures()
                .Where(x => brandFilterSelections.Contains(x.BrandId));

            var cultures = brandCultures.Select(c => new CultureViewModel
            {
                Code = c.CultureCode,
                BrandId = c.Brand.Id,
                BrandName = c.Brand.Name,
                Status = c.Brand.Status.ToString(),
                DateAdded = c.DateAdded,
                AddedBy = c.AddedBy
            }).AsQueryable();

            var dataBuilder = new SearchPackageDataBuilder<CultureViewModel>(searchPackage, cultures);

            dataBuilder.Map(record => record.BrandId + "," + record.Code, record => new object[]
            {
                record.Code,
                record.BrandName,
                record.Status,
                Format.FormatDate(record.DateAdded, false),
                record.AddedBy
            });

            return dataBuilder.GetPageData(record => record.Code);
        }
    }
}