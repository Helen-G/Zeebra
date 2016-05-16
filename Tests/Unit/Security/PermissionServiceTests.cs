using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Brand.Data;
using AFT.RegoV2.Core.Security.Helpers;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Helpers;
using AFT.RegoV2.Tests.Common.TestDoubles;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Security
{
    class PermissionServiceTests : SecurityTestsBase
    {
        private FakeBrandRepository _fakeBrandRepository;
        private FakeSecurityRepository _fakeSecurityRepository;
        private BrandQueries _brandQueries;

        public override void BeforeEach()
        {
            base.BeforeEach();

            _fakeBrandRepository = Container.Resolve<FakeBrandRepository>();
            _fakeSecurityRepository = Container.Resolve<FakeSecurityRepository>();
            _brandQueries = Container.Resolve<BrandQueries>();
        }

        [Test]
        public void Can_add_allowed_brand_to_user()
        {
            // *** Arrange ***
            var role = CreateTestRole();
            var user = CreateTestUser(role.Id);

            var brand = CreateBrand();

            // *** Act ***
            PermissionService.AddBrandToUser(user.Id, brand.Id);

            // *** Assert ***
            var createdUser = SecurityRepository.Users.Include(u => u.AllowedBrands).FirstOrDefault(u => u.Id == user.Id);

            Assert.IsNotNull(createdUser);
            Assert.IsNotNull(createdUser.AllowedBrands);
            Assert.True(createdUser.AllowedBrands.Any(b => b.Id == brand.Id));
        }

        [Test]
        public void Can_verify_operation_for_user()
        {
            // *** Arrange ***
            _fakeSecurityRepository.Permissions.Add(new Core.Security.Data.Permission
            {
                Id = Guid.NewGuid(),
                Name = "Test",
                Module = "Test"
            });

            var permission = PermissionService.GetPermission("Test", "Test");

            var role = CreateTestRole(null, new[] { permission });

            var user = CreateTestUser(role.Id);

            // *** Act ***
            var isValid = PermissionService.VerifyPermission(user.Id, "Test", "Test");
            var isInvalid = !PermissionService.VerifyPermission(user.Id, "Invalid", "Invalid");

            // *** Assert ***
            Assert.True(isValid);
            Assert.True(isInvalid);
        }

        [Test]
        public void Can_filter_licensees()
        {
            // *** Arrange ***
            var role = CreateTestRole();

            var userLicensee = CreateLicensees().First();

            var licensees = CreateLicensees(10);
            licensees.Add(userLicensee);

            var user = CreateTestUser(role.Id, licensees: new[] { userLicensee.Id });

            Container.Resolve<SecurityTestHelper>().SignInUser(user);

            // *** Act ***
            var filtered = _brandQueries.GetFilteredLicensees(licensees, user.Id);

            // *** Assert ***
            Assert.IsNotNull(filtered);
            Assert.True(filtered.Count() == 1);
            Assert.True(filtered.Any(l => l.Id == userLicensee.Id));
        }

        [Test]
        public void Can_filter_non_active_licensees()
        {
            // *** Arrange ***
            const int activeLicenseeCount = 5;
            var role = CreateTestRole();

            var activeLicensees = CreateLicensees(activeLicenseeCount);
            var inactiveLicensees = CreateLicensees(10, LicenseeStatus.Inactive);
            var deactivatedLicensee = CreateLicensees(10, LicenseeStatus.Deactivated);
            var licensees = activeLicensees.Concat(inactiveLicensees).Concat(deactivatedLicensee).ToArray();

            var user = CreateTestUser(role.Id, licensees: licensees.Select(x => x.Id).ToArray());

            Container.Resolve<SecurityTestHelper>().SignInUser(user);

            // *** Act ***
            var filtered = _brandQueries.GetFilteredLicensees(licensees, user.Id);

            // *** Assert ***
            Assert.IsNotNull(filtered);
            Assert.True(filtered.Count() == activeLicenseeCount);
        }

        [Test]
        public void Super_admin_can_access_all_licensees()
        {
            // *** Arrange ***
            const int licenseeCount = 10;
            var superAdmin = CreateSuperAdmin();
            var licensees = CreateLicensees(licenseeCount);
            Container.Resolve<SecurityTestHelper>().SignInUser(superAdmin);

            // *** Act ***
            var filtered = _brandQueries.GetFilteredLicensees(licensees, superAdmin.Id);

            // *** Assert ***
            Assert.IsNotNull(filtered);
            Assert.True(filtered.Count() == licenseeCount);
        }

        [Test]
        public void Can_filter_brands()
        {
            // *** Arrange ***
            var role = CreateTestRole();
            var user = CreateTestUser(role.Id);

            user.AllowedBrands.Clear();

            var brands = new List<Core.Brand.Data.Brand>();

            const int brandCount = 20;
            const int userBrandCount = 10;

            // Create user brands
            for (var i = 0; i < userBrandCount; i++)
            {
                var brand = CreateBrand();

                brands.Add(brand);

                PermissionService.AddBrandToUser(user.Id, brand.Id);
            }

            // Create other brands
            for (var i = 0; i < brandCount - userBrandCount; i++)
            {
                var brand = CreateBrand();

                brands.Add(brand);
            }

            Container.Resolve<SecurityTestHelper>().SignInUser(user);

            // *** Act ***
            var filtered = _brandQueries.GetFilteredBrands(brands, user.Id);

            // *** Assert ***
            Assert.IsNotNull(filtered);
            Assert.True(filtered.Count() == userBrandCount);

            var userBrandIds = user.AllowedBrands.Select(b => b.Id);
            var filteredBrandIds = filtered.Select(b => b.Id);
            var isEqual = userBrandIds.OrderBy(a => a).SequenceEqual(filteredBrandIds.OrderBy(a => a));

            Assert.True(isEqual);
        }

        [Test]
        public void Super_admin_can_access_all_brands()
        {
            // *** Arrange ***
            const int brandCount = 20;
            var brands = new List<Core.Brand.Data.Brand>();

            for (var i = 0; i < brandCount; i++)
            {
                brands.Add(CreateBrand());
            }

            var user = SecurityRepository.GetUserById(SecurityTestHelper.CurrentUser.UserId);
            var allowedBrands = brands.Select(b => b.Id).ToList();
            allowedBrands.AddRange(user.AllowedBrands.Select(b => b.Id));
            user.SetAllowedBrands(allowedBrands);
            SecurityTestHelper.SignInUser(user);

            // *** Act ***
            var filtered = _brandQueries.GetFilteredBrands(brands, user.Id);

            // *** Assert ***
            Assert.IsNotNull(filtered);
            Assert.True(filtered.Count() == brandCount);
        }

        private IList<Licensee> CreateLicensees(int count = 1, LicenseeStatus status = LicenseeStatus.Active)
        {
            var result = new List<Licensee>();
            for (var i = 0; i < count; i++)
            {
                var suffix = TestDataGenerator.GetRandomAlphabeticString(3);
                var licensee = new Licensee
                {
                    Id = Guid.NewGuid(),
                    Name = "Name" + suffix,
                    Status = status
                };

                result.Add(licensee);
            }

            return result;
        }

        public Core.Brand.Data.Brand CreateBrand()
        {
            var suffix = TestDataGenerator.GetRandomAlphabeticString(3);

            var brand = new Core.Brand.Data.Brand()
            {
                Id = Guid.NewGuid(),
                Code = "Code" + suffix,
                Name = "Name" + suffix
            };

            _fakeBrandRepository.Brands.Add(brand);
            _fakeBrandRepository.SaveChanges();

            return brand;
        }
    }
}
