using System;
using AFT.RegoV2.ApplicationServices.Security;
using AFT.RegoV2.Core.Security.ApplicationServices;
using AFT.RegoV2.Core.Security.ApplicationServices.Data.Users;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Core.Security.Data;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Helpers;
using AutoMapper;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Permission
{
    class UserServicePermissionTests : PermissionsTestsBase
    {
        private UserService _userService;

        public override void BeforeEach()
        {
            base.BeforeEach();

            _userService = Container.Resolve<UserService>();
        }

        [Test]
        public void Cannot_execute_UserService_without_permissions()
        {
            /* Arrange */
            LogWithNewUser(Modules.PlayerManager, Permissions.View);

            /* Act */
            Assert.Throws<InsufficientPermissionsException>(() => _userService.GetUsers());
            Assert.Throws<InsufficientPermissionsException>(() => _userService.CreateUser(new AddUserData()));
            Assert.Throws<InsufficientPermissionsException>(() => _userService.UpdateUser(new EditUserData()));
            Assert.Throws<InsufficientPermissionsException>(() => _userService.ChangePassword(new Guid(), "password"));
            Assert.Throws<InsufficientPermissionsException>(() => _userService.Activate(new Guid()));
            Assert.Throws<InsufficientPermissionsException>(() => _userService.Deactivate(new Guid()));
        }

        [Test]
        public void Cannot_create_user_with_invalid_brand()
        {
            /*** Arrange ***/
            var brandTestHelper = Container.Resolve<BrandTestHelper>();
            var securityTestHelper = Container.Resolve<SecurityTestHelper>();

            var licensee = brandTestHelper.CreateLicensee();
            var brands = new[] { brandTestHelper.CreateBrand(licensee, isActive: true).Id, brandTestHelper.CreateBrand(licensee, isActive: true).Id };
            var currencies = new[] {brandTestHelper.CreateCurrency("CAD", "Canadian Dollar").Code};
            var role = securityTestHelper.CreateRole(new[] {licensee.Id});

            var userData = new AddUserData
            {
                Username = "User123",
                FirstName = "User",
                LastName = "123",
                Password = "Password123",
                Language = "English",
                Status = UserStatus.Active,
                AssignedLicensees = new[] {licensee.Id},
                AllowedBrands = brands,
                Currencies = currencies,
                RoleId = role.Id
            };

            LogWithNewUser(Modules.AdminManager, Permissions.Add);

            /*** Act ***/
            Assert.Throws<InsufficientPermissionsException>(() => _userService.CreateUser(userData));
        }

        [Test]
        public void Cannot_update_user_with_invalid_brand()
        {
            /*** Arrange ***/
            var brandTestHelper = Container.Resolve<BrandTestHelper>();
            var securityTestHelper = Container.Resolve<SecurityTestHelper>();

            var licensee = brandTestHelper.CreateLicensee();
            var brands = new[] { brandTestHelper.CreateBrand(licensee, isActive: true).Id, brandTestHelper.CreateBrand(licensee, isActive: true).Id };
            var currencies = new[] { brandTestHelper.CreateCurrency("CAD", "Canadian Dollar").Code };
            var role = securityTestHelper.CreateRole(new[] { licensee.Id });

            var addUserData = new AddUserData
            {
                Username = "User123",
                FirstName = "User",
                LastName = "123",
                Password = "Password123",
                Language = "English",
                Status = UserStatus.Active,
                AssignedLicensees = new[] { licensee.Id },
                AllowedBrands = brands,
                Currencies = currencies,
                RoleId = role.Id
            };

            var user = _userService.CreateUser(addUserData);

            var editUserData = Mapper.DynamicMap<EditUserData>(addUserData);
            editUserData.Id = user.Id;

            LogWithNewUser(Modules.AdminManager, Permissions.Edit);

            /*** Act ***/
            Assert.Throws<InsufficientPermissionsException>(() => _userService.UpdateUser(editUserData));
        }

        [Test]
        public void Cannot_change_password_with_invalid_brand()
        {
            /*** Arrange ***/
            var brandTestHelper = Container.Resolve<BrandTestHelper>();
            var securityTestHelper = Container.Resolve<SecurityTestHelper>();

            var licensee = brandTestHelper.CreateLicensee();
            var brands = new[] { brandTestHelper.CreateBrand(licensee, isActive: true), brandTestHelper.CreateBrand(licensee, isActive: true) };
            var user = securityTestHelper.CreateUser(licensee.Id, brands);

            LogWithNewUser(Modules.AdminManager, Permissions.Edit);

            /*** Act ***/
            Assert.Throws<InsufficientPermissionsException>(() => _userService.ChangePassword(user.Id, "password"));
        }

        [Test]
        public void Cannot_activate_user_with_invalid_brand()
        {
            /*** Arrange ***/
            var brandTestHelper = Container.Resolve<BrandTestHelper>();
            var securityTestHelper = Container.Resolve<SecurityTestHelper>();

            var licensee = brandTestHelper.CreateLicensee();
            var brands = new[] { brandTestHelper.CreateBrand(licensee, isActive: true), brandTestHelper.CreateBrand(licensee, isActive: true) };
            var user = securityTestHelper.CreateUser(licensee.Id, brands);

            LogWithNewUser(Modules.AdminManager, Permissions.Edit);

            /*** Act ***/
            Assert.Throws<InsufficientPermissionsException>(() => _userService.Activate(user.Id));
        }

        [Test]
        public void Cannot_deactivate_user_with_invalid_brand()
        {
            /*** Arrange ***/
            var brandTestHelper = Container.Resolve<BrandTestHelper>();
            var securityTestHelper = Container.Resolve<SecurityTestHelper>();

            var licensee = brandTestHelper.CreateLicensee();
            var brands = new[] { brandTestHelper.CreateBrand(licensee, isActive: true), brandTestHelper.CreateBrand(licensee, isActive: true) };
            var user = securityTestHelper.CreateUser(licensee.Id, brands);

            LogWithNewUser(Modules.AdminManager, Permissions.Edit);

            /*** Act ***/
            Assert.Throws<InsufficientPermissionsException>(() => _userService.Deactivate(user.Id));
        }
    }
}
