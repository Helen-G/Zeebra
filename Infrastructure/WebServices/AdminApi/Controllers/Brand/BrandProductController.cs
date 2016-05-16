using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using AFT.RegoV2.AdminApi.Controllers.Base;
using AFT.RegoV2.AdminWebsite.Common.jqGrid;
using AFT.RegoV2.AdminWebsite.ViewModels;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Brand.Validators;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Brand;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.Core.Security.ApplicationServices;
using AFT.RegoV2.Core.Security.Common;
using ServiceStack.Validation;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Common.Interfaces.Brand;

namespace AFT.RegoV2.AdminApi.Controllers.Brand
{
    public class AssignBrandProductModel : IAssignBrandProductModel
    {
        public Guid Brand { get; set; }
        public string[] Products { get; set; }
    }

    [Authorize]
    public class BrandProductController : BaseApiController
    {
        private readonly IGameQueries _gameQueries;
        private readonly BrandQueries _brandQueries;
        private readonly BrandCommands _brandCommands;
        private readonly IGameManagement _gameCommands;
        private readonly UserService _userService;


        public BrandProductController(
            IGameQueries gameQueries,
            BrandQueries brandQueries, 
            BrandCommands brandCommands, 
            IGameManagement gameCommands, 
            UserService userService, 
            IPermissionService permissionService)
            : base(permissionService)
        {
            _gameQueries = gameQueries;
            _brandQueries = brandQueries;
            _brandCommands = brandCommands;
            _gameCommands = gameCommands;
            _userService = userService;
        }

        [HttpGet]
        [Route("BrandProduct/List")]
        [Filters.SearchPackageFilter("searchPackage")]
        public object List([FromUri] SearchPackage searchPackage)
        {
            VerifyPermission(Permissions.View, Modules.SupportedProducts);
            return SearchData(searchPackage);
        }

        [HttpGet]
        [Route("BrandProduct/GetAssignData")]
        public IHttpActionResult GetAssignData(Guid brandId)
        {
            // TODO missing permission check on brand.

            var brand = _brandQueries.GetBrandOrNull(brandId);
            var allowedProducts = GetAllowedBrandProducts(brandId);
            var assignedProducts = GetAssignedBrandProducts(brandId).ToArray();
            var availableProducts = allowedProducts.Except(assignedProducts);

            return Ok(new
            {
                result = "success",
                data = new
                {
                    AvailableProducts = availableProducts,
                    AssignedProducts = assignedProducts,
                    IsActive = brand.Status == BrandStatus.Active
                }
            });
        }

        [HttpPost]
        [Route("BrandProduct/Assign")]
        public IHttpActionResult Assign(AssignBrandProductModel model)
        {
            var data = new AssignBrandProductsData
            {
                BrandId = model.Brand,
                ProductsIds = model.Products.Select(Guid.Parse).ToArray()
            };

            var validationResult = _brandCommands.ValidateThatBrandProductsCanBeAssigned(data);
            if (!validationResult.IsValid)
                return Ok(ValidationExceptionResponse(validationResult.Errors));

            _brandCommands.AssignBrandProducts(data);
            return Ok(new { result = "success" });
        }

        [HttpGet]
        [Route("BrandProduct/BetLevels")]
        public string BetLevels(Guid brandId, Guid productId)
        {
            var data = new BrandProductSettingsData
            {
                BetLevels = _gameQueries
                    .GetBetLimits(productId, brandId)
                    .Select(x => new BetLevelData
                    {
                        Id = x.Id,
                        Code = x.LimitId,
                        Name = x.Name,
                        Description = x.Description
                    }).ToArray()
            };
            
            return SerializeJson(new
            {
                Setting = data,
                Currencies = _brandQueries.GetCurrenciesByBrand(brandId).Select(x => x.Code)
            });
        }

        [HttpPost]
        [Route("BrandProduct/ProductSettings")]
        public string ProductSettings(BrandProductSettingsData data)
        {
            VerifyPermission(Permissions.Add, Modules.SupportedProducts);
            VerifyPermission(Permissions.Add, Modules.BetLevels);
            VerifyPermission(Permissions.Edit, Modules.BetLevels);

            try
            {
                _gameCommands.UpdateProductSettings(data);
            }
            catch (ValidationError)
            {
                return false.ToString();
            }
            return true.ToString();
        }

        protected object SearchData(SearchPackage searchPackage)
        {
            var brandFilterSelections = _userService.GetBrandFilterSelections(UserId);

            var products = GetBrandProductViewModels(null)
                .Where(x => brandFilterSelections.Contains(x.BrandId))
                .AsQueryable();

            var dataBuilder = new SearchPackageDataBuilder<BrandProductData>(searchPackage, products);

            dataBuilder.Map(
                record =>
                    record.BrandId + "," + record.GameProviderId,
                record =>
                    new[]
                    {
                        record.BrandName,
                        record.GameProviderName
                    }
                );

            return dataBuilder.GetPageData(record => record.BrandName);
        }

        private IEnumerable<BrandProductData> GetBrandProductViewModels(Guid? brandId)
        {
            var brandProducts = _brandQueries.GetAllowedProducts(UserId, brandId).ToList();
            var productIds = brandProducts.Select(x => x.ProductId).Distinct();

            var gameProviders = _gameQueries.GetGameProviders().Where(x => productIds.Contains(x.Id));

            return brandProducts.Join(gameProviders, b => b.ProductId, s => s.Id,
                (b, s) => new BrandProductData()
                {
                    BrandId = b.BrandId,
                    BrandName = b.Brand.Name,
                    GameProviderId = s.Id,
                    GameProviderName = s.Name
                });
        }

        private IEnumerable<ProductViewModel> GetAllowedBrandProducts(Guid brandId)
        {
            var brand = _brandQueries.GetBrandOrNull(brandId);
            var productIds = brand.Licensee.Products.Select(x => x.ProductId);
            return ProductViewModel.BuildFromIds(_gameQueries, productIds);
        }

        private IEnumerable<ProductViewModel> GetAssignedBrandProducts(Guid brandId)
        {
            var brand = _brandQueries.GetBrandOrNull(brandId);
            var productIds = brand.Products.Select(x => x.ProductId);
            return ProductViewModel.BuildFromIds(_gameQueries, productIds);
        }
    }
}