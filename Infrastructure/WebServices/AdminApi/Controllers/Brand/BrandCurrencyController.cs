using System;
using System.Linq;
using System.Web.Http;
using AFT.RegoV2.AdminApi.Controllers.Base;
using AFT.RegoV2.AdminWebsite.Common;
using AFT.RegoV2.AdminWebsite.Common.jqGrid;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Brand.ApplicationServices.Data;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Payment.ApplicationServices;
using AFT.RegoV2.Core.Security.ApplicationServices;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Domain.Security.Interfaces;

namespace AFT.RegoV2.AdminApi.Controllers.Brand
{
    public class CurrencyViewModel
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public Guid BrandId { get; set; }
        public string BrandName { get; set; }
        public string DefaultCurrency { get; set; }
        public string BaseCurrency { get; set; }
        public DateTimeOffset DateAdded { get; set; }
        public string AddedBy { get; set; }
    }

    [Authorize]
    public class BrandCurrencyController : BaseApiController
    {
        private readonly BrandQueries _brandQueries;
        private readonly BrandCommands _brandCommands;
        private readonly CurrencyManagerQueries _currencyManagerQueries;
        private readonly UserService _userService;

        public BrandCurrencyController(
            BrandQueries brandQueries,
            BrandCommands brandCommands,
            CurrencyManagerQueries currencyManagerQueries,
            UserService userService, 
            IPermissionService permissionService)
            : base(permissionService)
        {
            _brandQueries = brandQueries;
            _brandCommands = brandCommands;
            _currencyManagerQueries = currencyManagerQueries;
            _userService = userService;
        }

        [HttpGet]
        [Route("BrandCurrency/List")]
        [Filters.SearchPackageFilter("searchPackage")]
        public object List([FromUri] SearchPackage searchPackage)
        {
            return SearchData(searchPackage);
        }

        [HttpGet]
        [Route("BrandCurrency/GetBrandCurrencies")]
        public IHttpActionResult GetBrandCurrencies(Guid brandId)
        {
            var response = new
            {
                CurrencyCodes = _currencyManagerQueries.GetCurrencyCodes(brandId)
            };
            return Ok(response);
        }

        [HttpGet]
        [Route("BrandCurrency/GetAssignData")]
        public IHttpActionResult GetAssignData(Guid brandId)
        {
            return Ok(new
            {
                result = "success",
                data = _currencyManagerQueries.GetAssignData(brandId)
            });
        }

        [HttpPost]
        [Route("BrandCurrency/Assign")]
        public IHttpActionResult Assign(AssignBrandCurrencyData data)
        {
            VerifyPermission(Permissions.Add, Modules.BrandCurrencyManager);
            _brandCommands.AssignBrandCurrency(data);
            return Ok(new { result = "success", data = "app:currencies.successfullyAssigned" });
        }

        protected object SearchData(SearchPackage searchPackage)
        {
            var brandFilterSelections = _userService.GetBrandFilterSelections(UserId);
            var brands = _brandQueries.GetBrands().Where(x => brandFilterSelections.Contains(x.Id));

            var currencies = brands
                .SelectMany(x => x.BrandCurrencies,
                (x, c) => new CurrencyViewModel
                {
                    Code = c.Currency.Code,
                    Name = c.Currency.Name,
                    BrandId = x.Id,
                    BrandName = x.Name,
                    DefaultCurrency = x.DefaultCurrency,
                    BaseCurrency = x.BaseCurrency,
                    DateAdded = c.DateAdded,
                    AddedBy = c.AddedBy
                });

            var dataBuilder = new SearchPackageDataBuilder<CurrencyViewModel>(searchPackage, currencies);

            dataBuilder
                .Map(record => record.BrandId + "," + record.Code,
                    record =>
                        new object[]
                        {
                            record.Code,
                            record.Name,
                            record.BrandName,
                            record.DefaultCurrency,
                            record.BaseCurrency,
                            Format.FormatDate(record.DateAdded, false),
                            record.AddedBy
                        }
                );
            return dataBuilder.GetPageData(record => record.Code);
        }
    }
}