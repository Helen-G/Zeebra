using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;
using AFT.RegoV2.Core.Brand;
using AFT.RegoV2.Core.Brand.Data;
using AFT.RegoV2.Core.Common.Brand.Data;
using AFT.RegoV2.Core.Common.Events.Brand;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Brand;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Infrastructure.DataAccess.Brand.Mappings;
using VipLevel = AFT.RegoV2.Core.Brand.Data.VipLevel;

namespace AFT.RegoV2.Infrastructure.DataAccess.Brand
{
    public class BrandRepository : DbContext, IBrandRepository, ISeedable
    {
        public const string Schema = "brand";

        static BrandRepository()
        {
            Database.SetInitializer(new BrandRepositoryInitializer());
        }

        public BrandRepository(): base("name=Default") { }

        public IDbSet<Country>                  Countries { get; set; }
        public IDbSet<Currency>                 Currencies { get; set; }
        public IDbSet<Culture>                  Cultures { get; set; }
        public IDbSet<Licensee>                 Licensees { get; set; }
        public IDbSet<Core.Brand.Data.Brand>    Brands { get; set; }
        public IDbSet<VipLevel>                 VipLevels { get; set; }
        public IDbSet<VipLevelGameProviderBetLimit>         VipLevelLimits { get; set; }
        public IDbSet<RiskLevel>                RiskLevels { get; set; }
        
        public IDbSet<WalletTemplate>           WalletTemplates { get; set; }
        public IDbSet<WalletTemplateProduct>    WalletTemplateProducts { get; set; }

        public IDbSet<ContentTranslation>       ContentTranslations { get; set; }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Configurations.Add(new LicenseeMap(Schema));
            modelBuilder.Configurations.Add(new BrandMap(Schema));
            modelBuilder.Configurations.Add(new CurrencyMap(Schema));
            modelBuilder.Configurations.Add(new CountryMap(Schema));
            modelBuilder.Configurations.Add(new CultureCodeMap(Schema));
            modelBuilder.Configurations.Add(new VipLevelMap(Schema));
            modelBuilder.Configurations.Add(new WalletTemplateMap(Schema));
            modelBuilder.Configurations.Add(new WalletTemplateProductMap(Schema));
            modelBuilder.Configurations.Add(new ContractMap(Schema));
            modelBuilder.Configurations.Add(new LicenseeProductMap(Schema));
            modelBuilder.Configurations.Add(new BrandProductMap(Schema));
            modelBuilder.Configurations.Add(new ContentTranslationMap(Schema));
            modelBuilder.Configurations.Add(new BrandCountryMap(Schema));
            modelBuilder.Configurations.Add(new BrandCultureMap(Schema));
            modelBuilder.Configurations.Add(new BrandCurrencyMap(Schema));
            modelBuilder.Configurations.Add(new VipLevelLimitMap(Schema));
            modelBuilder.Configurations.Add(new RiskLevelMap(Schema));
        }

        public override int SaveChanges()
        {
            try
            {
                return base.SaveChanges();
            }
            catch (DbEntityValidationException e)
            {
                var dbValidationErrorMessages = e.EntityValidationErrors.ToArray();
                Trace.WriteLine(dbValidationErrorMessages.ToString());
                throw;
            }
        }

