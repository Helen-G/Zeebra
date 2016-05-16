using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using AFT.RegoV2.Core.Bonus;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Brand.Data;
using AFT.RegoV2.Core.Common.Data.Brand;
using AFT.RegoV2.Core.Common.Brand.Data;
using AFT.RegoV2.Core.Common.Events.Brand;
using AFT.RegoV2.Core.Security.ApplicationServices;
using AFT.RegoV2.Core.Security.ApplicationServices.Data.Users;
using AFT.RegoV2.Core.Security.Data;
using AFT.RegoV2.Core.Security.Helpers;
using AFT.RegoV2.Core.Services.Security;
using AFT.RegoV2.Domain.BoundedContexts.Security.ApplicationServices;
using AFT.RegoV2.Domain.Payment.Data;
using AFT.RegoV2.Domain.Security.Interfaces;
using AFT.RegoV2.Shared.Constants;
using AFT.RegoV2.Tests.Common.Helpers;
using AFT.RegoV2.Tests.Common.TestDoubles;
using Microsoft.Practices.Unity;
using BrandCurrency = AFT.RegoV2.Core.Brand.Data.BrandCurrency;
using Licensee = AFT.RegoV2.Core.Brand.Data.Licensee;
using VipLevel = AFT.RegoV2.Core.Brand.Data.VipLevel;

namespace AFT.RegoV2.Tests.Common.Base
{
    public abstract class SecurityTestsBase : AdminWebsiteUnitTestsBase
    {
        protected PermissionService PermissionService;
        protected UserService UserService;
        protected RoleService RoleService;
        protected ISecurityRepository SecurityRepository;
        protected FakeSecurityRepository FakeSecurityRepository;
        protected FakeBrandRepository FakeBrandRepository;
        protected SecurityTestHelper SecurityTestHelper;
        protected BrandQueries BrandQueries;
        protected FakePaymentRepository PaymentRepository;
        protected FakePlayerRepository PlayerRepository;
        protected Core.Brand.Data.Brand Brand;
        protected Licensee Licensee;

