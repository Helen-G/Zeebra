using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Core.Security.ApplicationServices.Data.Users;
using AFT.RegoV2.Core.Security.Data;
using AFT.RegoV2.Shared;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AutoMapper;
using FluentAssertions;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Security
{
    class UserServiceTests : SecurityTestsBase
    {
        [Test]
        public void Can_create_user()
        {
            var role = CreateTestRole();
            var user = CreateTestUser(role.Id);

            Assert.IsNotNull(user);
            Assert.False(user.Id == Guid.Empty);
        }

        private AddUserData CreateAddUserData(IEnumerable<Guid> licensees = null, IEnumerable<Guid> brandIds = null)
        {
            var role = CreateTestRole();
            var userName = "User-" + TestDataGenerator.GetRandomString(5);

            var userData = new AddUserData
            {
                Username = userName,
                FirstName = userName,
                LastName = userName,
                Password = TestDataGenerator.GetRandomString(),
                Language = "English",
                Status = UserStatus.Active,
                RoleId = role.Id,
                AssignedLicensees = licensees != null ? licensees.ToList() : null,
                AllowedBrands = brandIds != null ? brandIds.ToList() : null
            };

            return userData;
        }

        [Test]
        public void Cannot_create_user_without_licensees()
        {
            /*** Arrange ***/
            var userData = CreateAddUserData();

            /*** Act ***/
            Action action = () => UserService.CreateUser(userData);
            var exception = Assert.Catch<RegoValidationException>(() => UserService.CreateUser(userData));

            action.ShouldThrow<RegoValidationException>()
                .Where(x => x.Message.Contains("licenseesRequired"));
        }

        [Test]
        public void Cannot_create_user_without_brands()
        {
            /*** Arrange ***/
            var licensees = BrandQueries.GetLicensees().Select(l => l.Id);
            var userData = CreateAddUserData(licensees);

            /*** Act ***/
            Action action = () => UserService.CreateUser(userData);

            /*** Assert ***/
            action.ShouldThrow<RegoValidationException>()
                .Where(x => x.Message.Contains("brandsRequired"));
        }

        [Test]
        public void Can_update_user()
        {
            var role = CreateTestRole();
            var user = CreateTestUser(role.Id, null, new[] {Licensee.Id}, new[] {Brand.Id});

            var firstName = TestDataGenerator.GetRandomString();
            var lastName = TestDataGenerator.GetRandomString();
            var status = UserStatus.Active;
            var language = TestDataGenerator.GetRandomString();
            var description = TestDataGenerator.GetRandomString();

            user.FirstName = firstName;
            user.LastName = lastName;
            user.Status = status;
            user.Language = language;
            user.Description = description;

            var userData = Mapper.DynamicMap<EditUserData>(user);

            userData.AllowedBrands = user.AllowedBrands.Select(b => b.Id).ToList();
            userData.AssignedLicensees = user.Licensees.Select(l => l.Id).ToList();
            userData.Password = TestDataGenerator.GetRandomString();

            UserService.UpdateUser(userData);

            user = UserService.GetUserById(user.Id);

            Assert.True(user.FirstName == firstName);
            Assert.True(user.LastName == lastName);
            Assert.True(user.Status == status);
            Assert.True(user.Language == language);
            Assert.True(user.Description == description);
        }

        [Test]
        public void Cannot_update_user_without_licensees()
        {
            /*** Arrange ***/
            var role = CreateTestRole();
            var user = CreateTestUser(role.Id);

            var userData = Mapper.DynamicMap<EditUserData>(user);

            userData.AllowedBrands = new[] {Brand.Id};
            userData.AssignedLicensees = null;
            userData.Password = TestDataGenerator.GetRandomString();

            /*** Act ***/
            Action action = () => UserService.UpdateUser(userData);

            /*** Assert ***/
            action.ShouldThrow<RegoValidationException>()
                .Where(x => x.Message.Contains("licenseesRequired"));
        }

        [Test]
        public void Cannot_update_user_without_brands()
        {
            /*** Arrange ***/
            var role = CreateTestRole();
            var user = CreateTestUser(role.Id);

            var userData = Mapper.DynamicMap<EditUserData>(user);

            userData.AssignedLicensees = user.Licensees.Select(l => l.Id).ToList();
            userData.Password = TestDataGenerator.GetRandomString();
            userData.AllowedBrands = null;

            /*** Act ***/
            Action action = () => UserService.UpdateUser(userData);

            /*** Assert ***/
            action.ShouldThrow<RegoValidationException>()
                .Where(x => x.Message.Contains("brandsRequired"));
        }

        [Test]
        public void Can_change_password()
        {
            var role = CreateTestRole();
            var user = CreateTestUser(role.Id);

            var password1 = user.PasswordEncrypted;

            user = UserService.ChangePassword(user.Id, TestDataGenerator.GetRandomString());

            var password2 = user.PasswordEncrypted;

            Assert.False(password1 == password2);
        }

        [Test]
        public void Can_activate()
        {
            // *** Arrange ***
            var role = CreateTestRole();
            var user = CreateTestUser(role.Id);

            // *** Act ***
            UserService.Activate(user.Id);

            // *** Assert ***
            var activatedUser = UserService.GetUserById(user.Id);

            Assert.IsNotNull(activatedUser);
            Assert.True(activatedUser.Status == UserStatus.Active);
        }

        [Test]
        public void Can_deactivate()
        {
            // *** Arrange ***
            var role = CreateTestRole();
            var user = CreateTestUser(role.Id);

            UserService.Activate(user.Id);

            // *** Act ***
            UserService.Deactivate(user.Id);

            // *** Assert ***
            var deactivatedUser = UserService.GetUserById(user.Id);

            Assert.IsNotNull(deactivatedUser);
            Assert.True(deactivatedUser.Status == UserStatus.Inactive);
        }

        [Test]
        public void Can_validate_login()
        {
            var password = TestDataGenerator.GetRandomString();
            var role = CreateTestRole();
            var user = CreateTestUser(role.Id, password);

            var valid1 = UserService.ValidateLogin(user.Username, TestDataGenerator.GetRandomString(6));
            var valid2 = UserService.ValidateLogin(user.Username, password);

            Assert.False(valid1);
            Assert.True(valid2);
        }
    }
}
