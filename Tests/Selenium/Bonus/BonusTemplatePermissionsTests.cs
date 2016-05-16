using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Extensions;
using AFT.RegoV2.Tests.Common.Pages.BackEnd;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Selenium
{
    class BonusTemplatePermissionsTests : SeleniumBaseForAdminWebsite
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
        public void Cannot_view_bonus_template_without_permission()
        {
            // create a user without bonus template permissions
            var roleData = TestDataGenerator.CreateValidRoleData(code: null, name: null, licensee: DefaultLicensee);
            var userData = TestDataGenerator.CreateValidAdminUserRegistrationData(
               roleData.RoleName, "Active", roleData.Licensee, DefaultBrand, currency: "ALL");
            _driver.CreateUser(roleData, userData, new[] { NewRoleForm.BonusTemplateManagerCreate });

            // log in as the user and try to access bonus templates
            _dashboardPage = _driver.LoginToAdminWebsiteAs(userData.UserName, userData.Password);
            var bonusTemplateManagerMenuItemVisible = _dashboardPage.Menu.CheckIfMenuItemDisplayed(BackendMenuBar.BonusTemplateManager);

            Assert.IsFalse(bonusTemplateManagerMenuItemVisible);
        }

        [Test]
        public void Cannot_manage_bonus_template_without_permissions()
        {
            // create a user without bonus template permissions
            var roleData = TestDataGenerator.CreateValidRoleData(code: null, name: null, licensee: DefaultLicensee);
            var userData = TestDataGenerator.CreateValidAdminUserRegistrationData(
                roleData.RoleName, "Active", roleData.Licensee, DefaultBrand, currency: "ALL");
            _driver.CreateUser(roleData, userData, new[] { NewRoleForm.BonusTemplateManagerView });

            // log in as the user and try to manage bonus templates
            _dashboardPage = _driver.LoginToAdminWebsiteAs(userData.UserName, userData.Password);
            var bonusTemplateManagerPage = _dashboardPage.Menu.ClickBonusTemplateMenuItem();

            Assert.False(bonusTemplateManagerPage._newButton.Displayed);
            Assert.False(bonusTemplateManagerPage._editButton.Displayed);
            Assert.False(bonusTemplateManagerPage._deleteButton.Displayed);
        }

        [Test]
        public void Can_create_and_view_bonus_template_with_MarketingOfficer_permissions()
        {
            // create a user with MarketingOfficer permissions
            var bonusTemplateName = TestDataGenerator.GetRandomString(7);
            var userData = TestDataGenerator.CreateValidAdminUserRegistrationData(
                role: "MarketingOfficer", status: "Active", licensee: DefaultLicensee, brand: DefaultBrand, currency: "CAD");
            _driver.CreateUserBasedOnPredefinedRole(userData);

            // log in as the user and view bonus templates
            _dashboardPage = _driver.LoginToAdminWebsiteAs(userData.UserName, userData.Password);
            var bonusTemplateManagerPage = _dashboardPage.Menu.ClickBonusTemplateMenuItem();
            bonusTemplateManagerPage.OpenNewBonusTemplateForm()
                .SelectLicenseeAndBrand(DefaultLicensee, DefaultBrand)
                .SetTemplateName(bonusTemplateName)
                .NavigateToRules()
                .SelectCurrency("CAD")
                .EnterBonusTier(100)
                .NavigateToSummary();
        }

        [Test]
        public void Can_manage_bonus_template_with_Licensee_permissions()
        {
            // create a user with Licensee permissions
            var bonusTemplateName = TestDataGenerator.GetRandomString(7);
            var newBonusTemplateName = bonusTemplateName + "edit";
            var userData = TestDataGenerator.CreateValidAdminUserRegistrationData(
                role: "Licensee", status: "Active", licensee: DefaultLicensee, brand: DefaultBrand, currency: "CAD");
            _driver.CreateUserBasedOnPredefinedRole(userData);

            // log in as the user and create the bonus template
            _dashboardPage = _driver.LoginToAdminWebsiteAs(userData.UserName, userData.Password);
            var bonusTemplateManagerPage = _dashboardPage.Menu.ClickBonusTemplateMenuItem();
             var submittedBonusTemplateForm = bonusTemplateManagerPage.OpenNewBonusTemplateForm()
                .SelectLicenseeAndBrand(DefaultLicensee, DefaultBrand)
                .SetTemplateName(bonusTemplateName)
                .NavigateToRules()
                .SelectCurrency("CAD")
                .EnterBonusTier(100)
                .NavigateToSummary();

            Assert.AreEqual(bonusTemplateName, submittedBonusTemplateForm.Name);

            // edit the bonus template
            submittedBonusTemplateForm.CloseTab();
            submittedBonusTemplateForm = bonusTemplateManagerPage.OpenEditForm(bonusTemplateName)
                .SelectLicenseeAndBrand(DefaultLicensee, DefaultBrand)
                .SetTemplateName(newBonusTemplateName)
                .NavigateToRules()
                .NavigateToSummary();

            Assert.AreEqual(newBonusTemplateName, submittedBonusTemplateForm.Name);

            //delete the bonus template
            submittedBonusTemplateForm.CloseTab();
            var deleteDialog = bonusTemplateManagerPage.OpenDeleteBonusTemplateDialog(newBonusTemplateName);
            bonusTemplateManagerPage = deleteDialog.Confirm();
            var deletedBonusTemplate = bonusTemplateManagerPage.SearchForDeletedRecord(bonusTemplateName);

            Assert.False(deletedBonusTemplate);
        }
    }
}