        public override void BeforeEach()
        {
            base.BeforeEach();

            PermissionService = Container.Resolve<PermissionService>();
            UserService = Container.Resolve<UserService>();
            RoleService = Container.Resolve<RoleService>();

            SecurityRepository = Container.Resolve<ISecurityRepository>();

            FakeSecurityRepository = Container.Resolve<FakeSecurityRepository>();

            FakeBrandRepository = Container.Resolve<FakeBrandRepository>();

            PlayerRepository = Container.Resolve<FakePlayerRepository>();
            PaymentRepository = Container.Resolve<FakePaymentRepository>();

            SecurityTestHelper = Container.Resolve<SecurityTestHelper>();

            SecurityTestHelper.PopulatePermissions();

            BrandQueries = Container.Resolve<BrandQueries>();

            var brandHelper = Container.Resolve<BrandTestHelper>();

            foreach (var countryCode in TestDataGenerator.CountryCodes)
            {
                FakeBrandRepository.Countries.Add(new Country { Code = countryCode });
            }

            foreach (var currencyCode in TestDataGenerator.CurrencyCodes)
            {
                FakeBrandRepository.Currencies.Add(new Currency { Code = currencyCode });
            }

            foreach (var cultureCode in TestDataGenerator.CultureCodes.Where(x => x != null))
            {
                FakeBrandRepository.Cultures.Add(new Culture { Code = cultureCode });
            }

            Brand = CreateBrand();

            Licensee = new Licensee
            {
                Id = Guid.NewGuid(),
                AllowedBrandCount = 1,
                Status = LicenseeStatus.Active
            };

            FakeBrandRepository.Licensees.Add(Licensee);

            var licenseeIds = new[] { Licensee.Id };
            var brandIds = new[] { Brand.Id };

            const string superAdminUsername = "SuperAdmin";
            const string superAdminPassword = "SuperAdmin";

            var userId = RoleIds.SuperAdminId;
            var role = new Core.Security.Data.Role
            {
                Id = userId,
                Code = "SuperAdmin",
                Name = "SuperAdmin",
                CreatedDate = DateTime.UtcNow,
                Permissions = PermissionService.GetPermissions()
                    .Select(p => new RolePermission { PermissionId = p.Id, RoleId = userId}).ToList()
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

            SecurityRepository.Users.AddOrUpdate(user);

            SecurityRepository.SaveChanges();

            Container.Resolve<SecurityTestHelper>().SignInSuperAdmin();
        }

        protected User CreateTestUser(Guid roleId, string password = null, IEnumerable<Guid> licensees = null, ICollection<Guid> brandIds = null)
        {
            // Todo: Change that to use SecurityTestHelper.CreateUser
            var userName = "User-" + TestDataGenerator.GetRandomString(5);

            var licenseesList = licensees != null
                ? licensees.ToList()
                : BrandQueries.GetLicensees().Select(l => l.Id).ToList();
            var brandsList = brandIds != null
                ? brandIds.ToList()
                : BrandQueries.GetBrands().Select(b => b.Id).ToList();

            var userData = new AddUserData
            {
                Username = userName,
                FirstName = userName,
                LastName = userName,
                Password = password ?? TestDataGenerator.GetRandomString(),
                Language = "English",
                Status = UserStatus.Active,
                RoleId = roleId,
                AssignedLicensees = licenseesList,
                AllowedBrands = brandsList,
                Currencies = BrandQueries.GetCurrencies().Select(c => c.Code).ToList()
            };

            return UserService.CreateUser(userData);
        }

        protected Role CreateTestRole(Guid? createdBy = null, ICollection<Permission> permissions = null)
        {
            return SecurityTestHelper.CreateTestRole(createdBy, permissions);
        }

        protected Role CreateRole(IEnumerable<Guid> licencees, IList<Permission> permissions)
        {
            return SecurityTestHelper.CreateRole(licencees, permissions);
        }

        protected User CreateSuperAdmin()
        {
            // Todo: Refactor it to reuse CreateUser code (DRY)
            var id = new Guid("00000000-0000-0000-0000-000000000001");
            const string superAdmin = "SuperAdmin";

            var role = new Core.Security.Data.Role
            {
                Id = id,
                Code = superAdmin,
                Name = superAdmin,
                Permissions = PermissionService.GetPermissions()
                    .Select(p => new RolePermission {PermissionId = p.Id, RoleId = id}).ToList()
            };

            SecurityRepository.Roles.Add(role);
            SecurityRepository.SaveChanges();

            var licenseesList = BrandQueries.GetLicensees().Select(l => l.Id).ToList();
            var brandsList = BrandQueries.GetBrands().Select(b => b.Id).ToList();

            var userData = new AddUserData
            {
                Username = superAdmin,
                FirstName = superAdmin,
                LastName = superAdmin,
                Password = TestDataGenerator.GetRandomString(),
                Language = "English",
                Status = UserStatus.Active,
                RoleId = role.Id,
                AssignedLicensees = licenseesList,
                AllowedBrands = brandsList,
                Currencies = BrandQueries.GetCurrencies().Select(c => c.Code).ToList()
            };

            return UserService.CreateUser(userData);
        }

        protected Licensee CreateTestLicensee(string name)
        {
            var licensee = new Licensee
            {
                Id = Guid.NewGuid(),
                Name = name,
                DateCreated = DateTimeOffset.Now
            };

            FakeBrandRepository.Licensees.Add(licensee);
            FakeBrandRepository.SaveChanges();

            return licensee;
        }

        protected Core.Brand.Data.Brand CreateTestBrand(Licensee licensee, string code, string name)
        {
            var brand = new Core.Brand.Data.Brand
            {
                Licensee = licensee,
                Code = code,
                Name = name,
                Type = BrandType.Deposit,
                TimezoneId = TimeZoneInfo.GetSystemTimeZones().First().Id
            };

            FakeBrandRepository.Brands.Add(brand);
            FakeBrandRepository.SaveChanges();

            return brand;
        }

        protected User CreateUserWithPermissions(string category, string[] permissions)
        {
            return SecurityTestHelper.CreateUserWithPermissions(category, permissions);
        }

        private Core.Brand.Data.Brand CreateBrand()
        {
            var brand = new Core.Brand.Data.Brand
            {
                Id = Guid.NewGuid(),
                Name = "138",
                Status = BrandStatus.Active,
                Licensee = new Licensee()
                {
                    Id = new Guid(),
                    Name = TestDataGenerator.GetRandomCountryName(),
                    CreatedBy = "SuperAdmin",
                    DateCreated = DateTimeOffset.UtcNow,
                    Email = TestDataGenerator.GetRandomEmail()
                }
            };

            brand.BrandCountries = FakeBrandRepository.Countries.Select(x => new BrandCountry
            {
                BrandId = brand.Id,
                CountryCode = x.Code
            }).ToList();

            brand.BrandCultures = FakeBrandRepository.Cultures.Select(x => new BrandCulture
            {
                BrandId = brand.Id,
                CultureCode = x.Code
            }).ToList();

            brand.BrandCurrencies = FakeBrandRepository.Currencies.Select(x => new BrandCurrency
            {
                BrandId = brand.Id,
                CurrencyCode = x.Code
            }).ToList();

            var vipLevel = new Core.Player.Data.VipLevel()
            {
                BrandId = brand.Id,
                Code = "Code",
                Id = Guid.NewGuid(),
                Name = "Name",
                Status = VipLevelStatus.Active,
                Description = "Descr",
                Rank = 123
            };
            brand.DefaultVipLevelId = vipLevel.Id;
            PlayerRepository.VipLevels.Add(vipLevel);
            var brandVipLevel = new VipLevel
            {
                Id = vipLevel.Id,
                BrandId = vipLevel.BrandId,
                Code = vipLevel.Code,
                Name = vipLevel.Name,
                Status = vipLevel.Status,
                Description = vipLevel.Description,
                Rank = vipLevel.Rank
            };
            FakeBrandRepository.VipLevels.Add(brandVipLevel);
            PlayerRepository.Brands.Add(new Core.Player.Data.Brand { Id = brand.Id, DefaultVipLevelId = vipLevel.Id });

            PaymentRepository.PaymentLevels.Add(new PaymentLevel()
            {
                Id = Guid.NewGuid(),
                Name = "Name",
                CreatedBy = "SuperAdmin",
                DateCreated = DateTimeOffset.UtcNow,
                //dpl IsDefault = true,
                BrandId = brand.Id,
                Code = "code",
                CurrencyCode = "CAD"
            });

            PaymentRepository.SaveChanges();
            PlayerRepository.SaveChanges();
            FakeBrandRepository.Brands.Add(brand);
            FakeBrandRepository.SaveChanges();

            var bonusRepository = Container.Resolve<IBonusRepository>();
            bonusRepository.Brands.Add(new Core.Bonus.Data.Brand { Id = brand.Id });

            return brand;
        }
    }
}
