using System;
using System.Linq;
using AFT.RegoV2.Core.Bonus;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Brand.Data;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Brand;
using AFT.RegoV2.Core.Common.Events.Brand;
using AFT.RegoV2.Core.Game.Data;
using AFT.RegoV2.Core.Player;
using AFT.RegoV2.Core.Security.Data;
using AFT.RegoV2.Core.Security.Helpers;
using AFT.RegoV2.Shared;
using AFT.RegoV2.Shared.Constants;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.TestDoubles;
using FluentAssertions;
using Microsoft.Practices.Unity;
using NUnit.Framework;
using Licensee = AFT.RegoV2.Core.Brand.Data.Licensee;

namespace AFT.RegoV2.Tests.Unit.Brand
{
    internal class VipLevelTests : SecurityTestsBase
    {
        private FakeBrandRepository _fakeBrandRepository;
        private FakeGameRepository _fakeGameRepository;
        private BrandCommands _brandCommands;
        private BrandQueries _brandQueries;

        public override void BeforeEach()
        {
            base.BeforeEach();

            _fakeBrandRepository = Container.Resolve<FakeBrandRepository>();
            _fakeGameRepository = Container.Resolve<FakeGameRepository>();
            _brandCommands = Container.Resolve<BrandCommands>();
            _brandQueries = Container.Resolve<BrandQueries>();

            FillFakeGamesRepository();

            _fakeBrandRepository.SaveChanges();
        }

