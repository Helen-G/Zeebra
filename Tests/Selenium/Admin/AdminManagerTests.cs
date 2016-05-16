using System;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Web.Configuration;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Brand.Data;
using AFT.RegoV2.Core.Security.Data;
using AFT.RegoV2.Core.Services.Security;
using AFT.RegoV2.Domain.Security.Interfaces;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Extensions;
using AFT.RegoV2.Tests.Common.Helpers;
using AFT.RegoV2.Tests.Common.Pages.BackEnd;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Selenium
{
    internal class AdminManagerTests : SeleniumBaseForAdminWebsite
    {
        private AdminManagerPage _adminManagerPage;
        private AdminWebsiteLoginPage _loginPage;
        private DashboardPage _dashboardPage;
        private User _userData;
        private Brand _brand;
        private readonly string _userPassword = TestDataGenerator.GetRandomString(6);
        private  SecurityTestHelper _securityTestHelper;
        private BrandTestHelper _brandTestHelper;
        private const string DefaultLicensee = "Flycow";
        private const string DefaultBrand = "138";
        private Guid _defaultLicenseeId;
        private Licensee _licensee;
        private Role _role;
        

        public override void BeforeAll()
        {
            base.BeforeAll();
            
            // create a brand for default licensee 
            _brandTestHelper = _container.Resolve<BrandTestHelper>();
            var brandQueries = _container.Resolve<BrandQueries>();
            _licensee = brandQueries.GetLicensees().First(x => x.Name == DefaultLicensee);
            _defaultLicenseeId = brandQueries.GetLicensees().First(x => x.Name == DefaultLicensee).Id;
            _brand = _brandTestHelper.CreateBrand(_licensee);
            
            // create a role
            _securityTestHelper = _container.Resolve<SecurityTestHelper>();
            _role = _securityTestHelper.CreateRole(new[] {_defaultLicenseeId});
            
            // create a user
            _userData = _securityTestHelper.CreateUser(_defaultLicenseeId, new[] { _brand }, new[]{"CNY"}, _userPassword, _role.Id);
        }

        public override void BeforeEach()
        {
            base.BeforeEach();
            _driver.Logout();
            _dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();
            _dashboardPage.BrandFilter.SelectAll();
            _adminManagerPage = _dashboardPage.Menu.ClickAdminManagerMenuItem();
        }

        [Test]
        public void  Can_create_a_user()
        {
            var newUserForm = _adminManagerPage.OpenNewUserForm();
            var validUserRegistrationData = TestDataGenerator.CreateValidAdminUserRegistrationData(
                "Licensee", "Active", DefaultLicensee, _brand.Name, "CAD");
            var submittedUserForm = newUserForm.Submit(validUserRegistrationData);

            Assert.AreEqual("User has been successfully created", submittedUserForm.ConfirmationMessage);
            Assert.AreEqual(validUserRegistrationData.UserName, submittedUserForm.Username);
            Assert.AreEqual("Licensee", submittedUserForm.UserRole);
            Assert.AreEqual(validUserRegistrationData.FirstName, submittedUserForm.FirstName);
        }

        [Test]
        public void Can_edit_user_details()
        {
            // create a user
            var user = _securityTestHelper.CreateUser(_defaultLicenseeId, new[] { _brand });

            //edit user details
            ResetAdminPageAndFilter();

            var editUserForm = _adminManagerPage.OpenEditUserForm(user.Username);
            editUserForm.ClearFieldsOnForm();
            var editAdminUserData = TestDataGenerator.EditAdminUserData(
                user.Username, user.Role.ToString(), user.FirstName, user.LastName, "Active", DefaultLicensee, 
                brand:"831", currency:"ALL", description:"updated"
              );
            var submittedForm = editUserForm.SubmitEditedData(editAdminUserData);

            Assert.AreEqual("User has been successfully updated", submittedForm.ConfirmationMessage);
            Assert.AreEqual(editAdminUserData.UserName, submittedForm.Username);
            Assert.AreEqual(editAdminUserData.FirstName, submittedForm.FirstName);
            Assert.AreEqual(editAdminUserData.LastName, submittedForm.LastName);
            Assert.AreEqual(editAdminUserData.Status, submittedForm.Status);
            Assert.AreEqual(editAdminUserData.Brand, submittedForm.EditedBrand);
            Assert.AreEqual(editAdminUserData.Currency, submittedForm.Currency);
            Assert.AreEqual(editAdminUserData.Description, submittedForm.Description);
        }

        [Test]
        public void Can_login_as_activated_user()
        {                       
            _dashboardPage = _driver.LoginToAdminWebsiteAs(_userData.Username, _userPassword);
            
            Assert.AreEqual(_dashboardPage.Url.ToString(), new Uri(WebConfigurationManager.AppSettings["AdminWebsiteUrl"]).ToString());
            Assert.AreEqual(_userData.Username, _dashboardPage.Username);
        }

        [Test]
        public void Cannot_login_as_deactivated_user()
        {
            // create a user in app service layer
            var userData = _securityTestHelper.CreateUser(_defaultLicenseeId, new[] { _brand }, null, _userPassword);

            ResetAdminPageAndFilter();

            _adminManagerPage.SelectAndDeactivateUser(userData.Username);
            _loginPage = new AdminWebsiteLoginPage(_driver);
            _loginPage.LoginAsDeactivatedUser(userData.Username, _userPassword);
            var errorMsg = _loginPage.GetUsernameErrorMsg();

            Assert.That(errorMsg, Is.StringContaining("Incorrect Username or Password"));
            Assert.That(_driver.Uri().ToString(), Is.StringContaining(_loginPage.Url.ToString()));
        }

        [Test]
        public void Can_view_user_details()
        {
            var userData = TestDataGenerator.CreateValidAdminUserRegistrationData(
                "CSOfficer", "Inactive", DefaultLicensee, _brand.Name, "CNY");

            ResetAdminPageAndFilter();

            var newUserForm = _adminManagerPage.OpenNewUserForm();
            var submittedUserForm = newUserForm.Submit(userData);
            submittedUserForm.SwitchToList();
            
            var viewUserForm = _adminManagerPage.OpenViewUserForm(userData.UserName);
            
            Assert.AreEqual("View User", viewUserForm.TabName);
            Assert.AreEqual(userData.UserName, viewUserForm.UserName);
            Assert.AreEqual(userData.Role, viewUserForm.UserRole);
            Assert.AreEqual(userData.FirstName, viewUserForm.FirstName);
            Assert.AreEqual(userData.LastName, viewUserForm.LastName);
            Assert.AreEqual(userData.Currency, viewUserForm.UserCurrencies);
        }

        [Test]
        public void Cannot_create_user_with_data_exceeding_max_length()
        {
            var validUserRegistrationData = TestDataGenerator.CreateAdminUserRegistrationDataExceedsMaxLimit(
                "Licensee", "Active", DefaultLicensee, DefaultBrand, "CAD");

            ResetAdminPageAndFilter();

            var newUserForm = _adminManagerPage.OpenNewUserForm();
            newUserForm.SubmitDataExceedingMaxLength(validUserRegistrationData);

            Assert.That(newUserForm.ValidationMessageForUsername,
                Is.StringContaining("Please enter no more than 50 characters."));
            Assert.That(newUserForm.ValidationMessageForFirstName,
                Is.StringContaining("Please enter no more than 50 characters."));
            Assert.That(newUserForm.ValidationMessageForLastName,
                Is.StringContaining("Please enter no more than 50 characters."));
        }

        [Test]
        public void Can_login_with_resetted_password()
        {
            // create a user
            var userData = _securityTestHelper.CreateUser(_defaultLicenseeId, new[] { _brand }, new[]{"CAD"}, _userPassword);

            ResetAdminPageAndFilter();
            var resetPasswordPage = _adminManagerPage.OpenResetPasswordTab(userData.Username);

            var newPassword = TestDataGenerator.GetRandomString(6);
            resetPasswordPage.ResetUserPassword(newPassword);

            Assert.DoesNotThrow(() => _driver.LoginToAdminWebsiteAs(userData.Username, newPassword));
            Assert.AreEqual(userData.Username, _dashboardPage.Username);
        }

        private void ResetAdminPageAndFilter()
        {
            _driver.Navigate().Refresh();
            _dashboardPage.BrandFilter.SelectAll();
            _adminManagerPage = _dashboardPage.Menu.ClickAdminManagerMenuItem();
        }
    }
}
