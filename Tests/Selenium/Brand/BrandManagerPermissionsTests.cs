using System.Linq;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Extensions;
using AFT.RegoV2.Tests.Common.Pages.BackEnd;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Selenium
{
    class BrandManagerPermissionsTests : SeleniumBaseForAdminWebsite
    {
        private DashboardPage _dashboardPage;
        private const string DefaultLicensee = "Flycow";
        private const string DefaultBrand = "138";

        public override void BeforeEach()
        {
            base.BeforeEach();
            _driver.Logout();
            _dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();
        }

       [Test]
        public void Cannot_view_brand_manager_without_permissions()
        {
            //create role
            var roleData = TestDataGenerator.CreateValidRoleData(code: null, name: null, licensee: DefaultLicensee);
            //create a user
            var userData = TestDataGenerator.CreateValidAdminUserRegistrationData(
                roleData.RoleName, "Active", roleData.Licensee, DefaultBrand, currency: "ALL");
            _driver.CreateUser(roleData, userData, new[] { NewRoleForm.BrandManagerCreate });
            
            //log in as the user
            _dashboardPage = _driver.LoginToAdminWebsiteAs(userData.UserName, userData.Password);
            var brandManagerMenuItemVisible = _dashboardPage.Menu.CheckIfMenuItemDisplayed(BackendMenuBar.BrandManager); ;

            Assert.IsFalse(brandManagerMenuItemVisible);
        }

        [Test]
        public void Can_view_brand_manager_with_permissions()
        {
            //create a user based on role with view permissions
            var brandName = TestDataGenerator.GetRandomString(7);
            var userData = TestDataGenerator.CreateValidAdminUserRegistrationData(
                role: "SuperAdmin", status: "Active", licensee: DefaultLicensee, brand: DefaultBrand, currency: "ALL");
            _driver.CreateUserBasedOnPredefinedRole(userData);

            //log in as the user
            _dashboardPage = _driver.LoginToAdminWebsiteAs(userData.UserName, userData.Password);
            _dashboardPage.Menu.ClickBrandManagerItem();
            var brandManagerMenuItemVisible = _dashboardPage.Menu.CheckIfMenuItemDisplayed(BackendMenuBar.BrandManager); ;

            Assert.IsTrue(brandManagerMenuItemVisible);
        }

        [Test]
        public void Can_manage_brand_with_permissions()
        {
            //create a brand
            var brandPage = _dashboardPage.Menu.ClickBrandManagerItem();
            var newBrandPage = brandPage.OpenNewBrandForm();
            var brandName = TestDataGenerator.GetRandomAlphabeticString(5);
            var brandCode = TestDataGenerator.GetRandomAlphabeticString(5);
            var playerPrefix = TestDataGenerator.GetRandomAlphabeticString(3);
            var submittedBrandForm = newBrandPage.Submit(brandName, brandCode, playerPrefix);
            Assert.AreEqual("The brand has been successfully created.", submittedBrandForm.ConfirmationMessage);
            _dashboardPage.BrandFilter.SelectAll();

            //create wallet for brand
            var walletTemplateListPage = _dashboardPage.Menu.ClickWalletManagerMenuItem();
            var addWalletTemplateForm = walletTemplateListPage.OpenNewWalletForm();
            addWalletTemplateForm.Submit(DefaultLicensee, brandName);

            //create a user for the new brand
            var userData = TestDataGenerator.CreateValidAdminUserRegistrationData(
                role: "Licensee",
                status: "Active",
                licensee: DefaultLicensee,
                brand: brandName,
                currency: "ALL");
            _driver.CreateUserBasedOnPredefinedRole(userData);

            //log in as the new user and verify management button states
            _driver.LoginToAdminWebsiteAs(userData.UserName, userData.Password);
            brandPage = _dashboardPage.Menu.ClickBrandManagerItem();
            var newButton = brandPage.FindButton(BrandManagerPage.NewButton);
            var editButton = brandPage.FindButton(BrandManagerPage.EditButton);
            var activateButton = brandPage.FindButton(BrandManagerPage.ActivateButton);

            Assert.That(newButton.Displayed);
            Assert.That(editButton.Displayed);
            Assert.That(activateButton.Displayed);

            //edit brand
            var editBrandPage = brandPage.OpenEditBrandForm(brandName);
            brandName += "edited";
            var submittedEditBrandForm = editBrandPage.EditOnlyRequiredData(
                brandType: "Deposit",
                brandName: brandName,
                brandCode: TestDataGenerator.GetRandomAlphabeticString(5));
            Assert.AreEqual("The brand has been successfully updated.", submittedEditBrandForm.ConfirmationMessage);

            //view brand
            _driver.LoginToAdminWebsiteAs(userData.UserName, userData.Password);
            brandPage = _dashboardPage.Menu.ClickBrandManagerItem();
            var viewBrandForm = brandPage.OpenViewBrandForm(brandName);
            //Assert.AreEqual(brandName, viewBrandForm.BrandNameValue);

            Assert.AreEqual("Deposit", viewBrandForm.BrandType);
            viewBrandForm.CloseTab("View Brand");

            //try activate brand and verify error messages
            var activateDialog = brandPage.OpenBrandActivateDialog(brandName);
            activateDialog.TryToActivate("approved");
            var validationMessages = activateDialog.GetErrorMessages().ToArray();

            Assert.That(validationMessages.Length, Is.EqualTo(6));
        }
    }
}
