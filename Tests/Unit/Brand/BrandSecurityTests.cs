using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Brand.Data;
using AFT.RegoV2.Core.Common.Brand.Data;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Brand;
using AFT.RegoV2.Core.Game.Data;
using AFT.RegoV2.Core.Security.Data;
using AFT.RegoV2.Core.Security.Helpers;
using AFT.RegoV2.Shared.Constants;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Helpers;
using AFT.RegoV2.Tests.Common.TestDoubles;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Brand
{
    class BrandSecurityTests : SecurityTestsBase
    {
        private FakeBrandRepository _fakeBrandRepository;
        private BrandCommands _brandCommands;
        private BrandQueries _brandQueries;
        private FakeGameRepository _fakeGameRepository;

        public override void BeforeEach()
        {
            base.BeforeEach();

            _fakeBrandRepository = Container.Resolve<FakeBrandRepository>();
            _brandCommands = Container.Resolve<BrandCommands>();
            _brandQueries = Container.Resolve<BrandQueries>();

            foreach (var countryCode in TestDataGenerator.CountryCodes)
            {
                _fakeBrandRepository.Countries.Add(new Country { Code = countryCode });
            }

            foreach (var cultureCode in TestDataGenerator.CultureCodes.Where(x => x != null))
            {
                _fakeBrandRepository.Cultures.Add(new Culture { Code = cultureCode });
            }

            _fakeGameRepository = Container.Resolve<FakeGameRepository>();
            _fakeGameRepository.GameProviderConfigurations.Add(new GameProviderConfiguration
            {
                Id = Guid.NewGuid(),
                Name = "name" + TestDataGenerator.GetRandomAlphabeticString(5)
            });

            _fakeGameRepository.GameProviders.Add(new GameProvider()
            {
                Id = Guid.NewGuid(),
                Name = TestDataGenerator.GetRandomAlphabeticString(6),
                GameProviderConfigurations = _fakeGameRepository.GameProviderConfigurations.ToList()
            });

            _fakeGameRepository.BetLimits.Add(new GameProviderBetLimit()
            {
                GameProviderId = _fakeGameRepository.GameProviders.First().Id,
                Id = Guid.NewGuid(),
                Code = TestDataGenerator.GetRandomAlphabeticString(5)
            });
        }

        public Core.Brand.Data.Brand CreateBrand(AFT.RegoV2.Core.Brand.Data.Licensee licensee)
        {
            var suffix = TestDataGenerator.GetRandomAlphabeticString(3);

            var brand = new Core.Brand.Data.Brand()
            {
                Id = Guid.NewGuid(),
                Code = "Code" + suffix,
                Name = "Name" + suffix,
                Licensee = licensee,
                Status = BrandStatus.Active
            };

            licensee.Brands.Add(brand);

            _fakeBrandRepository.Brands.Add(brand);
            _fakeBrandRepository.SaveChanges();

            return brand;
        }

        public Core.Brand.Data.Licensee CreateLicensee(int brandCount = 1)
        {
            var licensee = new Core.Brand.Data.Licensee
            {
                Id = Guid.NewGuid(),
                AllowedBrandCount = brandCount,
                Status = LicenseeStatus.Active
            };

            _fakeBrandRepository.Licensees.Add(licensee);
            _fakeBrandRepository.SaveChanges();

            return licensee;
        }

        [Test]
        public void Cannot_access_single_licensee_brands_that_are_not_allowed_for_user()
        {
            /*** Arrange ***/
            var licensee = CreateLicensee(15);

            var notAllowedBrands = new List<Guid>();
            var allowedBrands = new List<Guid>();

            // Generate 10 brands that are not allowed to user
            for (var i = 0; i < 10; i++)
            {
                var brand = CreateBrand(licensee);
                notAllowedBrands.Add(brand.Id);
            }

            var currentUser = SecurityRepository.GetUserById(SecurityTestHelper.CurrentUser.UserId);
            var currentUserAllowedBrands = currentUser.AllowedBrands.Select(b => b.Id).ToList();
            currentUserAllowedBrands.AddRange(notAllowedBrands);
            currentUser.SetAllowedBrands(currentUserAllowedBrands);
            SecurityTestHelper.SignInUser(currentUser);

            var role = CreateTestRole();
            var user = CreateTestUser(role.Id);

            user.Licensees.Clear();
            user.AllowedBrands.Clear();

            // Generate 5 brands that are allowed to user
            for (var i = 0; i < 5; i++)
            {
                var brand = CreateBrand(licensee);
                user.AddAllowedBrand(brand.Id);
                allowedBrands.Add(brand.Id);
            }

            Container.Resolve<SecurityTestHelper>().SignInUser(user);

            /*** Act ***/
            var brands = _brandQueries.GetAllBrands();

            /*** Assert ***/
            Assert.AreEqual(brands.Count(), allowedBrands.Count);
            // Check if filtered brands are same with allowed brands
            Assert.True(!brands.Select(b => b.Id).Except(allowedBrands).Any());
            // Check if there are no forbidden brands among filtered for user
            Assert.False(brands.Select(b => b.Id).Intersect(notAllowedBrands).Any());
        }

        [Test]
        public void Cannot_access_multiple_licensees_brands_that_are_not_allowed_for_user()
        {
            var notAllowedBrands = new List<Guid>();
            var allowedBrands = new List<Guid>();

            // Generate 10 brands that are not allowed to user
            for (var i = 0; i < 10; i++)
            {
                // Generate licensee for brand
                var licensee = CreateLicensee();

                var brand = CreateBrand(licensee);
                notAllowedBrands.Add(brand.Id);
            }

            var currentUser = SecurityRepository.GetUserById(SecurityTestHelper.CurrentUser.UserId);
            var currentUserAllowedBrands = currentUser.AllowedBrands.Select(b => b.Id).ToList();
            currentUserAllowedBrands.AddRange(notAllowedBrands);
            currentUser.SetAllowedBrands(currentUserAllowedBrands);
            SecurityTestHelper.SignInUser(currentUser);

            var role = CreateTestRole();
            var user = CreateTestUser(role.Id);

            user.Licensees.Clear();
            user.AllowedBrands.Clear();

            // Generate 5 brands that are allowed to user
            for (var i = 0; i < 5; i++)
            {
                var licensee = CreateLicensee();

                var brand = CreateBrand(licensee);
                user.AddAllowedBrand(brand.Id);
                allowedBrands.Add(brand.Id);
            }

            Container.Resolve<SecurityTestHelper>().SignInUser(user);

            /*** Act ***/
            var brands = _brandQueries.GetAllBrands();

            /*** Assert ***/
            Assert.AreEqual(brands.Count(), allowedBrands.Count);
            // Check if filtered brands are same with allowed brands
            Assert.True(!brands.Select(b => b.Id).Except(allowedBrands).Any());
            // Check if there are no forbidden brands among filtered for user
            Assert.False(brands.Select(b => b.Id).Intersect(notAllowedBrands).Any());
        }

        private VipLevelViewModel CreateAddVipLevelCommand(Core.Brand.Data.Brand brand)
        {
            _fakeBrandRepository.SaveChanges();

            var suffix = TestDataGenerator.GetRandomAlphabeticString(5);

            var vipLevel = new VipLevelViewModel
            {
                Brand = brand.Id,
                Code = "code" + suffix,
                Name = "name" + suffix,
                Description = "description" + suffix,
                Rank = TestDataGenerator.GetRandomNumber(1000),
                Color = TestDataGenerator.GetRandomColor(),
                Limits = new[]
                {
                    new VipLevelLimitViewModel
                    {
                        GameProviderId = _fakeGameRepository.GameProviders.First().Id,
                        CurrencyCode = "CAD",
                        BetLimitId = _fakeGameRepository.BetLimits.First().Id
                    }
                }
            };

            while (_brandQueries.GetAllVipLevels().Any(x => x.Brand.Id == brand.Id && x.Rank == vipLevel.Rank))
            {
                vipLevel.Rank = TestDataGenerator.GetRandomNumber(1000);
            }

            return vipLevel;
        }

        private Role CreateSingleBrandManager()
        {
            var role = new Role
            {
                Id = RoleIds.SingleBrandManagerId,
                Code = "SingleBrand",
                Name = "SingleBrand",
                CreatedDate = DateTime.UtcNow,
                Permissions = PermissionService.GetPermissions()
                    .Select(p => new RolePermission { PermissionId = p.Id, RoleId = RoleIds.SingleBrandManagerId }).ToList()
            };

            FakeSecurityRepository.Roles.Add(role);
            FakeSecurityRepository.SaveChanges();

            return role;
        }

        [Test]
        public void Cannot_access_vip_levels_that_are_not_allowed_to_user()
        {
            /*** Arrange ***/
            var licensee = CreateLicensee(2);

            var role = CreateSingleBrandManager();
            var user = CreateTestUser(role.Id);

            user.Licensees.Clear();
            user.AllowedBrands.Clear();
            user.Currencies.Clear();

            user.SetLicensees(new[] { licensee.Id });

            // Create 10 vip levels for brand that is restricted to user
            var restrictedBrand = CreateBrand(licensee);
            user.AddAllowedBrand(restrictedBrand.Id);
            SecurityTestHelper.SignInUser(user);

            for (var i = 0; i < 10; i++)
            {
                var addVipLevelCommand = CreateAddVipLevelCommand(restrictedBrand);
                _brandCommands.AddVipLevel(addVipLevelCommand);
                var vipLevel = _brandQueries.GetVipLevels().FirstOrDefault(x => x.Code == addVipLevelCommand.Code);
                restrictedBrand.VipLevels.Add(vipLevel);
            }

            // Create 10 vip levels for brand that is allowed to user
            user.AllowedBrands.Clear();
            var allowedBrand = CreateBrand(licensee);
            user.AddAllowedBrand(allowedBrand.Id);
            SecurityTestHelper.SignInUser(user);

            for (var i = 0; i < 10; i++)
            {
                var addVipLevelCommand = CreateAddVipLevelCommand(allowedBrand);
                _brandCommands.AddVipLevel(addVipLevelCommand);
                var vipLevel = _brandQueries.GetVipLevels().FirstOrDefault(x => x.Code == addVipLevelCommand.Code);
                allowedBrand.VipLevels.Add(vipLevel);
            }

            /*** Act ***/
            var vipLevels = _brandQueries.GetAllVipLevels();

            /*** Assert ***/
            Assert.IsNotNull(vipLevels);
            Assert.IsNotEmpty(vipLevels);
            // Check if filtered vip levels are same with vip levels of allowed brands
            Assert.True(!vipLevels.Select(v => v.Id)
                .Except(
                    allowedBrand.VipLevels.Select(v => v.Id))
                .Any());
            // Check if there are no restricted vip levels brands among filtered for user
            Assert.False(vipLevels.Select(v => v.Id)
                .Intersect(
                    restrictedBrand.VipLevels.Select(v => v.Id))
                .Any());
        }

        [Test]
        public void Cannot_access_licensees_that_are_not_allowed_to_user()
        {
            /*** Arrange ***/
            var role = CreateTestRole();
            var user = CreateTestUser(role.Id);

            var allowedLicensees = new List<Guid>();

            for (var i = 0; i < 10; i++)
            {
                var licensee = CreateLicensee();
                allowedLicensees.Add(licensee.Id);
            }

            user.SetLicensees(allowedLicensees);

            var restrictedLicensees = new List<Guid>();

            for (var i = 0; i < 10; i++)
            {
                var licensee = CreateLicensee();
                restrictedLicensees.Add(licensee.Id);
            }

            /*** Act ***/
            Container.Resolve<SecurityTestHelper>().SignInUser(user);

            var filteredLicensees = _brandQueries.GetAllLicensees();

            /*** Assert ***/
            Assert.IsNotNull(filteredLicensees);
            Assert.IsNotEmpty(filteredLicensees);
            // Check if filtered licensees are same with allowed ones
            Assert.True(!filteredLicensees.Select(l => l.Id)
                .Except(allowedLicensees)
                .Any());
            // Check if there are no restricted licensees among filtered for user
            Assert.False(filteredLicensees.Select(l => l.Id)
                .Intersect(restrictedLicensees)
                .Any());
        }

        [Test]
        public void Cannot_access_countries_that_are_not_allowed_to_user()
        {
            /*** Arrange ***/
            var role = CreateTestRole();
            var user = CreateTestUser(role.Id);

            var allowedLicensee = CreateLicensee();
            var allowedBrand = CreateBrand(allowedLicensee);

            _fakeBrandRepository.Brands.Single(x => x.Id == Brand.Id).BrandCountries.Clear();

            var allowedCountry = _fakeBrandRepository.Countries.Single(c => c.Code == "US");
            allowedLicensee.Countries.Add(allowedCountry);
            allowedBrand.BrandCountries.Add(new BrandCountry 
            {
                BrandId = allowedBrand.Id, 
                Country = allowedCountry, 
                CountryCode = allowedCountry.Code 
            });

            user.SetLicensees(new[] { allowedLicensee.Id });

            var restrictedLicensee = CreateLicensee();
            var restrictedBrand = CreateBrand(restrictedLicensee);

            var restrictedCountry = _fakeBrandRepository.Countries.Single(c => c.Code == "CN");
            restrictedLicensee.Countries.Add(restrictedCountry);
            restrictedBrand.BrandCountries.Add(new BrandCountry
            {
                BrandId = restrictedBrand.Id, 
                Country = restrictedCountry, 
                CountryCode = restrictedCountry.Code
            });

            /*** Act ***/
            Container.Resolve<SecurityTestHelper>().SignInUser(user);

            var filteredAllowedCountries = _brandQueries.GetAllBrandCountries().Where(c => c.BrandId == allowedBrand.Id);
            var filteredRestrictedCountries = _brandQueries.GetAllBrandCountries().Where(c => c.BrandId == restrictedBrand.Id);

            var filteredAllowedBrandCountries = _brandQueries.GetAllBrandCountries().Where(c => c.BrandId == allowedBrand.Id);
            var filteredRestrictedBrandCountries = _brandQueries.GetAllBrandCountries().Where(c => c.BrandId == restrictedBrand.Id);

            /*** Assert ***/
            Assert.IsNotNull(filteredAllowedCountries);
            Assert.IsNotEmpty(filteredAllowedCountries);
            Assert.AreEqual(filteredAllowedCountries.Count(), 1);
            Assert.True(filteredAllowedCountries.Any(ac => ac.CountryCode == allowedCountry.Code));

            Assert.IsNotNull(filteredRestrictedCountries);
            Assert.IsEmpty(filteredRestrictedCountries);

            Assert.IsNotNull(filteredAllowedBrandCountries);
            Assert.IsNotEmpty(filteredAllowedBrandCountries);
            Assert.AreEqual(filteredAllowedBrandCountries.Count(), 1);
            Assert.True(filteredAllowedBrandCountries.Any(ac => ac.CountryCode == allowedCountry.Code));

            Assert.IsNotNull(filteredRestrictedBrandCountries);
            Assert.IsEmpty(filteredRestrictedBrandCountries);
        }

        [Test]
        public void Cannot_access_cultures_that_are_not_allowed_to_user()
        {
            /*** Arrange ***/
            var role = CreateTestRole();
            var user = CreateTestUser(role.Id);

            var allowedLicensee = CreateLicensee();
            var allowedBrand = CreateBrand(allowedLicensee);

            _fakeBrandRepository.Brands.Single(x => x.Id == Brand.Id).BrandCultures.Clear();

            var allowedCulture = _fakeBrandRepository.Cultures.Single(c => c.Code == "en-US");
            allowedLicensee.Cultures.Add(allowedCulture);
            allowedBrand.BrandCultures.Add(new BrandCulture
            {
                BrandId = allowedBrand.Id, 
                Culture = allowedCulture,
                CultureCode = allowedCulture.Code
            });

            user.SetLicensees(new[] { allowedLicensee.Id });

            var restrictedLicensee = CreateLicensee();
            var restrictedBrand = CreateBrand(restrictedLicensee);

            var restrictedCulture = _fakeBrandRepository.Cultures.Single(c => c.Code == "zh-TW");
            restrictedLicensee.Cultures.Add(restrictedCulture);
            restrictedBrand.BrandCultures.Add(new BrandCulture
            {
                BrandId = restrictedBrand.Id, 
                Culture = restrictedCulture,
                CultureCode = restrictedCulture.Code
            });

            /*** Act ***/
            Container.Resolve<SecurityTestHelper>().SignInUser(user);

            var filteredAllowedCultures = _brandQueries.GetAllBrandCultures().Where(c => c.BrandId == allowedBrand.Id);
            var filteredRestrictedCultures = _brandQueries.GetAllBrandCultures().Where(c => c.BrandId == restrictedBrand.Id);

            var filteredAllowedBrandCultures = _brandQueries.GetAllBrandCultures().Where(c => c.BrandId == allowedBrand.Id);
            var filteredRestrictedBrandCultures = _brandQueries.GetAllBrandCultures().Where(c => c.BrandId == restrictedBrand.Id);

            /*** Assert ***/
            Assert.IsNotNull(filteredAllowedCultures);
            Assert.IsNotEmpty(filteredAllowedCultures);
            Assert.AreEqual(filteredAllowedCultures.Count(), 1);
            Assert.True(filteredAllowedCultures.Any(ac => ac.CultureCode == allowedCulture.Code));

            Assert.IsNotNull(filteredRestrictedCultures);
            Assert.IsEmpty(filteredRestrictedCultures);

            Assert.IsNotNull(filteredAllowedBrandCultures);
            Assert.IsNotEmpty(filteredAllowedBrandCultures);
            Assert.AreEqual(filteredAllowedBrandCultures.Count(), 1);
            Assert.True(filteredAllowedBrandCultures.Any(ac => ac.CultureCode == allowedCulture.Code));

            Assert.IsNotNull(filteredRestrictedBrandCultures);
            Assert.IsEmpty(filteredRestrictedBrandCultures);
        }
    }
}