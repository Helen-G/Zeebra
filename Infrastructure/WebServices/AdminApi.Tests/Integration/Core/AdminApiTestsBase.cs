using System;
using System.Data.Entity.Migrations;
using System.Linq;
using AFT.RegoV2.Core.Brand.Data;
using AFT.RegoV2.Core.Common.Brand.Data;
using AFT.RegoV2.Core.Common.Data.Brand;
using AFT.RegoV2.Core.Security.ApplicationServices;
using AFT.RegoV2.Core.Security.Data;
using AFT.RegoV2.Core.Security.Helpers;
using AFT.RegoV2.Core.Services.Security;
using AFT.RegoV2.Shared.Constants;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Helpers;
using AFT.RegoV2.Tests.Common.TestDoubles;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.AdminApi.Tests.Integration.Core
{
    public class AdminApiTestsBase
    {
        protected FakeBrandRepository FakeBrandRepository { get; set; }
        protected FakeSecurityRepository FakeSecurityRepository { get; set; }
        protected PermissionService PermissionService { get; set; }

        public IUnityContainer Container { get; set; }

        public AdminApiTestsBase()
        {
            Container = new AdminApiIntegrationTestContainerFactory().CreateWithRegisteredTypes();

            FakeBrandRepository = Container.Resolve<FakeBrandRepository>();
            FakeSecurityRepository = Container.Resolve<FakeSecurityRepository>();
            PermissionService = Container.Resolve<PermissionService>();

            for (int i = 0; i < TestDataGenerator.CountryCodes.Length; i++)
            {
                FakeBrandRepository.Countries.Add(new Country { Code = TestDataGenerator.CountryCodes[i] });
            }

            for (int i = 0; i < TestDataGenerator.CurrencyCodes.Length; i++)
            {
                FakeBrandRepository.Currencies.Add(new Currency { Code = TestDataGenerator.CurrencyCodes[i] });
            }

            for (int i = 0; i < TestDataGenerator.CultureCodes.Length; i++)
            {
                FakeBrandRepository.Cultures.Add(new Culture { Code = TestDataGenerator.CultureCodes[i] });
            }

            var brandId = new Guid("00000000-0000-0000-0000-000000000138");
            var brand = new Brand { Id = brandId, Name = "138", Status = BrandStatus.Active, Code = "", Licensee = new Licensee() };
            for (int i = 0; i < TestDataGenerator.CurrencyCodes.Length; i++)
            {
                var currencyCode = TestDataGenerator.CurrencyCodes[i];

                brand.BrandCurrencies.Add(new BrandCurrency
                {
                    BrandId = brand.Id,
                    Brand = brand,
                    CurrencyCode = currencyCode,
                    Currency = FakeBrandRepository.Currencies.Single(x => x.Code == currencyCode),
                    DefaultPaymentLevelId = currencyCode == "CAD"
                        ? new Guid("E1E600D4-0729-4D5C-B93E-085A94B55B33")
                        : new Guid("1ED97A2B-EBA2-4B68-A70C-18A7070908F9")
                });

            }
            for (int i = 0; i < TestDataGenerator.CountryCodes.Length; i++)
            {
                var countryCode = TestDataGenerator.CountryCodes[i];

                brand.BrandCountries.Add(new BrandCountry
                {
                    BrandId = brand.Id,
                    Brand = brand,
                    CountryCode = countryCode,
                    Country = FakeBrandRepository.Countries.Single(x => x.Code == countryCode)
                });
            }
            for (int i = 0; i < TestDataGenerator.CultureCodes.Length; i++)
            {
                var cultureCode = TestDataGenerator.CultureCodes[i];

                brand.BrandCultures.Add(new BrandCulture
                {
                    BrandId = brand.Id,
                    Brand = brand,
                    CultureCode = cultureCode,
                    Culture = FakeBrandRepository.Cultures.Single(x => x.Code == cultureCode)
                });
            }
            var walletTemplate = new WalletTemplate()
            {
                Brand = brand,
                Id = Guid.NewGuid(),
                IsMain = true,
                Name = "Main wallet",
                DateCreated = DateTimeOffset.UtcNow,
                CreatedBy = Guid.NewGuid()
            };

            brand.WalletTemplates.Add(walletTemplate);
            brand.DefaultCulture = brand.BrandCultures.First().Culture.Code;
            brand.DefaultCurrency = brand.BrandCurrencies.First().Currency.Code;
            brand.BrandCultures.Add(new BrandCulture { BrandId = brand.Id, CultureCode = "en-US" });
            brand.BrandCultures.Add(new BrandCulture { BrandId = brand.Id, CultureCode = "zh-TW" });
            var vipLevel = new VipLevel { Name = "Standard", BrandId = brandId };
            brand.DefaultVipLevelId = vipLevel.Id;
            brand.DefaultVipLevel = vipLevel;

            FakeBrandRepository.WalletTemplates.Add(walletTemplate);
            RegoV2.Core.Player.Data.VipLevel playerVipLevel = new RegoV2.Core.Player.Data.VipLevel
            {
                Id = Guid.NewGuid(),
                Name = "Standard",
                BrandId = brandId
            };
            brand.DefaultVipLevelId = playerVipLevel.Id;

            FakeBrandRepository.Brands.Add(brand);

            var licensee = new Licensee
            {
                Id = Guid.NewGuid(),
                AllowedBrandCount = 1,
                Status = LicenseeStatus.Active
            };

            FakeBrandRepository.Licensees.Add(licensee);
            FakeBrandRepository.SaveChanges();

            var securityHelper = Container.Resolve<SecurityTestHelper>();
            securityHelper.PopulatePermissions();

            var licenseeIds = new[] { licensee.Id };
            var brandIds = new[] { brand.Id };

            const string superAdminUsername = "SuperAdmin";
            const string superAdminPassword = "SuperAdmin";

            var userId = RoleIds.SuperAdminId;
            var role = new Role
            {
                Id = userId,
                Code = "SuperAdmin",
                Name = "SuperAdmin",
                CreatedDate = DateTime.UtcNow,
                Permissions = PermissionService.GetPermissions()
                    .Select(p => new RolePermission { PermissionId = p.Id, RoleId = userId }).ToList()
            };

            role.SetLicensees(licenseeIds);

            var user = new User
            {
                Id = userId,
                Username = superAdminUsername,
                FirstName = superAdminUsername,
                LastName = superAdminUsername,
                Status = UserStatus.Active,
                Description = superAdminUsername,
                PasswordEncrypted = PasswordHelper.EncryptPassword(userId, superAdminPassword),
                Role = role
            };

            user.SetLicensees(licenseeIds);

            foreach (var licenseeId in licenseeIds)
            {
                user.LicenseeFilterSelections.Add(new LicenseeFilterSelection
                {
                    UserId = user.Id,
                    LicenseeId = licenseeId,
                    User = user
                });
            }

            user.SetAllowedBrands(brandIds);

            foreach (var item in brandIds)
            {
                user.BrandFilterSelections.Add(new BrandFilterSelection
                {
                    UserId = user.Id,
                    BrandId = item,
                    User = user
                });
            }

            FakeSecurityRepository.Users.AddOrUpdate(user);

            FakeSecurityRepository.SaveChanges();

            securityHelper.SignInSuperAdmin();
        }
    }
}
