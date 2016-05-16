using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Brand.Data;
using AFT.RegoV2.Domain.Security.Interfaces;
using AFT.RegoV2.Shared;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Helpers;
using AFT.RegoV2.Tests.Common.TestDoubles;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Brand
{
    public class WalletManagerTests : AdminWebsiteUnitTestsBase
    {
        private FakeGameRepository _fakeGameRepository;
        private BrandCommands _brandCommands;
        private BrandQueries _brandQueries;
        private ISecurityProvider _securityProvider;
        private Guid _brandId;

        public override void BeforeEach()
        {
            base.BeforeEach();

            _fakeGameRepository = Container.Resolve<FakeGameRepository>();
            _brandCommands = Container.Resolve<BrandCommands>();
            _brandQueries = Container.Resolve<BrandQueries>();
            _securityProvider = Container.Resolve<ISecurityProvider>();
            var gamesHelper = Container.Resolve<GamesTestHelper>();

            var product = gamesHelper.CreateGameProvider();
            gamesHelper.CreateGame(product.Id, "Game");

            Container.Resolve<SecurityTestHelper>().SignInUser();

            var brand = Container.Resolve<BrandTestHelper>().CreateBrand();
            _brandId = brand.Id;
            brand.WalletTemplates.Clear();
        }

        [Test]
        public void Can_create_wallet_template()
        {
            var walletViewModel = new WalletTemplateViewModel
            {
                BrandId = _brandId,
                MainWallet = new WalletViewModel
                {
                    Name = "Main wallet",
                    IsMain = true,
                    ProductIds = new Guid[0]
                }
            };

            _brandCommands.CreateWalletStructureForBrand(walletViewModel);

            Assert.True(_brandQueries.GetWalletTemplates(_brandId).Any());
        }

        [Test]
        public void Can_update_wallet_template()
        {
            var walletViewModel = new WalletTemplateViewModel
            {
                BrandId = _brandId,
                MainWallet = new WalletViewModel
                {
                    Name = "Main wallet",
                    IsMain = true,
                    ProductIds = new Guid[0]
                }
            };

            _brandCommands.CreateWalletStructureForBrand(walletViewModel);

            Assert.True(_brandQueries.GetWalletTemplates(_brandId).Any());

            var wallets = _brandQueries.GetWalletTemplates(_brandId);
            walletViewModel.MainWallet.Id = wallets.First(x => x.IsMain).Id;
            walletViewModel.MainWallet.Name = "NewName";
            _brandCommands.UpdateWalletStructureForBrand(walletViewModel);

            var updatedWallet = _brandQueries.GetWalletTemplates(_brandId).First(x => x.IsMain);

            Assert.AreEqual(updatedWallet.Name, "NewName");
            Assert.AreEqual(updatedWallet.UpdatedBy, _securityProvider.User.UserId);
        }

        [Test]
        public void Can_create_wallet_template_with_main_wallet_products()
        {
            var walletViewModel = new WalletTemplateViewModel
            {
                BrandId = _brandId,
                MainWallet = new WalletViewModel
                {
                    Name = "Main wallet",
                    IsMain = true,
                    ProductIds = new[] {_fakeGameRepository.GameProviders.First().Id}
                }
            };

            _brandCommands.CreateWalletStructureForBrand(walletViewModel);

            var walletTemplate = _brandQueries.GetWalletTemplates(_brandId).First(x => x.IsMain);

            Assert.IsNotNull(walletTemplate.WalletTemplateProducts);
            Assert.IsNotEmpty(walletTemplate.WalletTemplateProducts);
            Assert.AreEqual(walletTemplate.WalletTemplateProducts.Count(), 1);
            Assert.AreEqual(walletTemplate.WalletTemplateProducts.First().ProductId,
                _fakeGameRepository.GameProviders.First().Id);
        }

        [Test]
        public void Can_create_product_wallets()
        {
            var walletViewModel = new WalletTemplateViewModel
            {
                BrandId = _brandId,
                MainWallet = new WalletViewModel
                {
                    Name = "Main wallet",
                    IsMain = true,
                    ProductIds = new Guid[0]
                },
                ProductWallets = new List<WalletViewModel>
                {
                    new WalletViewModel
                    {
                        Name = "Product wallet",
                        IsMain = false,
                        ProductIds = new[] {_fakeGameRepository.GameProviders.First().Id}
                    }
                }
            };

            _brandCommands.CreateWalletStructureForBrand(walletViewModel);

            var wallets = _brandQueries.GetWalletTemplates(_brandId).ToList();

            Assert.IsNotNull(wallets);
            Assert.IsNotEmpty(wallets);
            Assert.IsTrue(wallets.Count(x => x.IsMain) == 1);
            Assert.IsTrue(wallets.Count(x => !x.IsMain) == 1);
            Assert.IsTrue(wallets.First(x => !x.IsMain).WalletTemplateProducts.Count == 1);
        }

        [Test]
        public void Can_not_create_product_wallets_with_same_name_for_one_brand()
        {
            var walletViewModel = new WalletTemplateViewModel
            {
                BrandId = _brandId,
                MainWallet = new WalletViewModel
                {
                    Name = "Main wallet",
                    IsMain = true,
                    ProductIds = new Guid[0]
                },
                ProductWallets = new List<WalletViewModel>
                {
                    new WalletViewModel
                    {
                        Name = "Product wallet",
                        IsMain = false,
                        ProductIds = new[] {_fakeGameRepository.GameProviders.First().Id}
                    },
                    new WalletViewModel
                    {
                        Name = "Product wallet",
                        IsMain = false,
                        ProductIds = new[] {_fakeGameRepository.GameProviders.First().Id}
                    }
                }
            };

            Assert.Throws<RegoValidationException>(
                () => _brandCommands.CreateWalletStructureForBrand(walletViewModel));

            var wallets = _brandQueries.GetWalletTemplates(_brandId);
            Assert.IsNotNull(wallets);
            Assert.IsEmpty(wallets);
        }

        [Test]
        public void Can_not_create_product_wallets_without_products()
        {
            var walletViewModel = new WalletTemplateViewModel
            {
                BrandId = _brandId,
                MainWallet = new WalletViewModel
                {
                    Name = "Main wallet",
                    IsMain = true,
                    ProductIds = new Guid[0]
                },
                ProductWallets = new List<WalletViewModel>
                {
                    new WalletViewModel
                    {
                        Name = "Product wallet",
                        IsMain = false,
                        ProductIds = new[] {_fakeGameRepository.GameProviders.First().Id}
                    },
                    new WalletViewModel
                    {
                        Name = "Product wallet",
                        IsMain = false,
                        ProductIds = new Guid[0]
                    }
                }
            };

            Assert.Throws<RegoValidationException>(
                () => _brandCommands.CreateWalletStructureForBrand(walletViewModel));

            var wallets = _brandQueries.GetWalletTemplates(_brandId);
            Assert.IsNotNull(wallets);
            Assert.IsEmpty(wallets);
        }

        [Test]
        public void Can_not_create_wallets_with_same_name_for_one_brand()
        {
            var walletViewModel = new WalletTemplateViewModel
            {
                BrandId = _brandId,
                MainWallet = new WalletViewModel
                {
                    Name = "a",
                    IsMain = true,
                    ProductIds = new Guid[0]
                },
                ProductWallets = new List<WalletViewModel>
                {
                    new WalletViewModel
                    {
                        Name = "a",
                        IsMain = false,
                        ProductIds = new[] {_fakeGameRepository.GameProviders.First().Id}
                    }
                }
            };

            Assert.Throws<RegoValidationException>(
                () => _brandCommands.CreateWalletStructureForBrand(walletViewModel));

            var wallets = _brandQueries.GetWalletTemplates(_brandId);
            Assert.IsNotNull(wallets);
            Assert.IsEmpty(wallets);
        }

        [Test]
        public void Can_not_create_wallets_with_empty_names()
        {
            var walletViewModel = new WalletTemplateViewModel
            {
                BrandId = _brandId,
                MainWallet = new WalletViewModel
                {
                    Name = "",
                    IsMain = true,
                    ProductIds = new Guid[0]
                },
                ProductWallets = new List<WalletViewModel>
                {
                    new WalletViewModel
                    {
                        Name = "",
                        IsMain = false,
                        ProductIds = new[] {_fakeGameRepository.GameProviders.First().Id}
                    }
                }
            };

            Assert.Throws<RegoValidationException>(
                () => _brandCommands.CreateWalletStructureForBrand(walletViewModel));

            var wallets = _brandQueries.GetWalletTemplates(_brandId);
            Assert.IsNotNull(wallets);
            Assert.IsEmpty(wallets);
        }
    }
}