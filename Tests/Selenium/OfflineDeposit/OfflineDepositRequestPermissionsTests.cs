using System;
using AFT.RegoV2.ApplicationServices.Player;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Brand.Data;
using AFT.RegoV2.Core.Security.ApplicationServices;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Domain.Security.Interfaces;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Extensions;
using AFT.RegoV2.Tests.Common.Helpers;
using AFT.RegoV2.Tests.Common.Pages.BackEnd;
using Microsoft.Practices.Unity;
using NUnit.Framework;
using Brand = AFT.RegoV2.Core.Brand.Data.Brand;
using Permissions = AFT.RegoV2.Core.Security.Common.Permissions;

namespace AFT.RegoV2.Tests.Selenium
{
    class OfflineDepositRequestPermissionsTests : SeleniumBaseForAdminWebsite
    {
        private DashboardPage _dashboardPage;
        private PlayerManagerPage _playerManagerPage;
        private BrandTestHelper _brandTestHelper;
        private Licensee _defaultLicensee;
        private Brand _brand;
        private PlayerTestHelper _playerTestHelper;
        private Guid _player;
        private PlayerQueries _playerQueries;
        private PermissionService _permissionService;
        private string _playerUsername;
        private string _playerFullName;
        private SecurityTestHelper _securityTestHelper;
        private const string DefaultLicensee = "Flycow";
        private const string DefaultBrand = "138";

        public override void BeforeAll()
        {
            base.BeforeAll();
            //create a brand for the default licensee
            _brandTestHelper = _container.Resolve<BrandTestHelper>();
            _defaultLicensee = _brandTestHelper.GetDefaultLicensee();
            _brand = _brandTestHelper.CreateBrand(_defaultLicensee, null, null, null, true);

            //create a player for the brand
            _playerTestHelper = _container.Resolve<PlayerTestHelper>();
            _playerQueries = _container.Resolve<PlayerQueries>();
            _player = _playerTestHelper.CreatePlayer(referredBy: null, isActive: true, brandId: _brand.Id);
            _playerUsername = _playerQueries.GetPlayer(_player).Username;
            _playerFullName = _playerQueries.GetPlayer(_player).FirstName + " " + _playerQueries.GetPlayer(_player).LastName;

            _permissionService = _container.Resolve<PermissionService>();
            _securityTestHelper = _container.Resolve<SecurityTestHelper>();

        }
        
        public override void BeforeEach()
        {
            base.BeforeEach();
            _driver.Logout();
            _dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();
        }

        [Test]
        public void Can_view_offline_deposit_request_with_permission()
        {
            var permissions = new[]
            {
                _permissionService.GetPermission(Permissions.View, Modules.PlayerManager),
                _permissionService.GetPermission(Permissions.View, Modules.OfflineDepositRequests),
            };

            var role = _securityTestHelper.CreateRole(new[] {_defaultLicensee.Id}, permissions);
            const string password = "123456";
            var user = _securityTestHelper.CreateUser(_defaultLicensee.Id, new[] {_brand}, password: password, roleId: role.Id);
            
            // create a player and make an offline deposit request
            _driver.Navigate().Refresh();
            _dashboardPage.BrandFilter.SelectAll();
            _playerManagerPage = _dashboardPage.Menu.ClickPlayerManagerMenuItem();

            _playerManagerPage.SelectPlayer(_playerUsername);
            var newOfflineDepositRequest = _playerManagerPage.OpenOfflineDepositRequestForm();
            var submittedOfflineDepositRequestForm = newOfflineDepositRequest.Submit(amount: 850);
            var referenceCode = submittedOfflineDepositRequestForm.ReferenceCode;

            // log in as the user with permissions and view offline deposit request details
            _dashboardPage = _driver.LoginToAdminWebsiteAs(user.Username, password);
            var depositRequestsPage = _dashboardPage.Menu.ClickOfflineDepositConfirmMenuItem();
            var viewOfflineDepositRequestForm = depositRequestsPage.OpenViewOfflineDepositConfirmForm(_playerUsername, referenceCode);

            Assert.AreEqual(submittedOfflineDepositRequestForm.Username, viewOfflineDepositRequestForm.Username);
        }
        
        [Test]
        public void Can_сonfirm_offline_deposit_request_with_permission()
        {
            var permissions = new[]
            {
                _permissionService.GetPermission(Permissions.Confirm, Modules.OfflineDepositConfirmation),
                _permissionService.GetPermission(Permissions.View, Modules.OfflineDepositRequests),
                _permissionService.GetPermission(Permissions.View, Modules.PlayerManager),
            };

            var role = _securityTestHelper.CreateRole(new[] { _defaultLicensee.Id }, permissions);
            const string password = "123456";
            var user = _securityTestHelper.CreateUser(_defaultLicensee.Id, new[] { _brand }, password: password, roleId: role.Id);
            
            const decimal depositAmount = 25M;
            _driver.Navigate().Refresh();
            _dashboardPage.BrandFilter.SelectAll();

            // create a player and make an offline deposit request
            _playerManagerPage = _dashboardPage.Menu.ClickPlayerManagerMenuItem();
            _playerManagerPage.SelectPlayer(_playerUsername);
            var newOfflineDepositRequest = _playerManagerPage.OpenOfflineDepositRequestForm();
            var submittedOfflineDepositRequestForm = newOfflineDepositRequest.Submit(depositAmount);
            var referenceCode = submittedOfflineDepositRequestForm.ReferenceCode;

            // confirm the deposit
            _dashboardPage = _driver.LoginToAdminWebsiteAs(user.Username, password);
            var offlineDepositRequestsPage = _dashboardPage.Menu.ClickOfflineDepositConfirmMenuItem();
            offlineDepositRequestsPage.SelectOfflineDepositRequest(_playerUsername, referenceCode);
            var depositConfirmPage = offlineDepositRequestsPage.ClickConfirmButton();
            var validDepositConfirmData = TestDataGenerator.CreateValidDepositConfirmData(_playerFullName, depositAmount);
            var confirmedDepositRequest = depositConfirmPage.SubmitValidDepositConfirm(validDepositConfirmData);

            Assert.IsNotNull(confirmedDepositRequest.GetConfirmationMessage); ;
        }
        
