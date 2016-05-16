using System;
using System.Threading;
using AFT.RegoV2.ApplicationServices.Player;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Brand.Data;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Player.Data;
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

namespace AFT.RegoV2.Tests.Selenium
{
    class PlayerManagementPermissionsTests : SeleniumBaseForAdminWebsite
    {
        private DashboardPage _dashboardPage;
        private const string DefaultLicensee = "Flycow";
        private const string DefaultBrand = "138";
        private PlayerTestHelper _playerTestHelper;
        private PermissionService _permissionService;
        private SecurityTestHelper _securityTestHelper;
        private Licensee _defaultLicensee;
        private BrandTestHelper _brandTestHelper;
        protected readonly Guid DefaultBrandId = new Guid("00000000-0000-0000-0000-000000000138");
        private PlayerQueries _playerQueries;
        private BrandQueries _brandQueries;

        public override void BeforeAll()
        {
            base.BeforeAll();
            _playerTestHelper = _container.Resolve<PlayerTestHelper>();
            _permissionService = _container.Resolve<PermissionService>();
            _securityTestHelper = _container.Resolve<SecurityTestHelper>();
            _brandTestHelper = _container.Resolve<BrandTestHelper>();
            _defaultLicensee = _brandTestHelper.GetDefaultLicensee();
            _playerQueries = _container.Resolve<PlayerQueries>();
            _brandQueries = _container.Resolve<BrandQueries>();
        }

        public override void BeforeEach()
        {
            base.BeforeEach();
            _driver.Logout();
            _dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();
        }

        [Test]
        public void Cannot_create_player_without_permission()
        {
            //create a user
            var roleData = TestDataGenerator.CreateValidRoleData(code: null, name: null, licensee: DefaultLicensee);
            var userData = TestDataGenerator.CreateValidAdminUserRegistrationData(
                roleData.RoleName, "Active", roleData.Licensee, DefaultBrand, currency: "ALL");
            _driver.CreateUser(roleData, userData, new[] { NewRoleForm.PlayerManagerView });

            //login as the user and try to access New button on Player Manager page
            _dashboardPage = _driver.LoginToAdminWebsiteAs(userData.UserName, userData.Password);
            var playerManagerPage = _dashboardPage.Menu.ClickPlayerManagerMenuItem();
            var newButton = playerManagerPage.FindButton(PlayerManagerPage.NewButton);

            Assert.That(!newButton.Displayed);
        }

        [Test]
        public void Cannot_update_player_without_permission()
        {
            //create a user
            var roleData = TestDataGenerator.CreateValidRoleData(code: null, name: null, licensee: DefaultLicensee);
            var userData = TestDataGenerator.CreateValidAdminUserRegistrationData(
                roleData.RoleName, "Active", roleData.Licensee, DefaultBrand, currency: "CAD");
            _driver.CreateUser(roleData, userData, new[] { NewRoleForm.PlayerManagerView });
            var playerData = _driver.LoginAsSuperAdminAndCreatePlayer(DefaultLicensee, DefaultBrand, currency: "CAD");

            //login as the user and try access Edit button on Player Info page
            _dashboardPage = _driver.LoginToAdminWebsiteAs(userData.UserName, userData.Password);
            var playerManagerPage = _dashboardPage.Menu.ClickPlayerManagerMenuItem();
            var playerInfoPage = playerManagerPage.OpenPlayerInfoPage(playerData.LoginName);

            var isEditButtonPresent = playerInfoPage.IsEditButtonPresent();

            Assert.AreEqual(isEditButtonPresent, false);
        }

        [Test]
        public void Cannot_view_player_without_permission()
        {
            //create a user
            var roleData = TestDataGenerator.CreateValidRoleData(code: null, name: null, licensee: DefaultLicensee);
            var userData = TestDataGenerator.CreateValidAdminUserRegistrationData(
                roleData.RoleName, "Active", roleData.Licensee, DefaultBrand, currency: "ALL");
            _driver.CreateUser(roleData, userData, new[] { NewRoleForm.PlayerManagerCreate });

            //login as the user and try to open Player Manager page
            _dashboardPage = _driver.LoginToAdminWebsiteAs(userData.UserName, userData.Password);
            var playerManagerMenuItemVisible = _dashboardPage.Menu.CheckIfMenuItemDisplayed(BackendMenuBar.OfflineDepositConfirm);

            Assert.IsFalse(playerManagerMenuItemVisible);
        }

