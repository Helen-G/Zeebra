using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Extensions;
using AFT.RegoV2.Tests.Common.Pages.BackEnd;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Selenium
{
    class OfflineWithdrawRequestPermissionsTests : SeleniumBaseForAdminWebsite
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
        public void Cannot_view_Offline_Withdraw_Requests_page_without_permission()
        {
            //create a user based on a role without permissions to view withdraw requests page
            var userData = TestDataGenerator.CreateValidAdminUserRegistrationData(
                role: "KYCOfficer", status: "Active", licensee: DefaultLicensee, brand: DefaultBrand, currency: "ALL");
            _driver.CreateUserBasedOnPredefinedRole(userData);

            //login as the user
            _driver.LoginToAdminWebsiteAs(userData.UserName, userData.Password);
            
            var offlineWithdrawRequestMenuItemVisible = _dashboardPage.Menu.CheckIfMenuItemDisplayed(BackendMenuBar.OfflineWithdrawRequests);
            Assert.IsFalse(offlineWithdrawRequestMenuItemVisible);
        }
        
        [Test]
        public void Cannot_create_offline_withdraw_requst_without_permission()
        {
            var roleData = TestDataGenerator.CreateValidRoleData(code: null, name: null, licensee: DefaultLicensee);
            var userData = TestDataGenerator.CreateValidAdminUserRegistrationData(
              roleData.RoleName, "Active", roleData.Licensee, DefaultBrand, currency: "ALL");
            _driver.CreateUser(roleData, userData, new[] { NewRoleForm.PlayerManagerView});
            
            _dashboardPage = _driver.LoginToAdminWebsiteAs(userData.UserName, userData.Password);
            var playerManagerPage = _dashboardPage.Menu.ClickPlayerManagerMenuItem();
            var withdrawRequestButton = playerManagerPage.FindButton(PlayerManagerPage.OfflineWithdrawRequestButton);

            Assert.That(!withdrawRequestButton.Displayed);
        }

        [Test]
        public void Cannot_verify_unverify_offline_withdraw_request_without_permissions()
        {
            var roleData = TestDataGenerator.CreateValidRoleData(code: null, name: null, licensee: DefaultLicensee);
            var userData = TestDataGenerator.CreateValidAdminUserRegistrationData(
                roleData.RoleName, "Active", roleData.Licensee, DefaultBrand, currency: "ALL");
            _driver.CreateUser(roleData, userData, new[]
            {
                NewRoleForm.OfflineWithdrawView
            });

            _dashboardPage = _driver.LoginToAdminWebsiteAs(userData.UserName, userData.Password);
            var oflineWithdrawRequestsPage = _dashboardPage.Menu.ClickOfflineWithdrawRequestsMenuItem();
            var verifyButton = oflineWithdrawRequestsPage.FindButton(OfflineWithrawRequestsPage.VerifyButton);
            var unverifyButton = oflineWithdrawRequestsPage.FindButton(OfflineWithrawRequestsPage.UnverifyButton);

            Assert.That(!verifyButton.Displayed);
            Assert.That(!unverifyButton.Displayed);
        }
        
    }
}
