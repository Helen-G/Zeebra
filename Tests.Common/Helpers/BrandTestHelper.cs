using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Core.Brand;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Brand.ApplicationServices.Data;
using AFT.RegoV2.Core.Brand.Data;
using AFT.RegoV2.Core.Brand.Validators;
using AFT.RegoV2.Core.Common.Brand.Data;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Admin;
using AFT.RegoV2.Core.Common.Data.Brand;
using AFT.RegoV2.Core.Fraud.ApplicationServices;
using AFT.RegoV2.Core.Game.Data;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.Core.Payment.ApplicationServices;
using AFT.RegoV2.Tests.Common.Extensions;
using Brand = AFT.RegoV2.Core.Brand.Data.Brand;
using Licensee = AFT.RegoV2.Core.Brand.Data.Licensee;
using RiskLevel = AFT.RegoV2.Core.Fraud.Data.RiskLevel;
using VipLevel = AFT.RegoV2.Core.Brand.Data.VipLevel;
using WalletTemplate = AFT.RegoV2.Core.Brand.Data.WalletTemplate;

namespace AFT.RegoV2.Tests.Common.Helpers
{
    public class BrandTestHelper
    {
        private readonly IBrandRepository _brandRepository;
        private readonly BrandCommands _brandCommands;
        private readonly LicenseeCommands _licenseeCommands;
        private readonly BrandQueries _brandQueries;
        private readonly GamesTestHelper _gamesTestHelper;
        private readonly PaymentTestHelper _paymentTestHelper;
        private readonly CultureCommands _cultureCommands;
        private readonly RiskLevelCommands _riskLevelCommands;
        private readonly IGameRepository _gameRepository;

        public BrandTestHelper(
            IBrandRepository brandRepository,
            IGameRepository gameRepository,
            BrandCommands brandCommands,
            LicenseeCommands licenseeCommands,
            BrandQueries brandQueries,
            GamesTestHelper gamesTestHelper,
            PaymentTestHelper paymentTestHelper,
            CultureCommands cultureCommands,
            RiskLevelCommands riskLevelCommands)
        {
            _brandRepository = brandRepository;
            _gameRepository = gameRepository;
            _brandCommands = brandCommands;
            _licenseeCommands = licenseeCommands;
            _brandQueries = brandQueries;
            _gamesTestHelper = gamesTestHelper;
            _paymentTestHelper = paymentTestHelper;
            _cultureCommands = cultureCommands;
            _riskLevelCommands = riskLevelCommands;
        }

        public Brand CreateBrand(
            Licensee licensee = null,
            Country country = null,
            Culture culture = null,
            Currency currency = null,
            bool isActive = false
            )
        {

            licensee = licensee ?? _brandRepository.Licensees.FirstOrDefault() ?? CreateLicensee();

            var products = licensee.Products;

            country = country ?? licensee.Countries.First();
            culture = culture ?? licensee.Cultures.First();
            currency = currency ?? licensee.Currencies.First();
            
            var brandId = CreateBrand(licensee, PlayerActivationMethod.Automatic);
            CreateWallet(licensee.Id, brandId, products.Select(x => x.ProductId).ToArray());
            AssignCountry(brandId, country.Code);
            AssignCurrency(brandId, currency.Code);
            _brandRepository.SaveChanges();
            AssignCulture(brandId, culture.Code);
            AssignProducts(brandId, products.Select(x => x.ProductId).ToArray());
            CreateRiskLevel(brandId);

            _paymentTestHelper.CreateBank(brandId, country.Code);
            _paymentTestHelper.CreateBankAccount(brandId, currency.Code);
            _paymentTestHelper.CreatePaymentLevel(brandId, currency.Code);

            CreateVipLevel(brandId);

            if (isActive)
                _brandCommands.ActivateBrand(brandId, TestDataGenerator.GetRandomString());

            return _brandQueries.GetBrandOrNull(brandId);
        }

        public Guid CreateBrand(Licensee licensee, PlayerActivationMethod playerActivationMethod)
        {
            var brandId = _brandCommands.AddBrand(new AddBrandData
            {
                Code = TestDataGenerator.GetRandomString(),
                InternalAccounts = 1,
                EnablePlayerPrefix = true,
                PlayerPrefix = TestDataGenerator.GetRandomString(3),
                Licensee = licensee.Id,
                Name = TestDataGenerator.GetRandomString(),
                PlayerActivationMethod = playerActivationMethod,
                TimeZoneId = TestDataGenerator.GetRandomTimeZone().Id,
                Type = BrandType.Integrated
            });

            licensee.Brands.Add(_brandRepository.Brands.Single(b => b.Id == brandId));

            return brandId;
        }

