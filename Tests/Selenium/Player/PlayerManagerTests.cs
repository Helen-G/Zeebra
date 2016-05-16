using System;
using System.Threading;
using System.Web.Configuration;
using AFT.RegoV2.ApplicationServices.Player;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Domain.Payment.ApplicationServices;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Extensions;
using AFT.RegoV2.Tests.Common.Helpers;
using AFT.RegoV2.Tests.Common.Pages.BackEnd;
using AFT.RegoV2.Tests.Common.Pages.FrontEnd;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Selenium
{
    class PlayerManagerTests : SeleniumBaseForAdminWebsite
    {
        private PlayerManagerPage _playerManagerPage;
        private DashboardPage _dashboardPage;
        private BrandCommands _brandCommands;
        private BrandQueries _brandQueries;
        private const string DefaultLicensee = "Flycow";
        private const string DefaultBrand = "138";
        private static readonly Guid DefaultBrandId = Guid.Parse("00000000-0000-0000-0000-000000000138");
        private PlayerTestHelper _playerTestHelper;
        private PaymentSettingsCommands _paymentSettingsCommands;
        private SecurityTestHelper _securityTestHelper;

        public override void BeforeEach()
        {
            base.BeforeEach();
            _driver.Logout();
            _dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();
            _playerManagerPage = _dashboardPage.Menu.ClickPlayerManagerMenuItem();
            _brandCommands = _container.Resolve<BrandCommands>();
            _brandQueries = _container.Resolve<BrandQueries>();
            _playerTestHelper = _container.Resolve<PlayerTestHelper>();
            _paymentSettingsCommands = _container.Resolve<PaymentSettingsCommands>();
            _securityTestHelper = _container.Resolve<SecurityTestHelper>();
        }

        [Test]
        public void Can_view_default_payment_level_automatically_applied_to_player()
        {
            // create a player
            const string paymentLevel = "CNYLevel";
            var playerData = TestDataGenerator.CreateValidPlayerDataForAdminWebsite(DefaultLicensee, DefaultBrand, currency: "CNY");

            var newPlayerForm = _playerManagerPage.OpenNewPlayerForm();
            var submittedForm = newPlayerForm.Register(playerData);
            Assert.AreEqual("The player has been successfully created", submittedForm.ConfirmationMessage);

            submittedForm.CloseTab("View Player");
            var playerInfoPage = _playerManagerPage.OpenPlayerInfoPage(playerData.LoginName);
            playerInfoPage.OpenAccountInformationSection();
            var playerAccountInfo = playerInfoPage.GetAccountDetails();

            Assert.AreEqual(paymentLevel, playerAccountInfo.PaymentLevel);
        }

        [Test]
        public void Can_view_default_vip_level_automatically_applied_to_player()
        {
            // create VIP level for "138" brand
            var brand = _brandQueries.GetBrand(DefaultBrandId);
            var vipLevelData = brand.DefaultVipLevel;

            // create a player
            var playerData = TestDataGenerator.CreateValidPlayerDataForAdminWebsite(DefaultLicensee, DefaultBrand, currency: "CAD");
            var playerManagerPage = _dashboardPage.Menu.ClickPlayerManagerMenuItem();
            var newPlayerForm = playerManagerPage.OpenNewPlayerForm();
            var submittedPlayerForm = newPlayerForm.Register(playerData);
            submittedPlayerForm.CloseTab("View Player");

            // view VIP level applied in Player Info
            var playerInfoPage = playerManagerPage.OpenPlayerInfoPage(playerData.LoginName);
            playerInfoPage.OpenAccountInformationSection();
            var playerAccountInfo = playerInfoPage.GetAccountDetails();

            Assert.AreEqual(vipLevelData.Name, playerAccountInfo.VIPLevel);
        }

        [Test]
        public void Cannot_login_to_brand_website_as_deactivated_player()
        {
            var player = _playerTestHelper.CreatePlayerForMemberWebsite();

            //deactivate a player
            var playerInfoPage = _playerManagerPage.OpenPlayerInfoPage(player.Username);
            playerInfoPage.OpenAccountInformationSection();
            playerInfoPage.DeactivatePlayer();

            //Refresh the page as a temporary solution
            _driver.Navigate().Refresh();
            _playerManagerPage = _dashboardPage.Menu.ClickPlayerManagerMenuItem();
            _playerManagerPage.SelectPlayer(player.Username);
            Assert.AreEqual("Inactive", _playerManagerPage.Status);

            //try to log in to the brand website
            var brandWebsiteLoginPage = new MemberWebsiteLoginPage(_driver);
            brandWebsiteLoginPage.NavigateToMemberWebsite();
            brandWebsiteLoginPage.TryToLogin(player.Username, player.Password);

            Assert.AreEqual("NonActive", brandWebsiteLoginPage.GetErrorMsg());

            var expectedUrl = WebConfigurationManager.AppSettings["MemberWebsiteUrl"] + "Home/PlayerProfile";
            var actualUrl = _driver.Url;

            Assert.AreNotEqual(expectedUrl, actualUrl);
        }

        [Test]
        public void Can_edit_player_account_details()
        {
            // create a player
            var playerData = TestDataGenerator.CreateValidPlayerDataForAdminWebsite(DefaultLicensee, DefaultBrand, currency: "CAD");
            var editedPlayerData = TestDataGenerator.CreateValidPlayerDataForAdminWebsite(DefaultLicensee, DefaultBrand);
            var playerManagerPage = _dashboardPage.Menu.ClickPlayerManagerMenuItem();
            var newplayerForm = playerManagerPage.OpenNewPlayerForm();
            newplayerForm.Register(playerData);

            // login as the user and edit player's details
            _driver.Navigate().Refresh();
            var dashboardPage = new DashboardPage(_driver);
            playerManagerPage = dashboardPage.Menu.ClickPlayerManagerMenuItem();
            var playerInfo = playerManagerPage.OpenPlayerInfoPage(playerData.LoginName);
            playerInfo.OpenAccountDetailsInEditMode();
            playerInfo.Edit(editedPlayerData);

            Assert.AreEqual(editedPlayerData.FirstName, playerInfo.FirstName);
            Assert.AreEqual(editedPlayerData.LastName, playerInfo.LastName);
        }

        [Test]
        public void Can_send_new_password_to_player()
        {
            //create a player
            var playerData = TestDataGenerator.CreateValidPlayerDataForAdminWebsite(DefaultLicensee, DefaultBrand,
               currency: "CAD");
            var newPlayerForm = _playerManagerPage.OpenNewPlayerForm();
            var submittedPlayerForm = newPlayerForm.Register(playerData);
            submittedPlayerForm.CloseTab("View Player");

            // send a new password to the player
            var newPassword = TestDataGenerator.GetRandomString(8);
            var playerInfoPage = _playerManagerPage.OpenPlayerInfoPage(playerData.LoginName);
            playerInfoPage.OpenAccountInformationSection();

            var sendNewPasswordDialog = playerInfoPage.OpenSendNewPasswordDialog();
            sendNewPasswordDialog.SpecifyNewPassword(newPassword);
            sendNewPasswordDialog.Send();

            Assert.AreEqual("New password has been successfully sent", sendNewPasswordDialog.ConfirmationMessage);

            // register a player on a brand website
            var brandWebsiteLoginPage = new MemberWebsiteLoginPage(_driver);
            brandWebsiteLoginPage.NavigateToMemberWebsite();
            var playerProfilePage = brandWebsiteLoginPage.Login(playerData.LoginName, newPassword);

            Assert.AreEqual(playerData.LoginName, playerProfilePage.GetUserName());
        }

        [Test]
        public void Can_change_vip_level_of_player_and_view_new_payment_settings_are_applied()
        {
            var brandTestHelper = _container.Resolve<BrandTestHelper>();
            var paymentTestHelper = _container.Resolve<PaymentTestHelper>();
            var playerTestHelper = _container.Resolve<PlayerTestHelper>();
            var playerQueries = _container.Resolve<PlayerQueries>();
            var defaultLicenseeId = brandTestHelper.GetDefaultLicensee();

            //create a brand for a default licensee
            var brand = brandTestHelper.CreateBrand(defaultLicenseeId, null, null, null, true);
            paymentTestHelper.CreatePaymentSettings(brand, PaymentType.Deposit);

            // create a player with a bound bank account for a brand
            var playerId = playerTestHelper.CreatePlayer(null, true, brand.Id);
            Thread.Sleep(5000);
            var player = playerQueries.GetPlayer(playerId);
            var playerUsername = player.Username;
            paymentTestHelper.CreatePlayerBankAccount(playerId, true);
            var defaultVipLevel = player.VipLevel;

            // create second brand's vip level and payment settings based on it
            var secondVipLevel = brandTestHelper.CreateVipLevel(brand.Id, isDefault: false);
            var settings = new PaymentSettingsValues
            {
                MinAmountPerTransaction = 5,
                MaxAmountPerTransaction = 9
            };
            var paymentSettingsForSecondVipLevel = paymentTestHelper.CreatePaymentSettings(brand, PaymentType.Deposit, secondVipLevel.Id.ToString(), settings);

            //login to admin website, select to display the custom brand
            _driver.Logout();
            _dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();
            _dashboardPage.BrandFilter.SelectAll();

            //make offline deposit and check the relevant payment settings are applied for player
            _playerManagerPage = _dashboardPage.Menu.ClickPlayerManagerMenuItem();
            _playerManagerPage.SelectPlayer(playerUsername);
            var offlineDepositRequestForm = _playerManagerPage.OpenOfflineDepositRequestForm();

            const decimal depositAmount = 10;
            var submittedOfflineDepositRequest = offlineDepositRequestForm.Submit(depositAmount);
            Assert.AreEqual("Offline deposit request has been created successfully", submittedOfflineDepositRequest.Confirmation);

            //deactivate the default vip level and make the second vip level default
            submittedOfflineDepositRequest.CloseTab("View Offline Deposit Request");
            var vipLevelManagerPage = _playerManagerPage.Menu.ClickVipLevelManagerMenuItem();
            var deactivateVipLevelDialog = vipLevelManagerPage.OpenDeactivateDialog(defaultVipLevel.Name, true);
            vipLevelManagerPage = deactivateVipLevelDialog.Deactivate();

            //change the vip level of the player
            _playerManagerPage = vipLevelManagerPage.Menu.ClickPlayerManagerMenuItem();
            var playerInfoPage = _playerManagerPage.OpenPlayerInfoPage(playerUsername);
            playerInfoPage.OpenAccountInformationSection();
            var changeVipLevelDialog = playerInfoPage.OpenChangeVipLevelDialog();
            changeVipLevelDialog.Submit(secondVipLevel.Name);
            playerInfoPage.CloseTab("Player Info");

            //enable payment settings for second vip level
            _paymentSettingsCommands.Enable(paymentSettingsForSecondVipLevel.Id, "remark");

            _playerManagerPage.SelectPlayer(playerUsername);
            var secondOfflineDepositRequestForm = _playerManagerPage.OpenOfflineDepositRequestForm();
            secondOfflineDepositRequestForm.TryToSubmit(depositAmount);

            Assert.That(offlineDepositRequestForm.ErrorMessage, Is.StringContaining("Deposit failed. The entered amount exceeds the allowed value. Maximum value is 9.00."));
        }

    }
}
