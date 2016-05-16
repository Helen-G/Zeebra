using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Web.Mvc;
using AFT.RegoV2.AdminWebsite.Common;
using AFT.RegoV2.AdminWebsite.Common.jqGrid;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Brand.Data;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Brand;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Game.ApplicationServices;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.Core.Security.ApplicationServices;
using AFT.RegoV2.Domain.BoundedContexts.Security.ApplicationServices;
using ServiceStack.Common;
using ServiceStack.Validation;

namespace AFT.RegoV2.AdminWebsite.Controllers
{
    [Authorize]
    public class WalletController : BaseController
    {
        #region Fields

        private readonly BrandCommands _brandCommands;
        private readonly BrandQueries _brandQueries;
        private readonly IGameQueries _gameQueries;
        private readonly UserService _userService;

        #endregion

        #region Ctor and initialization

        public WalletController(
            BrandCommands brandCommands,
            BrandQueries brandQueries,
            IGameQueries gameQueries, 
            UserService userService)
        {
            _brandCommands = brandCommands;
            _brandQueries = brandQueries;
            _gameQueries = gameQueries;
            _userService = userService;
        }

        #endregion

        #region Actions

        [SearchPackageFilter("searchPackage")]
        public ActionResult List(SearchPackage searchPackage)
        {
            var data = SearchWallets(searchPackage);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        private object SearchWallets(SearchPackage searchPackage)
        {
            var brandFilterSelections = _userService.GetBrandFilterSelections(CurrentUser.UserId);
            var wallets = _brandQueries.GetWalletTemplates().Where(x => brandFilterSelections.Contains(x.Brand));
            var dataBuilder = new SearchPackageDataBuilder<WalletListDTO>(searchPackage, wallets);

            dataBuilder
                .Map(walletTemplate => walletTemplate.LicenseeId.ToString() + "," + walletTemplate.Brand.ToString() + "," + _brandQueries.GetBrandOrNull(walletTemplate.Brand).Status.ToString(),
                    walletTemplate => new[]
                    {
                        walletTemplate.LicenseeName,
                        walletTemplate.BrandName,
                        _brandQueries.GetBrandOrNull(walletTemplate.Brand).Status.ToString(),
                        walletTemplate.CreatedBy,
                        Format.FormatDate(walletTemplate.DateCreated, false),
                        walletTemplate.UpdatedBy,
                        Format.FormatDate(walletTemplate.DateUpdated, false)
                    }
                );

            return dataBuilder.GetPageData(walletTemplate => walletTemplate.DateCreated);
        }

        public string Brands(Guid licensee, bool isEditMode)
        {
            var brands = _brandQueries.GetFilteredBrands(
                _brandQueries.GetAllBrands()
                .Where(b => b.Licensee.Id == licensee), 
                CurrentUser.UserId)
                .ToList();

            if (!isEditMode)
            {
                var brandFilterSelections = _userService.GetBrandFilterSelections(CurrentUser.UserId);

                brands = brands.Where(b =>
                    b.Status == BrandStatus.Inactive &&
                    b.WalletTemplates.IsEmpty() &&
                    brandFilterSelections.Contains(b.Id))
                    .ToList();
            }

            return SerializeJson(new
            {
                Brands = brands
                    .OrderBy(b => b.Name)
                    .Select(b => new {name = b.Name, id = b.Id})
            });
        }

        public string GameProviders(Guid brandId)
        {
            var allowedProductIds = _brandQueries
                .GetBrandOrNull(brandId)
                .Products
                .Select(x => x.ProductId)
                .ToList();

            var gameProviders = _gameQueries
                .GetGameProviders()
                .Where(x => allowedProductIds.Contains(x.Id))
                .Select(x => new {name = x.Name, id = x.Id});

            return SerializeJson(gameProviders);
        }

        public string WalletsInfo(Guid brandId)
        {
            var mainWallet = _brandQueries
                .GetWalletTemplates(brandId).FirstOrDefault(x => x.IsMain);
            var productWallets = _brandQueries.GetWalletTemplates(brandId).Where(x => !x.IsMain);

            var mainWaletName = mainWallet == null ? "Main wallet" : mainWallet.Name;
            var response = new
            {
                MainWallet = new
                {
                    AssignedProducts = mainWallet == null ? null : GetGameProviders(mainWallet.WalletTemplateProducts, brandId),
                    Name = mainWaletName,
                    Id = mainWallet == null ? Guid.Empty : mainWallet.Id
                },
                ProductWallets = productWallets.ToArray().Select(x => new
                {
                    AssignedProducts = GetGameProviders(x.WalletTemplateProducts, brandId),
                    Name = x.Name,
                    Id = x.Id
                }).ToArray()
            };
            return SerializeJson(response);
        }

        public IEnumerable<object> GetGameProviders(IEnumerable<WalletTemplateProduct> walletTemplateProducts,
            Guid brandId)
        {
            var ids = walletTemplateProducts.Select(x => x.ProductId);

            return _gameQueries
                .GetGameProviders(brandId)
                .Where(x => ids.Contains(x.Id))
                .Select(x => new {id = x.Id, name = x.Name});
        }

        [HttpPost]
        public ActionResult Wallet(WalletTemplateViewModel model)
        {
            try
            {
                var message = String.Empty;
                if (model.MainWallet.Id == null || model.MainWallet.Id == Guid.Empty)
                {
                    _brandCommands.CreateWalletStructureForBrand(model);
                    message = "app:wallet.common.created";
                }
                else
                {
                    _brandCommands.UpdateWalletStructureForBrand(model);
                    message = "app:wallet.common.updated";
                }
                return this.Success(message);
            }
            catch (ValidationError e)
            {
                return this.Failed(e);
            }
            catch(Exception e)
            {
                return this.Failed(e);
            }
        }

        #endregion
    }
}