        [Test]
        public void Cannot_deactivate_player_without_permission()
        {
            //create an active player
            var player = _playerTestHelper.CreatePlayerForMemberWebsite();

            //create a user
            var roleData = TestDataGenerator.CreateValidRoleData(code: null, name: null, licensee: DefaultLicensee);
            var userData = TestDataGenerator.CreateValidAdminUserRegistrationData(
               roleData.RoleName, "Active", roleData.Licensee, DefaultBrand, currency: "CAD");
            _driver.CreateUser(roleData, userData, new[] { NewRoleForm.PlayerManagerCreate, NewRoleForm.PlayerManagerView });

            //login as the user and try to access Deactivate button on Player Info
            _dashboardPage = _driver.LoginToAdminWebsiteAs(userData.UserName, userData.Password);
            var playerManagerPage = _dashboardPage.Menu.ClickPlayerManagerMenuItem();
            var playerInfoPage = playerManagerPage.OpenPlayerInfoPage(player.Username);
            playerInfoPage.OpenAccountInformationSection();
            var deactivateButton = playerInfoPage.FindButton(PlayerInfoPage.DeactivateButton);

            Assert.IsFalse(deactivateButton.Displayed);
        }

        [Test]
        public void Cannot_activate_player_without_permission()
        {
            //create a user
            var roleData = TestDataGenerator.CreateValidRoleData(code: null, name: null, licensee: DefaultLicensee);
            var userData = TestDataGenerator.CreateValidAdminUserRegistrationData(
               roleData.RoleName, "Active", roleData.Licensee, DefaultBrand, currency: "ALL");
            var playerData = TestDataGenerator.CreateValidPlayerDataForAdminWebsite(DefaultLicensee, DefaultBrand);
            var viewUserForm = _driver.CreateUser(roleData, userData, new[] { NewRoleForm.PlayerManagerCreate, NewRoleForm.PlayerManagerView });
            var playerManagerPage = viewUserForm.Menu.ClickPlayerManagerMenuItem();

            // create an inactive player
            var newPlayerForm = playerManagerPage.OpenNewPlayerForm();
            playerData.AccountStatus = AccountStatus.Inactive.ToString();
            newPlayerForm.Register(playerData);

            //login as the user and try to access Deactivate button on Player Info
            _dashboardPage = _driver.LoginToAdminWebsiteAs(userData.UserName, userData.Password);
            playerManagerPage = _dashboardPage.Menu.ClickPlayerManagerMenuItem();
            var playerInfoPage = playerManagerPage.OpenPlayerInfoPage(playerData.LoginName);
            playerInfoPage.OpenAccountInformationSection();
            var deactivateButton = playerInfoPage.FindButton(PlayerInfoPage.DeactivateButton);

            Assert.IsFalse(deactivateButton.Displayed);
        }

        [Test]
        public void Can_activate_player_as_CSOfficer()
        {
            // create a user based on "CSOfficer" role
            const string role = "CSOfficer";
            var playerData = TestDataGenerator.CreateValidPlayerDataForAdminWebsite(DefaultLicensee, "138");
            var userData = TestDataGenerator.CreateValidAdminUserRegistrationData(
               role, status: "Active", licensee: DefaultLicensee, brand: DefaultBrand, currency: "ALL");
            _driver.CreateUserBasedOnPredefinedRole(userData);

            // create a player
            var playerManagerPage = _dashboardPage.Menu.ClickPlayerManagerMenuItem();
            var newPlayerForm = playerManagerPage.OpenNewPlayerForm();
            playerData.AccountStatus = AccountStatus.Inactive.ToString();
            newPlayerForm.Register(playerData);

            // login as CSOfficer user and activate the player
            _dashboardPage = _driver.LoginToAdminWebsiteAs(userData.UserName, userData.Password);
            playerManagerPage = _dashboardPage.Menu.ClickPlayerManagerMenuItem();
            var playerInfoPage = playerManagerPage.OpenPlayerInfoPage(playerData.LoginName);
            playerInfoPage.OpenAccountInformationSection();
            playerInfoPage.ActivatePlayer();

            // check that Deactivate button is also available
            var deactivateButton = playerInfoPage.FindButton(PlayerInfoPage.DeactivateButton);
            Assert.NotNull(deactivateButton);

            _driver.Navigate().Refresh();
            var dashboardPage = new DashboardPage(_driver);
            playerManagerPage = dashboardPage.Menu.ClickPlayerManagerMenuItem();
            playerManagerPage.SelectPlayer(playerData.LoginName);

            Assert.AreEqual("Active", playerManagerPage.Status);
        }