        public Licensee CreateLicensee(bool isActive = true, IEnumerable<Culture> cultures = null, IEnumerable<Country> countries = null, IEnumerable<Currency> currencies = null, string [] productIds = null )
        {
            countries = countries ?? new List<Country> { CreateCountry("CA", "Canada") };
            cultures = cultures ?? new List<Culture> { CreateCulture("en-CA", "English (Canada)") };
            currencies = currencies ?? new List<Currency> { CreateCurrency("CAD", "Canadian Dollar") };

            if (productIds == null)
            {
                var product = _gamesTestHelper.CreateGameProvider();
                productIds = new [] { product.Id.ToString() };
            }

            var licenseeId = _licenseeCommands.Add(new AddLicenseeData
            {
                BrandCount = 10,
                Name = TestDataGenerator.GetRandomString(),
                CompanyName = TestDataGenerator.GetRandomString(),
                Email = TestDataGenerator.GetRandomEmail(),
                ContractStart = DateTime.UtcNow,
                ContractEnd = DateTime.UtcNow.AddMonths(1),
                TimeZoneId = TestDataGenerator.GetRandomTimeZone().Id,
                Products = productIds ,
                Countries = countries.Select(c => c.Code).ToArray(),
                Currencies = currencies.Select(c => c.Code).ToArray(),
                Languages = cultures.Select(c => c.Code).ToArray()
            });

            if (isActive)
                _licenseeCommands.Activate(licenseeId, TestDataGenerator.GetRandomString());

            return _brandQueries.GetLicensee(licenseeId);
        }

        public void CreateWallet(Guid licenseeId, Guid brandId, IEnumerable<Guid> productIds = null)
        {
            IEnumerable<Guid> mainWalletProductIds = null;

            if (productIds != null)
            {
                var productList = productIds.ToList();
                if (productList.Count() > 1)
                {
                    mainWalletProductIds = new List<Guid> {productList.First()};
                    productList.RemoveAt(0);
                    productIds = productList;
                }
            }
            _brandCommands.CreateWalletStructureForBrand(new WalletTemplateViewModel
            {
                BrandId = brandId,
                LicenseeId = licenseeId,
                MainWallet = new WalletViewModel
                {
                    Name = "Main",
                    IsMain = true,
                    ProductIds = mainWalletProductIds ?? new List<Guid> { Guid.Empty }
                },
                ProductWallets = new List<WalletViewModel>
                {
                    new WalletViewModel
                    {
                        Name = "Product",
                        IsMain = false,
                        ProductIds = productIds ?? new List<Guid>{Guid.Empty}
                    }
                }
            });

            //Unit tests brand to wallet template persistence fix
            var brand = _brandRepository.Brands.Single(b => b.Id == brandId);
            if (brand != null)
                brand.WalletTemplates.ForEach(wt =>
                {
                    wt.Brand = brand;
                    _brandRepository.WalletTemplates.Add(wt);
                });
        }

        public Currency CreateCurrency(string code, string name)
        {
            var currency = _brandRepository.Currencies.SingleOrDefault(c => c.Code == code);
            if (currency == null)
            {
                currency = new Currency
                {
                    Code = code,
                    Name = name
                };
                _brandRepository.Currencies.Add(currency);
            }
            return currency;
        }

        public void AssignCurrency(Guid brandId, string code)
        {
            _brandCommands.AssignBrandCurrency(new AssignBrandCurrencyData
            {
                Brand = brandId,
                Currencies = new[] { code },
                DefaultCurrency = code,
                BaseCurrency = code,
            });
            _brandRepository.SaveChanges();
        }

        public void AssignProducts(Guid brandId, IEnumerable<Guid> productIds)
        {
            _brandCommands.AssignBrandProducts(new AssignBrandProductsData
            {
                BrandId = brandId,
                ProductsIds = productIds.ToArray()
            });
        }

        public Culture CreateCulture(string code, string name)
        {
            var culture = _brandRepository.Cultures.SingleOrDefault(c => c.Code == code);
            if (culture == null)
            {
                _cultureCommands.Save(new EditCultureData
                {
                    Code = code,
                    Name = name,
                    NativeName = name
                });
                _brandCommands.ActivateCulture(code, "remark");
                culture = _brandRepository.Cultures.Single(c => c.Code == code);
            }
            return culture;
        }