        public void Seed()
        {
            var licenseeId = new Guid("4A557EA9-E6B7-4F1F-AEE5-49E170ADB7E0");
            var brand138Id = new Guid("00000000-0000-0000-0000-000000000138");
            var brand831Id = new Guid("D2CFDF01-FB79-4365-9E47-FC8E4DC1349C");

           

            AddContentTranslation("en-GB", "MainWallet-UK", "Main Wallet", "Main Balance");
            AddContentTranslation("zh-CN", "MainWallet-CN", "Main Wallet", "?????");

            AddBrand("138", "138", brand138Id, "Pacific Standard Time", PlayerActivationMethod.Automatic);
            AddBrand("831", "831", brand831Id, "Pacific Standard Time", PlayerActivationMethod.Email);

            AddVipLevel(new Guid("30e9988c-afed-49a0-be6b-ad60f7a50beb"), brand831Id, "G", "Gold",
                "High Roller", "#fad165", 0);
            AddVipLevel(new Guid("0447e567-bdc6-4330-979c-5e0984bfb626"), brand138Id, "S", "Silver",
                "Baller", "#cabdbf", 1);
            AddVipLevel(new Guid("541F60EF-AEE7-408B-9B39-90289D49F6AD"), brand138Id, "B", "Bronze",
                "Some description", "#d06b64", 2);

            MakeVipLevelDefaultForBrand(brand138Id, new Guid("0447e567-bdc6-4330-979c-5e0984bfb626"));
            MakeVipLevelDefaultForBrand(brand831Id, new Guid("30e9988c-afed-49a0-be6b-ad60f7a50beb"));

            AddLicenseeProduct(licenseeId, new Guid("321B0909-1768-42E2-8BD4-11A28CDAD039"));
            AddLicenseeProduct(licenseeId, new Guid("1814418D-BC00-43B4-AD18-BEBEF6501D7F"));
            AddBrandProduct(brand138Id, new Guid("321B0909-1768-42E2-8BD4-11A28CDAD039"));
            AddBrandProduct(brand138Id, new Guid("1814418D-BC00-43B4-AD18-BEBEF6501D7F"));

            AddWalletTemplate(brand138Id, new Guid("9D366EF4-7AEF-4DFE-80E1-045909DC8EFD"), 
                "Main 138", true,
                new Guid[] { new Guid("18FB823B-435D-42DF-867E-3BA38ED92060"), });
            AddWalletTemplate(brand138Id, new Guid("5855385F-8013-48E1-A3E4-B0BC1A49EF33"), 
                "Product 138", false,
                new Guid[] { new Guid("1814418D-BC00-43B4-AD18-BEBEF6501D7F") });
            AddWalletTemplate(brand831Id, new Guid("237D0F30-C605-4214-A37C-061AEE5FA0F4"), 
                "Main 831", true,
                new Guid[] { new Guid("1814418D-BC00-43B4-AD18-BEBEF6501D7F") });

            AddRiskLevels(brand138Id, brand831Id);
        }

        private void MakeVipLevelDefaultForBrand(Guid brandId, Guid vipLevelId)
        {
            var brand = Brands.Single(o => o.Id == brandId);
            brand.DefaultVipLevelId = vipLevelId;
            SaveChanges();
        }

        private void AddCountryCode(string code, string name)
        {
            if (Countries.Any(c => c.Code == code) == false)
            {
                Countries.Add(new Country { Code = code, Name = name });
                SaveChanges();
            }
        }

        private void AddBrand(string code, string name, Guid id, string timeZoneId,
            PlayerActivationMethod method)
        {
            if (Brands.Any(b => b.Name == name) == false)
            {
                var brand = new Core.Brand.Data.Brand
                {
                    Id = id,
                    Code = code,
                    Name = name,
                    Status = BrandStatus.Active,
                    Licensee = Licensees.First(),
                    TimezoneId = timeZoneId,
                    PlayerActivationMethod = method,
                };

                brand.BrandCultures.Add(new BrandCulture { BrandId = brand.Id, CultureCode = "en-US", DateAdded = DateTimeOffset.UtcNow, AddedBy = "System" });
                brand.BrandCultures.Add(new BrandCulture { BrandId = brand.Id, CultureCode = "zh-TW", DateAdded = DateTimeOffset.UtcNow, AddedBy = "System" });
                brand.BrandCurrencies.Add(new BrandCurrency { BrandId = brand.Id, CurrencyCode = "CAD", DateAdded = DateTimeOffset.UtcNow, AddedBy = "System", DefaultPaymentLevelId = new Guid("E1E600D4-0729-4D5C-B93E-085A94B55B33") });
                brand.BrandCurrencies.Add(new BrandCurrency { BrandId = brand.Id, CurrencyCode = "CNY", DateAdded = DateTimeOffset.UtcNow, AddedBy = "System", DefaultPaymentLevelId = new Guid("1ED97A2B-EBA2-4B68-A70C-18A7070908F9") });
                brand.BrandCountries.Add(new BrandCountry{ BrandId = brand.Id, CountryCode = "CA", DateAdded = DateTimeOffset.UtcNow, AddedBy = "System" });
                brand.BrandCountries.Add(new BrandCountry { BrandId = brand.Id, CountryCode = "CN", DateAdded = DateTimeOffset.UtcNow, AddedBy = "System" });
                brand.BrandCountries.Add(new BrandCountry { BrandId = brand.Id, CountryCode = "GB", DateAdded = DateTimeOffset.UtcNow, AddedBy = "System" });
                brand.BrandCountries.Add(new BrandCountry { BrandId = brand.Id, CountryCode = "US", DateAdded = DateTimeOffset.UtcNow, AddedBy = "System" });
                brand.DefaultCurrency = "CAD";
                brand.BaseCurrency = "CAD";
                brand.DefaultCulture = "en-US";
                brand.CurrencySetCreated = DateTime.Now;
                brand.CurrencySetCreatedBy = "system";
                brand.EmailActivationUrl = ConfigurationManager.AppSettings["MemberWebsiteUrl"] + "en-US/Home/Activate";
                Brands.Add(brand);
                SaveChanges();
            }
        }