        [Test]
        public void Can_view_player_as_FraudOfficer()
        {
            //create a user based on "Fraud Officer" role
            const string role = "FraudOfficer";
            var playerData = TestDataGenerator.CreateValidPlayerDataForAdminWebsite(DefaultLicensee, "138");
            var userData = TestDataGenerator.CreateValidAdminUserRegistrationData(
               role, status: "Active", licensee: DefaultLicensee, brand: DefaultBrand, currency: "ALL");
            _driver.CreateUserBasedOnPredefinedRole(userData);

            // create a player
            var playerManagerPage = _dashboardPage.Menu.ClickPlayerManagerMenuItem();
            var newPlayerForm = playerManagerPage.OpenNewPlayerForm();
            newPlayerForm.Register(playerData);

            //login as Fraud Officer user and view player details
            _dashboardPage = _driver.LoginToAdminWebsiteAs(userData.UserName, userData.Password);
            playerManagerPage = _dashboardPage.Menu.ClickPlayerManagerMenuItem();
            var playerInfoPage = playerManagerPage.OpenPlayerInfoPage(playerData.LoginName);

            Assert.AreEqual(playerData.LoginName, playerInfoPage.Username);
        }

        [Test]
        public void Can_assign_VIP_level_to_player_with_permission()
        {
            var defaultLicenseeId = _brandTestHelper.GetDefaultLicensee();
            var brand = _brandTestHelper.CreateBrand(defaultLicenseeId, isActive: true);

            // create a user with permissions to view and assign VIP level for a player
            var permissions = new[]
            {
                _permissionService.GetPermission(Permissions.View, Modules.PlayerManager),
                _permissionService.GetPermission(Permissions.Edit, Modules.PlayerManager),
                _permissionService.GetPermission(Permissions.AssignVipLevel, Modules.PlayerManager)
            };

            var role = _securityTestHelper.CreateRole(new[] { _defaultLicensee.Id }, permissions);
            Thread.Sleep(5000);
            const string password = "123456";
            var user = _securityTestHelper.CreateUser(_defaultLicensee.Id, new[] { brand }, password: password, roleId: role.Id);

            // create a player
            var playerId = _playerTestHelper.CreatePlayer(referredBy: null, isActive: true, brandId: brand.Id);
            Thread.Sleep(5000);
            var player = _playerQueries.GetPlayer(playerId);

            //create a VIP Level
            var vipLevel = _brandTestHelper.CreateVipLevel(brand.Id, isDefault: false);

            // login as the user and assign VIP level to the player
            _dashboardPage = _driver.LoginToAdminWebsiteAs(user.Username, password);
            _dashboardPage.BrandFilter.SelectAll();

            var playerManagerPage = _dashboardPage.Menu.ClickPlayerManagerMenuItem();
            var playerInfo = playerManagerPage.OpenPlayerInfoPage(player.Username);
            playerInfo.OpenAccountInformationSection();
            var changeVipLevelDialog = playerInfo.OpenChangeVipLevelDialog();
            changeVipLevelDialog.Submit(vipLevel.Name);

            Assert.AreEqual(vipLevel.Name, playerInfo.VipLevel);
        }
    }
}