        private void FillFakeGamesRepository()
        {
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

        [Test]
        public void Can_create_VIP_level()
        {
            var role = CreateSingleBrandManager();
            var user = CreateTestUser(role.Id);

            user.Licensees.Clear();
            user.AllowedBrands.Clear();
            user.Currencies.Clear();

            user.SetLicensees(new[] { Licensee.Id });
            user.AddAllowedBrand(Brand.Id);
            SecurityTestHelper.SignInUser(user);

            var addVipLevelCommand = CreateAddVipLevelCommand(Brand);

            _brandCommands.AddVipLevel(addVipLevelCommand);

            var vipLevel = _brandQueries.GetVipLevels().FirstOrDefault(x => x.Code == addVipLevelCommand.Code);

            Assert.That(vipLevel, Is.Not.Null);
            Assert.That(vipLevel.Name, Is.EqualTo(addVipLevelCommand.Name));
            Assert.That(vipLevel.VipLevelLimits.Count, Is.EqualTo(1));
            Assert.That(vipLevel.VipLevelLimits.First().Currency.Code, Is.EqualTo(addVipLevelCommand.Limits.First().CurrencyCode));
            Assert.That(vipLevel.VipLevelLimits.First().GameProviderId, Is.EqualTo(addVipLevelCommand.Limits.First().GameProviderId));
            //            Assert.That(vipLevel.VipLevelLimits.First().Minimum, Is.EqualTo(addVipLevelCommand.Limits.First().Minimum));
        }


        public void Can_not_create_two_default_vip_levels()
        {
            var role = CreateSingleBrandManager();
            var user = CreateTestUser(role.Id);

            user.Licensees.Clear();
            user.AllowedBrands.Clear();
            user.Currencies.Clear();

            user.SetLicensees(new[] { Licensee.Id });
            user.AddAllowedBrand(Brand.Id);
            SecurityTestHelper.SignInUser(user);

            var vipLevel = CreateAddVipLevelCommand(Brand);
            var vipLevel2 = CreateAddVipLevelCommand(Brand);

            _brandCommands.AddVipLevel(vipLevel);

            Action action = () =>_brandCommands.AddVipLevel(vipLevel2);

            action.ShouldThrow<RegoValidationException>()
                .Where(x => x.Message.Contains("Default vip level for this brand already exists"));
        }
        [Test]
        public void Can_deactivate_Default_VIP_Level()
        {
            var role = CreateSingleBrandManager();
            var user = CreateTestUser(role.Id);

            user.Licensees.Clear();
            user.AllowedBrands.Clear();
            user.Currencies.Clear();

            user.SetLicensees(new[] { Licensee.Id });
            user.AddAllowedBrand(Brand.Id);
            SecurityTestHelper.SignInUser(user);

            var newVipLevel = CreateAddVipLevelCommand(Brand);
            var defaultVipLevelId = Brand.DefaultVipLevelId;
            var newVipLevelId = _brandCommands.AddVipLevel(newVipLevel);

            Assert.DoesNotThrow(() =>
            {
                _brandCommands.DeactivateVipLevel(defaultVipLevelId.Value, "-", newVipLevelId);
            });


            var oldDefaultVipLevel = _fakeBrandRepository.VipLevels.Single(o => o.Id == defaultVipLevelId);
            var newDefaultVipLevel = _fakeBrandRepository.VipLevels.Single(o => o.Id == newVipLevelId);
            var brand = _fakeBrandRepository.Brands.Single(o => o.Id == Brand.Id);

            Assert.True(oldDefaultVipLevel.Status == VipLevelStatus.Inactive);
            Assert.True(brand.DefaultVipLevelId == newDefaultVipLevel.Id && newDefaultVipLevel.Status == VipLevelStatus.Active);
        }

        [Test]
        public void Can_activate_vip_level()
        {
            var addVipLevelCommand = CreateAddVipLevelCommand(Brand);
            var id = _brandCommands.AddVipLevel(addVipLevelCommand);

            _fakeBrandRepository.VipLevels.Single(x => x.Id == id).Status = VipLevelStatus.Inactive;

            _brandCommands.ActivateVipLevel(id, "");

            var status = _fakeBrandRepository.VipLevels.Single(x => x.Id == id).Status;

            Assert.That(status, Is.EqualTo(VipLevelStatus.Active));
        }

        private VipLevelViewModel CreateAddVipLevelCommand(Core.Brand.Data.Brand brand)
        {
            var suffix = TestDataGenerator.GetRandomAlphabeticString(5);

            var vipLevel = new VipLevelViewModel
            {
                Id = Guid.NewGuid(),
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

            return vipLevel;
        }

        private Core.Brand.Data.Brand CreateBrand(Licensee licensee)
        {
            var brand = new Core.Brand.Data.Brand
            {
                Id = Guid.Empty,
                Name = "138",
                Licensee = licensee,
                Status = BrandStatus.Active
            };

            licensee.Brands.Add(brand);

            brand.BrandCountries = _fakeBrandRepository.Countries.Select(x => new BrandCountry
            {
                BrandId = brand.Id,
                CountryCode = x.Code
            }).ToList();

            brand.BrandCultures = _fakeBrandRepository.Cultures.Select(x => new BrandCulture
            {
                BrandId = brand.Id,
                CultureCode = x.Code
            }).ToList();

            brand.BrandCurrencies = _fakeBrandRepository.Currencies.Select(x => new BrandCurrency
            {
                BrandId = brand.Id,
                CurrencyCode = x.Code
            }).ToList();

            _fakeBrandRepository.Brands.Add(brand);
            _fakeBrandRepository.SaveChanges();

            var playerRepository = Container.Resolve<IPlayerRepository>();
            playerRepository.Brands.Add(new Core.Player.Data.Brand { Id = brand.Id });

            var bonusRepository = Container.Resolve<IBonusRepository>();
            bonusRepository.Brands.Add(new Core.Bonus.Data.Brand { Id = brand.Id });

            return brand;
        }

        private Licensee CreateLicensee(int brandCount = 1)
        {
            var licensee = new Licensee
            {
                Id = Guid.NewGuid(),
                AllowedBrandCount = brandCount,
                Status = LicenseeStatus.Active
            };

            _fakeBrandRepository.Licensees.Add(licensee);
            _fakeBrandRepository.SaveChanges();

            return licensee;
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
    }
}