        private void AddLicenseeProduct(Guid licenseeId, Guid productId)
        {
            var licensee = Licensees.Include(x => x.Products).Single(x => x.Id == licenseeId);
            if (licensee.Products.Any(x => x.ProductId == productId)) return;

            licensee.Products.Add(new LicenseeProduct
            {
                Licensee = licensee,
                ProductId = productId
            });
            SaveChanges();
        }

        private void AddBrandProduct(Guid brandId, Guid productId)
        {
            var brand = Brands.Include(x => x.Products).Single(x => x.Id == brandId);
            if (brand.Products.Any(x => x.ProductId == productId)) return;

            brand.Products.Add(new BrandProduct
            {
                Brand = brand,
                ProductId = productId
            });
            SaveChanges();
        }

        private void AddVipLevel(Guid vipLevelId, Guid brandId, string code, string name,
            string description, string colorCode, int rank)
        {
            if (VipLevels.Any(v => v.Id == vipLevelId))
                return;

            VipLevels.Add(new VipLevel
            {
                Id = vipLevelId,
                BrandId = brandId,
                Code = code,
                Name = name,
                Description = description,
                ColorCode = colorCode,
                Status = VipLevelStatus.Active,
                Rank = rank
            });

            SaveChanges();
        }

        private void AddWalletTemplate(Guid brandId, Guid templateId, string templateName, bool isMain, IEnumerable<Guid> ProductIds )
        {
            if (WalletTemplates.Any(v => v.Id == templateId)) return;

            WalletTemplates.Add(new WalletTemplate
            {
                Id = templateId,
                Brand = Brands.FirstOrDefault(x => x.Id == brandId),
                IsMain = isMain,
                Name = templateName,
                WalletTemplateProducts = ProductIds.Select(productId => new WalletTemplateProduct
                {
                    Id = Guid.NewGuid(),
                    ProductId = productId,
                    WalletTemplateId = templateId
                }).ToList()
            });
            SaveChanges();
        }

        private void AddContentTranslation(string languageCode, string name, string source, string translation)
        {
            if (ContentTranslations.Any(c => c.Name == name)) return;

            ContentTranslations.Add(new ContentTranslation
            {
                Created = DateTimeOffset.UtcNow,
                CreatedBy = "system",
                Id = Guid.NewGuid(),
                Language = languageCode,
                Name = name,
                Source = source,
                Status = TranslationStatus.Enabled,
                Translation = translation
            });

            SaveChanges();
        }

        private void AddRiskLevels(Guid brand138Id, Guid brand831Id)
        {
            var riskIds = new Guid[]
            {
                new Guid("5B6EA085-9661-4FA9-8391-54704040FE91"),
                new Guid("5B6EA085-9661-4FA9-8391-54704040FE92"),
                new Guid("5B6EA085-9661-4FA9-8391-54704040FE93"),
                new Guid("5B6EA085-9661-4FA9-8391-54704040FE94"),
                new Guid("5B6EA085-9661-4FA9-8391-54704040FE95")
            };

            string[] riskNames = { "VIP", "New Players", "Test Account", "Multiple Accounts", "Stolen Accounts" };

            for (int i = 0; i < 5; i++)
            {
                RiskLevels.AddOrUpdate(x => x.Id, new RiskLevel
                {
                    Id = riskIds[i],
                    Level = i + 1,
                    Name = riskNames[i],
                    Status = i % 2 == 0 ? Status.Inactive : Status.Active,
                    BrandId = i % 2 == 0 ? brand138Id : brand831Id,
                    Description = (i + 1) + " Initialized by dao"
                });
            }

            SaveChanges();
        }
    }
}