        public void AssignCulture(Guid brandId, string cultureCode)
        {
            _brandCommands.AssignBrandCulture(new AssignBrandCultureData
            {
                Brand = brandId,
                Cultures = new[] { cultureCode },
                DefaultCulture = cultureCode
            });
        }

        public Country CreateCountry(string code, string name)
        {
            var country = _brandRepository.Countries.SingleOrDefault(c => c.Code == code);
            if (country == null)
            {
                _brandCommands.CreateCountry(code, name);
                country = _brandRepository.Countries.Single(c => c.Code == code);
            }

            return country;
        }

        public void AssignCountry(Guid brandId, string code)
        {
            _brandCommands.AssignBrandCountry(new AssignBrandCountryData
            {
                Brand = brandId,
                Countries = new[] { code }
            });
        }

        public VipLevel CreateVipLevel(Guid brandId, int limitCount = 0, bool isDefault = true)
        {
            var brand = _brandRepository.Brands.Single(x => x.Id == brandId);
            var vipLevelName = TestDataGenerator.GetRandomString();
            int rank;
            do
            {
                rank = TestDataGenerator.GetRandomNumber(100);
            } while (_brandRepository.VipLevels.Any(vl => vl.Rank == rank));
            var limits = new List<VipLevelLimitViewModel>();
            for (var i = 0; i < limitCount; i++)
            {
                var gameProvider = _gamesTestHelper.CreateGameProvider();
                var betLimit = _gamesTestHelper.CreateBetLevel(gameProvider, brand.Id);
                limits.Add(new VipLevelLimitViewModel
                {
                    Id = Guid.NewGuid(),
                    CurrencyCode = brand.DefaultCurrency,
                    GameProviderId = gameProvider.Id,
                    BetLimitId = betLimit.Id
                });
            }

            var newVipLevel = new VipLevelViewModel
            {
                Name = vipLevelName,
                Code = vipLevelName.Remove(3),
                Brand = brand.Id,
                Rank = rank,
                Limits = limits,
                IsDefault = isDefault
            };
            _brandCommands.AddVipLevel(newVipLevel);
            var vipLevel = _brandQueries.GetVipLevels().Single(x => x.Code == newVipLevel.Code);

            return vipLevel;
        }

        public void CreateRiskLevel(Guid brandId)
        {
            _riskLevelCommands.Create(new RiskLevel
            {
                BrandId = brandId,
                Level = TestDataGenerator.GetRandomNumber(1000),
                Name = TestDataGenerator.GetRandomAlphabeticString(5),
                Description = TestDataGenerator.GetRandomAlphabeticString(5)
            });
        }

        public Licensee GetDefaultLicensee()
        {
            var licensee = _brandQueries.GetLicensees().First(x => x.Name == "Flycow");
            return licensee;
        }

        public WalletTemplate GetProductWalletTemplate(Guid? brandId = null)
        {

            if (brandId.HasValue == false)
                brandId = _brandRepository.Brands.Last().Id;
            return
                _brandRepository
                    .WalletTemplates.Where(x => x.Name.Contains("Product"))
                    .Single(x => x.Brand.Id == brandId);
        }

        public WalletTemplate GetMainWalletTemplate(Guid? brandId = null)
        {
            if (brandId.HasValue == false)
                brandId = _brandRepository.Brands.Last().Id;
            return
                _brandRepository
                    .WalletTemplates.Where(x => x.Name == "Main")
                    .Single(x => x.Brand.Id == brandId);
        }

        public Brand CreateActiveBrandWithProducts()
        {
            var mainWalletProduct = _gamesTestHelper.CreateGameProvider();
            var productWalletProduct = _gamesTestHelper.CreateGameProvider();

            var licensee = CreateLicensee(productIds: new[]
            {
                mainWalletProduct.Id.ToString(),
                productWalletProduct.Id.ToString()
            });

            return CreateBrand(licensee, isActive: true);
        }

        public Guid GetMainWalletGameId(Guid playerId)
        {
            return GetMainWalletGame(playerId).Id;
        }

        public Game GetMainWalletGame(Guid playerId)
        {
            var wallet = _gameRepository.Wallets.Single(a => a.PlayerId == playerId && a.Template.IsMain);
            var gameProviderId = wallet.Template.WalletTemplateGameProviders.First().GameProviderId;
            return _gameRepository.GameProviders.Single(x => x.Id == gameProviderId).Games.First();
        }

    }
}