        [Test]
        public void Cannot_create_offline_deposit_request_without_permission()
        {
            var roleData = TestDataGenerator.CreateValidRoleData(code: null, name: null, licensee: DefaultLicensee);
            var userData = TestDataGenerator.CreateValidAdminUserRegistrationData(
              roleData.RoleName, "Active", roleData.Licensee, DefaultBrand, currency: "ALL");
            _driver.CreateUser(roleData, userData, new[] { NewRoleForm.OfflineDepositRequestView, NewRoleForm.PlayerManagerView });

            // login as the user and check the  feature
            _dashboardPage = _driver.LoginToAdminWebsiteAs(userData.UserName, userData.Password);
            var playerManagerPage = _dashboardPage.Menu.ClickPlayerManagerMenuItem();
            var depositRequestButton = playerManagerPage.FindDepositRequestButton();

            Assert.That(!depositRequestButton.Displayed);

            var offlineDepositRequestsPage = playerManagerPage.Menu.ClickOfflineDepositConfirmMenuItem();
            Assert.IsNotNull(offlineDepositRequestsPage);
        }

        [Test]
        public void Cannot_view_offline_deposit_request_without_permission()
        {
            var roleData = TestDataGenerator.CreateValidRoleData(code: null, name: null, licensee: DefaultLicensee);
            var userData = TestDataGenerator.CreateValidAdminUserRegistrationData(
              roleData.RoleName, "Active", roleData.Licensee, DefaultBrand, currency: "ALL");
            _driver.CreateUser(roleData, userData, new[] { NewRoleForm.OfflineDepositRequestCreate, NewRoleForm.PlayerManagerView });

            // login as the user and check the offline deposit feature
            _dashboardPage = _driver.LoginToAdminWebsiteAs(userData.UserName, userData.Password);
            var offlineDepositRequestMenuItemVisible = _dashboardPage.Menu.CheckIfMenuItemDisplayed(BackendMenuBar.OfflineDepositConfirm);

            Assert.IsFalse(offlineDepositRequestMenuItemVisible);
        }

        [Test]
        public void Cannot_сonfirm_offline_deposit_request_without_permission()
        {
            var roleData = TestDataGenerator.CreateValidRoleData(code: null, name: null, licensee: DefaultLicensee);
            var userData = TestDataGenerator.CreateValidAdminUserRegistrationData(
               roleData.RoleName, "Active", roleData.Licensee, DefaultBrand, currency: "CAD");
            _driver.CreateUser(roleData, userData, new[] { NewRoleForm.OfflineDepositRequestView, NewRoleForm.PlayerManagerView });

            // login as the user and check offline deposit confirm feature
            _dashboardPage = _driver.LoginToAdminWebsiteAs(userData.UserName, userData.Password);
            var offlineDepositRequestsPage = _dashboardPage.Menu.ClickOfflineDepositConfirmMenuItem();
            var confirmButtonIsVisible = offlineDepositRequestsPage.CheckIfConfirmButtonDisplayed();

            Assert.IsFalse(confirmButtonIsVisible);
        }

        [Test]
        public void Cannot_verify_unverify_offline_deposit_request_without_permissions()
        {
            var roleData = TestDataGenerator.CreateValidRoleData(code: null, name: null, licensee: DefaultLicensee);
            var userData = TestDataGenerator.CreateValidAdminUserRegistrationData(
                roleData.RoleName, "Active", roleData.Licensee, DefaultBrand, currency: "ALL");
            _driver.CreateUser(roleData, userData, new[]
            {
                NewRoleForm.OfflineDepositConfirmationView
            });

            _dashboardPage = _driver.LoginToAdminWebsiteAs(userData.UserName, userData.Password);
            var playerDepositVerifyPage = _dashboardPage.Menu.ClickPlayerDepositVerifyItem();
            var verifyButton = playerDepositVerifyPage.FindButton(PlayerDepositVerifyPage.VerifyButton);
            var unverifyButton = playerDepositVerifyPage.FindButton(PlayerDepositVerifyPage.UnverifyButton);

            Assert.That(!verifyButton.Displayed);
            Assert.That(!unverifyButton.Displayed);
        }

        [Test]
        public void Cannot_approve_reject_offline_deposit_request_without_permissions()
        {
            var roleData = TestDataGenerator.CreateValidRoleData(code: null, name: null, licensee: DefaultLicensee);
            var userData = TestDataGenerator.CreateValidAdminUserRegistrationData(
                roleData.RoleName, "Active", roleData.Licensee, DefaultBrand, currency: "USD");
            _driver.CreateUser(roleData, userData, new[]
            {
                NewRoleForm.OfflineDepositVerificationView
            });
            _dashboardPage = _driver.LoginToAdminWebsiteAs(userData.UserName, userData.Password);
            var playerDepositApprovePage = _dashboardPage.Menu.ClickPlayerDepositApproveItem();
            var approveButton = playerDepositApprovePage.FindButton(PlayerDepositApprovePage.ApproveButton);
            var rejectButton = playerDepositApprovePage.FindButton(PlayerDepositApprovePage.RejectButton);

            Assert.That(!approveButton.Displayed);
            Assert.That(!rejectButton.Displayed);
        }
    }
}
