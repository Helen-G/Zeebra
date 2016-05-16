using System;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using AFT.RegoV2.ApplicationServices.Payment;
using AFT.RegoV2.ApplicationServices.Player;
using AFT.RegoV2.Core.Bonus;
using AFT.RegoV2.Core.Bonus.Data;
using AFT.RegoV2.Core.Bonus.Data.ViewModels;
using AFT.RegoV2.Domain.Payment;
using AFT.RegoV2.Infrastructure.DataAccess.Bonus;
using AFT.RegoV2.Infrastructure.DataAccess.Player;
using AFT.RegoV2.Infrastructure.DependencyResolution;
using AFT.RegoV2.Shared;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Extensions;
using AFT.RegoV2.Tests.Common.Helpers;
using AFT.RegoV2.Tests.Common.Pages.BackEnd;
using AFT.RegoV2.Tests.Common.Pages.FrontEnd;
using AFT.RegoV2.Tests.Unit.Bonus.Features;
using Microsoft.Practices.Unity;
using NUnit.Framework;
using ServiceStack.Common.Utils;

namespace AFT.RegoV2.Tests.Selenium
{
    internal class BonusTests : SeleniumBaseForAdminWebsite
    {
        private BackendMenuBar _menu;

        private const string LicenseeName = "Flycow";
        private const string BrandName = "138";
        private string _bonusName;
        private string _bonusCode;
        private string _bonusTemplateName;
        private DashboardPage _dashboardPage;
        private BonusTestHelper _bonusTestHelper;
        private PlayerTestHelper _playerTestHelper;
        private PaymentTestHelper _paymentTestHelper;
        private PaymentQueries _paymentQueries;
        private PlayerQueries _playerQueries;
        private GamesTestHelper _gamesTestHelper;

        public override void BeforeAll()
        {
            base.BeforeAll();
            var brandTestHelper = _container.Resolve<BrandTestHelper>();
            //_paymentTestHelper = _container.Resolve<PaymentTestHelper>();
            _playerTestHelper = _container.Resolve<PlayerTestHelper>();
            _paymentQueries = _container.Resolve<PaymentQueries>();
            _gamesTestHelper = _container.Resolve<GamesTestHelper>();

            _playerQueries = _container.Resolve<PlayerQueries>();
            var defaultLicenseeId = brandTestHelper.GetDefaultLicensee();
            _bonusTestHelper = _container.Resolve<BonusTestHelper>();

        }

        public override void BeforeEach()
        {
            base.BeforeEach();
            _driver.Logout();
            _dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();
            _menu = _dashboardPage.Menu;
            _paymentTestHelper = _container.Resolve<PaymentTestHelper>();
            _bonusName = TestDataGenerator.GetRandomString(12);
            _bonusCode = TestDataGenerator.GetRandomString(6);
            _bonusTemplateName = TestDataGenerator.GetRandomString(12);
        }

        [Test]
        public void Can_redeem_first_deposit_bonus_with_code_issuance_mode()
        {
            const decimal bonusAmount = 10;

            //create a bonus template and a bonus
            var bonusTemplateManagerPage = _dashboardPage.Menu.ClickBonusTemplateMenuItem();
            var submittedBonusTemplateForm = bonusTemplateManagerPage.OpenNewBonusTemplateForm()
                .SelectLicenseeAndBrand(LicenseeName, BrandName)
                .SetTemplateName(_bonusTemplateName)
                .SelectBonusType(BonusType.FirstDeposit)
                .SelectIssuanceMode(IssuanceMode.AutomaticWithCode)
                .NavigateToRules()
                .SelectCurrency("CAD")
                .EnterBonusTier(bonusAmount)
                .NavigateToSummary();

            var bonusManagerPage = submittedBonusTemplateForm.Menu.ClickBonusMenuItem();
            var newBonusForm = bonusManagerPage.OpenNewBonusForm();
            var submittedBonusForm = newBonusForm.Submit(_bonusName, _bonusCode, _bonusTemplateName, 0);

            Assert.AreEqual("Bonus has been successfully created.",
                submittedBonusForm.ConfirmationMessageAfterBonusSaving);

            submittedBonusForm.SwitchToBonusList();
            bonusManagerPage.ActivateBonus(_bonusName);
            
            //create a player
            var player = _playerTestHelper.CreatePlayerForMemberWebsite(currencyCode: "CAD");
            Thread.Sleep(5000); //wait for Player created event processing

            Thread.Sleep(5000); //wait for Player created event processing
            _driver.MakeOfflineDeposit(player.Username, 100, player.FullName, _bonusName, _bonusCode);
            //Thread.Sleep(5000); //wait for Deposit created event processing

            //login to admin site
            _driver.Logout();
            _dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();
            var playerManagerPage = _dashboardPage.Menu.ClickPlayerManagerMenuItem();

            //check the bouns
            playerManagerPage.SelectPlayer(player.Username);
            var playerInfoPage = playerManagerPage.OpenPlayerInfoPage();
            playerInfoPage.OpenTransactionsSection();
            var playersBonusAmount = playerInfoPage.GetTransactionBonusAmount("Bonus");
            Assert.AreEqual(bonusAmount, playersBonusAmount);
            
            //deactivate bonus
            DeactivateBonus();
        }

        [Test, Ignore("Configuration of bonus is not available for R1")]
        public void Can_redeem_refer_friends_bonus()
        {
            const decimal bonusAmount = 150;
            const decimal wageringCondition = 2;
            const decimal minDepositAmount = 200;
            const decimal actualDepositAmount = 250;
            const decimal requiredWagering = actualDepositAmount*wageringCondition;

            //create a bonus template
            var bonusTemplateManagerPage = _dashboardPage.Menu.ClickBonusTemplateMenuItem();
            var submittedBonusTemplateForm = bonusTemplateManagerPage.OpenNewBonusTemplateForm()
                .SelectLicenseeAndBrand(LicenseeName, BrandName)
                .SetTemplateName(_bonusTemplateName)
                .SelectBonusType(BonusType.ReferFriend)
                .SelectIssuanceMode(IssuanceMode.ManualByPlayer)
                .NavigateToRules()
                .SelectCurrency("CAD")
                .EnterReferFriendsConfiguration(minDepositAmount, wageringCondition)
                .EnterBonusTier(bonusAmount, 1, 1)
                .NavigateToSummary();

            var bonusManagerPage = submittedBonusTemplateForm.Menu.ClickBonusMenuItem();
            var newBonusForm = bonusManagerPage.OpenNewBonusForm();

            //create a bonus
            var submittedBonusForm = newBonusForm.Submit(_bonusName, _bonusCode, _bonusTemplateName, 0);
            submittedBonusForm.SwitchToBonusList();
            bonusManagerPage.ActivateBonus(_bonusName);

            //create a referrer
            var referrerData = _driver.LoginAsSuperAdminAndCreatePlayer(LicenseeName, BrandName, "CAD");

            //refer a friend
            _driver.Manage().Cookies.DeleteAllCookies();
            var memberWebsiteLoginPage = new MemberWebsiteLoginPage(_driver);
            memberWebsiteLoginPage.NavigateToMemberWebsite();
            var playerProfilePage = memberWebsiteLoginPage.Login(referrerData.LoginName, referrerData.Password);
            var referFriendPage = playerProfilePage.Menu.ClickReferFriendsMenu();
            referFriendPage.ReferFriend();
            Assert.AreEqual("Phone numbers successfully submitted.", referFriendPage.Message);

            _driver.Manage().Cookies.DeleteAllCookies();

            //register referred
            var referralId = new BonusRepository().Players.Single(a => a.Name == referrerData.LoginName).ReferralId;
            var referredRegistrationData = TestDataGenerator.CreateValidPlayerDataForMemberWebsite("CAD");
            var registerPage = new RegisterPage(_driver);
            registerPage.NavigateToMemberWebsite(referralId.ToString());
            registerPage.Register(referredRegistrationData);

            //depositing missing funds in order to complete wagering
            _paymentTestHelper.MakeDeposit(referredRegistrationData.Username, requiredWagering - actualDepositAmount);

            //make deposit for referred
            _driver.MakeOfflineDeposit(referredRegistrationData.Username, actualDepositAmount,
                referredRegistrationData.FullName);
            _driver.Manage().Cookies.DeleteAllCookies();

            //complete wagering requirements for referred
            memberWebsiteLoginPage = new MemberWebsiteLoginPage(_driver);
            memberWebsiteLoginPage.NavigateToMemberWebsite();
            _driver.Manage().Cookies.DeleteAllCookies();
            _driver.Navigate().Refresh();

            playerProfilePage = memberWebsiteLoginPage.Login(referredRegistrationData.Username,
                referredRegistrationData.Password);
            var gameListPage = playerProfilePage.Menu.ClickPlayGamesMenu();
            var gamePage = gameListPage.StartGame("Poker");
            gamePage.PlaceInitialBet(requiredWagering, "");
            gamePage.WinBet(requiredWagering);
            _driver.Manage().Cookies.DeleteAllCookies();

            //claim refer a friend bonus reward
            memberWebsiteLoginPage = new MemberWebsiteLoginPage(_driver);
            memberWebsiteLoginPage.NavigateToMemberWebsite();

            playerProfilePage = memberWebsiteLoginPage.Login(referrerData.LoginName, referrerData.Password);
            var claimBonusPage = playerProfilePage.Menu.ClickClaimBonusSubMenu();
            claimBonusPage.ClaimBonus();

            Assert.AreEqual("Redemption claimed successfully.", claimBonusPage.Message);

            //login to admin site
            _driver.Logout();
            _dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();
            var playerManagerPage = _dashboardPage.Menu.ClickPlayerManagerMenuItem();

            //check the bouns
            playerManagerPage.SelectPlayer(referrerData.LoginName);
            var playerInfoPage = playerManagerPage.OpenPlayerInfoPage();
            playerInfoPage.OpenTransactionsSection();
            var playersBonusAmount = playerInfoPage.GetTransactionBonusAmount("Bonus");
            Assert.AreEqual(bonusAmount, playersBonusAmount);

            DeactivateBonus();
        }

        [Test, Ignore("Configuration of bonus is not available for R1")]
        public void Can_receive_high_deposit_bonus_reward()
        {
            const decimal bonusAmount = 100;
            const decimal depositAmount = 300;
            const decimal requiredDeposit = 250;

            //create a bonus template and a bonus
            var bonusTemplateManagerPage = _dashboardPage.Menu.ClickBonusTemplateMenuItem();
            var submittedBonusTemplateForm = bonusTemplateManagerPage.OpenNewBonusTemplateForm()
                .SelectLicenseeAndBrand(LicenseeName, BrandName)
                .SetTemplateName(_bonusTemplateName)
                .SelectBonusType(BonusType.HighDeposit)
                .NavigateToRules()
                .SelectCurrency("CAD")
                .EnterHighDepositConfiguration(requiredDeposit, bonusAmount)
                .LimitMaxTotalBonusAmount(bonusAmount)
                .NavigateToSummary();

            var bonusManagerPage = submittedBonusTemplateForm.Menu.ClickBonusMenuItem();
            var newBonusForm = bonusManagerPage.OpenNewBonusForm();
            var submittedBonusForm = newBonusForm.Submit(_bonusName, _bonusCode, _bonusTemplateName, 0);

            submittedBonusForm.SwitchToBonusList();
            bonusManagerPage.ActivateBonus(_bonusName);

            var playerData = _driver.LoginAsSuperAdminAndCreatePlayer(LicenseeName, BrandName, "CAD");

            _driver.MakeOfflineDeposit(playerData.LoginName, depositAmount, playerData.FullName);

            //login to admin site
            _driver.Logout();
            _dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();
            var playerManagerPage = _dashboardPage.Menu.ClickPlayerManagerMenuItem();

            //check the bouns
            playerManagerPage.SelectPlayer(playerData.LoginName);
            var playerInfoPage = playerManagerPage.OpenPlayerInfoPage();
            playerInfoPage.OpenTransactionsSection();
            var playersBonusAmount = playerInfoPage.GetTransactionBonusAmount("Bonus");
            Assert.AreEqual(bonusAmount, playersBonusAmount);

            DeactivateBonus();
        }

        [Test, Ignore("Configuration of bonus is not available for R1")]
        public void Can_redeem_mobile_plus_email_verification_bonus()
        {
            const decimal bonusAmount = 10;
            const string brandName = "831"; //Brand with Email activation method

            //Create a bonus template and a bonus
            var bonusTemplateManagerPage = _dashboardPage.Menu.ClickBonusTemplateMenuItem();
            var submittedBonusTemplateForm = bonusTemplateManagerPage.OpenNewBonusTemplateForm()
                .SelectLicenseeAndBrand(LicenseeName, brandName)
                .SetTemplateName(_bonusTemplateName)
                .SelectBonusType(BonusType.MobilePlusEmailVerification)
                .NavigateToRules()
                .SelectCurrency("CAD")
                .EnterBonusTier(bonusAmount)
                .NavigateToSummary();

            var bonusManagerPage = submittedBonusTemplateForm.Menu.ClickBonusMenuItem();
            var newBonusForm = bonusManagerPage.OpenNewBonusForm();
            var submittedBonusForm = newBonusForm.Submit(_bonusName, _bonusCode, _bonusTemplateName, 0);
            submittedBonusForm.SwitchToBonusList();
            bonusManagerPage.ActivateBonus(_bonusName);

            var playerData = _driver.LoginAsSuperAdminAndCreatePlayer(LicenseeName, brandName, "CAD", "en-US", "CA","Inactive");
            Thread.Sleep(5000); //wait for Player created event processing

            //Verify email address
            var emailVerificationToken =
                new PlayerRepository().Players.Single(p => p.Username == playerData.LoginName)
                    .AccountActivationEmailToken;
            _driver.Manage().Cookies.DeleteAllCookies();
            var memberWebsiteAccountActivationPage = new AccountActivationPage(_driver, emailVerificationToken);
            memberWebsiteAccountActivationPage.NavigateTo();

            //Verify mobile phone number
            var memberWebsiteLoginPage = new MemberWebsiteLoginPage(_driver);
            memberWebsiteLoginPage.NavigateToMemberWebsite();
            var playerProfilePage = memberWebsiteLoginPage.Login(playerData.LoginName, playerData.Password);
            playerProfilePage.RequestMobileVerificationCode();
            var mobileVerificationCode =
                new PlayerRepository().Players.Single(p => p.Username == playerData.LoginName).MobileVerificationCode;
            playerProfilePage.VerifyMobileNumber(mobileVerificationCode);

            //login to admin site
            _driver.Logout();
            _dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();
            var playerManagerPage = _dashboardPage.Menu.ClickPlayerManagerMenuItem();

            //check the bouns
            playerManagerPage.SelectPlayer(playerData.LoginName);
            var playerInfoPage = playerManagerPage.OpenPlayerInfoPage();
            playerInfoPage.OpenTransactionsSection();
            var playersBonusAmount = playerInfoPage.GetTransactionBonusAmount("Bonus");
            Assert.AreEqual(bonusAmount, playersBonusAmount);

            DeactivateBonus();
        }

        [Test]
        public void Can_redeem_fund_in_bonus_with_code_issuance_mode()
        {
            const decimal fundInFrom = 100;
            const decimal amount = 200;
            const decimal fundInPercentage = 75;
            const decimal maxTierReward = 100;
            const string walletName = "Product 138";

            //Create a bonus template and a bonus
            var bonusTemplateManagerPage = _dashboardPage.Menu.ClickBonusTemplateMenuItem();
            var submittedBonusTemplateForm = bonusTemplateManagerPage.OpenNewBonusTemplateForm()
                .SelectLicenseeAndBrand(LicenseeName, BrandName)
                .SetTemplateName(_bonusTemplateName)
                .SelectBonusType(BonusType.HighDeposit)
                //temporary commented onsaut 'cos selection is index based
                //.SelectBonusType(BonusUIType.FundIn)
                .SelectIssuanceMode(IssuanceMode.AutomaticWithCode)
                .NavigateToRules()
                .SelectRewardType(BonusRewardType.TieredPercentage)
                .SelectCurrency("CAD")
                .SelectFundInWallet(walletName)
                .EnterBonusTier(fundInPercentage, fromAmount: fundInFrom, maxTierReward: maxTierReward)
                .NavigateToSummary();

            var bonusManagerPage = submittedBonusTemplateForm.Menu.ClickBonusMenuItem();
            var newBonusForm = bonusManagerPage.OpenNewBonusForm();
            var submittedBonusForm = newBonusForm.Submit(_bonusName, _bonusCode, _bonusTemplateName, 0);
            submittedBonusForm.SwitchToBonusList();
            bonusManagerPage.ActivateBonus(_bonusName);

            //create a player
            var player = _playerTestHelper.CreatePlayerForMemberWebsite(currencyCode: "CAD");
            Thread.Sleep(5000); //wait for Player created event processing

            //Make a deposit to have funds for fund in
            _paymentTestHelper.MakeDeposit(player.Username, amount);
            //_container.Resolve<PaymentTestHelper>().MakeDeposit(player.Username, amount);

            Thread.Sleep(5000); //wait for Deposit created event processing
            var playerId = _playerQueries.GetPlayerByUsername(player.Username).Id;
            //Wait for record in DB
            WaitForOfflineDeposit(playerId, _container, TimeSpan.FromSeconds(20), amount: 200);

            //login to memeber site
            var memberWebsiteLoginPage = new MemberWebsiteLoginPage(_driver);
            memberWebsiteLoginPage.NavigateToMemberWebsite();
            var playerProfilePage = memberWebsiteLoginPage.Login(player.Username, player.Password);

            // make fundIn 
            var fundInSection = playerProfilePage.Menu.ClickTransferFundSubMenu();
            fundInSection.FundIn(amount, _bonusCode);
            Thread.Sleep(5000); //wait for Fundin transaction created event processing
            //Assert.AreEqual(maxTierReward, GetPlayerTransactionBonusAmount(player.Username, "Bonus"));

            //login to admin site
            _driver.Logout();
            _dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();
            var playerManagerPage = _dashboardPage.Menu.ClickPlayerManagerMenuItem();
            playerManagerPage.SelectPlayer(player.Username);
            var playerInfoPage = playerManagerPage.OpenPlayerInfoPage();
            playerInfoPage.OpenTransactionsSection();
            var playersBonusAmount = playerInfoPage.GetTransactionBonusAmount("Bonus");
            Assert.AreEqual(maxTierReward, playersBonusAmount);
            
            //deactivate bonus
            DeactivateBonus();
        }

        [Test]
        public void Can_redeem_first_deposit_bonus_with_automatic_issuance_mode_via_member_website()
        {
            //create a bonus template and a bonus - Bonus type: First, Reward Type: Fixed
            var bonusTemplate = _bonusTestHelper.CreateFirstDepositTemplate(mode: IssuanceMode.Automatic);
            var bonus = _bonusTestHelper.CreateBonus(bonusTemplate);
            _bonusName = bonus.Name;
            
            //create a player
            var player = _playerTestHelper.CreatePlayerForMemberWebsite(currencyCode: "CAD");
            //Thread.Sleep(5000); //wait for Player created event processing

            //login to memeber site
            var memberWebsiteLoginPage = new MemberWebsiteLoginPage(_driver);
            memberWebsiteLoginPage.NavigateToMemberWebsite();
            var playerProfilePage = memberWebsiteLoginPage.Login(player.Username, player.Password);

            //create deposit via memeber site
            var offlineDepositRequestPage = playerProfilePage.Menu.ClickOfflineDepositSubmenu();
            offlineDepositRequestPage.Submit(amount: "113", playerRemark: "my deposit");
            //Assert.AreEqual("Offline deposit requested successfully.", offlineDepositRequestPage.ConfirmationMessage);
            Thread.Sleep(5000); //wait for Deposit created event processing
            var playerId = _playerQueries.GetPlayerByUsername(player.Username).Id;
            //Wait for record in DB
            WaitForOfflineDeposit(playerId, _container, TimeSpan.FromSeconds(20), amount: 113);
            var firstdeposit = _paymentQueries.GetDepositByPlayerId(playerId);
            _paymentTestHelper.ConfirmOfflineDeposit(firstdeposit);
            _paymentTestHelper.VerifyOfflineDeposit(firstdeposit, true);
            _paymentTestHelper.ApproveOfflineDeposit(firstdeposit, true);
            Thread.Sleep(5000); //wait for deposit Aapproval event processing

            //re-login to memeber site
            memberWebsiteLoginPage.NavigateToMemberWebsite();
            _driver.Logout();
            
            //make sure that we've got a bonus
            playerProfilePage = memberWebsiteLoginPage.Login(player.Username, player.Password);
            var balanceDetailsPage = playerProfilePage.Menu.ClickBalanceInformationMenu();
            Assert.AreEqual("25.00", balanceDetailsPage.GetBonusBalance);

            //deactivate bonus
            DeactivateBonus();
        }

        [Test]
        public void Can_redeem_reload_deposit_bonus_with_manual_issuance_mode_via_memeber_website()
        {
            //create a bonus template and a bonus - Bonus type:Reload, Reward Type: Fixed
            var bonusTemplate = _bonusTestHelper.CreateReloadDepositTemplate(mode: IssuanceMode.ManualByPlayer);
            var bonus = _bonusTestHelper.CreateBonus(bonusTemplate);
            _bonusName = bonus.Name;

            //create a player
            var player = _playerTestHelper.CreatePlayerForMemberWebsite(currencyCode: "CAD");
            //Thread.Sleep(5000); //wait for Player created event processing

            //login to memeber site
            var memberWebsiteLoginPage = new MemberWebsiteLoginPage(_driver);
            memberWebsiteLoginPage.NavigateToMemberWebsite();
            var playerProfilePage = memberWebsiteLoginPage.Login(player.Username, player.Password);

            //make first deposit via memeber site
            var offlineDepositRequestPage = playerProfilePage.Menu.ClickOfflineDepositSubmenu();
            offlineDepositRequestPage.Submit(amount: "117", playerRemark: "my deposit");
            //Thread.Sleep(5000); //wait for Deposit created event processing
            var playerId = _playerQueries.GetPlayerByUsername(player.Username).Id;
            //Wait for record in DB
            WaitForOfflineDeposit(playerId, _container, TimeSpan.FromSeconds(20), amount: 117);
            var firstdeposit = _paymentQueries.GetDepositByPlayerId(playerId);
            _paymentTestHelper.ConfirmOfflineDeposit(firstdeposit);
            _paymentTestHelper.VerifyOfflineDeposit(firstdeposit, true);
            _paymentTestHelper.ApproveOfflineDeposit(firstdeposit, true);
            Thread.Sleep(5000); //wait for deposit Aapproval event processing

            //make deposit again  - to get reload bouns
            offlineDepositRequestPage.Submit(amount: "115", playerRemark: "my deposit");
            //Thread.Sleep(5000); //wait for Deposit created event processing
            //var playerId = _playerQueries.GetPlayerByUsername(player.Username).Id;
            //Wait for Deposit record in DB
            WaitForOfflineDeposit(playerId, _container, TimeSpan.FromSeconds(20), amount: 115);
            var reloaddeposit = _paymentQueries.GetLastDepositByPlayerId(playerId);
            _paymentTestHelper.ConfirmOfflineDeposit(reloaddeposit);
            _paymentTestHelper.VerifyOfflineDeposit(reloaddeposit, true);
            _paymentTestHelper.ApproveOfflineDeposit(reloaddeposit, true);
            Thread.Sleep(5000); //wait for Bonus Redemtion created event processing
            //Wait for Redemtion record in DB
            WaitForBonusRedemtion(playerId, _bonusName, _container, TimeSpan.FromSeconds(20));

            //re-login to memeber site
            memberWebsiteLoginPage.NavigateToMemberWebsite();
            _driver.Logout();
            playerProfilePage = memberWebsiteLoginPage.Login(player.Username, player.Password);
            
            //can see Clime button
              ////start debug
               //var repository = _container.Resolve<IBonusRepository>();
               //var bonusRedemtion = repository.Players.Single(p => p.Id == playerId).Wallets.SelectMany(w => w.BonusesRedeemed).Single();
               //Assert.AreEqual(bonusRedemtion.ActivationState, ActivationStatus.Claimable);
              ////end debug
            var claimBonusPage = playerProfilePage.Menu.ClickClaimBonusSubMenu();
            var button = _driver.CheckIfButtonDisplayed(ClaimBonusPage.ClaimButton);
            Assert.True(button);

            //claim the bonus
            claimBonusPage.ClaimBonus();
            Assert.AreEqual("Redemption claimed successfully.", claimBonusPage.Message);

            //go to balance page
            var balanceDetailsPage = playerProfilePage.Menu.ClickBalanceInformationMenu();
            Assert.AreEqual("25.00", balanceDetailsPage.GetBonusBalance);

            //deactivate bonus
            DeactivateBonus();
        }
        

        [Test]
        public void Can_redeem_first_deposit_bonus_with_code_issuance_mode_via_member_website()
        {
            //create a bonus
            var bonusTemplate = _bonusTestHelper.CreateFirstDepositTemplate(mode: IssuanceMode.AutomaticWithCode);
            var bonus = _bonusTestHelper.CreateBonus(bonusTemplate);
            _bonusName = bonus.Name;

            //create a player
            var player = _playerTestHelper.CreatePlayerForMemberWebsite(currencyCode: "CAD");
            Thread.Sleep(5000); //wait for Player created event processing

            //log in to brand website
            var brandWebsiteLoginPage = new MemberWebsiteLoginPage(_driver);
            brandWebsiteLoginPage.NavigateToMemberWebsite();
            var playerProfilePage = brandWebsiteLoginPage.Login(player.Username, player.Password);
            var offlineDepositRequestPage = playerProfilePage.Menu.ClickOfflineDepositSubmenu();
            
            // qualification check
            var qualificationMessage = offlineDepositRequestPage.CheckBonusCode("112", bonus.Code);
            Assert.AreEqual("Qualification check passed.", qualificationMessage);

            //create Deposit via memeber site
            offlineDepositRequestPage.Submit(amount: "112", playerRemark: "my deposit");
            Assert.AreEqual("Offline deposit requested successfully.", offlineDepositRequestPage.ConfirmationMessage);
            Thread.Sleep(5000); //wait for Deposit created event processing
            var playerId = _playerQueries.GetPlayerByUsername(player.Username).Id;
            //Wait for record in DB
            WaitForOfflineDeposit(playerId, _container, TimeSpan.FromSeconds(20), amount: 112);
            var deposit = _paymentQueries.GetDepositByPlayerId(playerId);
            _paymentTestHelper.ConfirmOfflineDeposit(deposit);
            _paymentTestHelper.VerifyOfflineDeposit(deposit, true);
            _paymentTestHelper.ApproveOfflineDeposit(deposit, true);
            Thread.Sleep(5000); //wait for deposit Approval event processing

            //log in to brand website
            brandWebsiteLoginPage.NavigateToMemberWebsite();
            _driver.Logout();
            playerProfilePage = brandWebsiteLoginPage.Login(player.Username, player.Password);
            var balanceDetailsPage = playerProfilePage.Menu.ClickBalanceInformationMenu();
            Assert.AreEqual("25.00", balanceDetailsPage.GetBonusBalance);

            //deactivate bonus
            DeactivateBonus();
        }

        [Test, Ignore("Until VladK fixes for PlaceAndBet Test Helper")]
        public void Can_set_games_for_bonus_to_contribute_players_complition_of_wagering_requirenment()
        {

            const decimal amount = 1000;

            //create a bonus - First Deposit, Automatical, Withdrawable
            var bonusTemplate = _bonusTestHelper.CreateFirstDepositWithContributionTemplate();
            var bonus = _bonusTestHelper.CreateBonus(bonusTemplate);
            _bonusName = bonus.Name;

            //create a player
            var player = _playerTestHelper.CreatePlayerForMemberWebsite(currencyCode: "CAD");
            Thread.Sleep(5000); //wait for Player created event processing

            //make a Deposit
            _paymentTestHelper.MakeDeposit(player.Username, amount);
            //_container.Resolve<PaymentTestHelper>().MakeDeposit(player.Username, amount);
            Thread.Sleep(5000); //wait for Deposit created event processing

            //place and win first bet  - must deduct only 50% wagering contribution for now
            var playerId = _playerQueries.GetPlayerByUsername(player.Username).Id;
            //TODO: VladK.  - removed hardcoded GameId 
            var gameId = Guid.Parse("BDCD4277-4FF7-46EF-B8ED-F4192E51F03C");
            //TODO: VladK. - using PlaceAndWinBet, Main and Bonus Balances updating differently in comparing with manual testing results.
            _gamesTestHelper.PlaceAndWinBet(13, 13, playerId, gameId);

            ////log in as the player to the member website and choose a game
            //var memberWebsiteLoginPage = new MemberWebsiteLoginPage(_driver);
            //memberWebsiteLoginPage.NavigateToMemberWebsite();
            //var playerProfilePage = memberWebsiteLoginPage.Login(player.Username, player.Password);
            //var gameListPage = playerProfilePage.Menu.ClickPlayGamesMenu();
            //var gamePage = gameListPage.StartGame("Poker");

            //// check the player balance
            //var initialBalance = gamePage.Balance;
            //Thread.Sleep(5000);
            //_driver.Navigate().Refresh();
            //Assert.AreEqual("Balance: $1,013.00", initialBalance);

            ////place first bet  - must deduct only 50% wagering contribution for now
            //gamePage.PlaceInitialBet(13, "initial game action");
            ////Thread.Sleep(5000);
            ////_driver.Navigate().Refresh();
            ////Assert.AreEqual("Balance: $1,000.00", gamePage.Balance);
            //// win bet
            //gamePage.WinBet(amount: 13);
            ////Thread.Sleep(5000);
            ////_driver.Navigate().Refresh();
            ////Assert.AreEqual("Balance: $1,013.00", gamePage.Balance);

            //make sure that bonus have't been unlocked yet
            var memberWebsiteLoginPage = new MemberWebsiteLoginPage(_driver);
            memberWebsiteLoginPage.NavigateToMemberWebsite();
            var playerProfilePage = memberWebsiteLoginPage.Login(player.Username, player.Password);
            var playerBalanceInformationPage = playerProfilePage.Menu.ClickBalanceInformationMenu();
            //TODO: VladK - MB=987 and BB=27, that is what we supposed to have on Player Balances after Betting
            Assert.AreEqual("987.00", playerBalanceInformationPage.GetMainBalance);
            Assert.AreEqual("26.00", playerBalanceInformationPage.GetBonusBalance);

            //place and win second bet - must deduct 50%+50% = 100% wagering contribution
            _gamesTestHelper.PlaceAndWinBet(13, 13, playerId, gameId);

            ////back to game again
            //gameListPage = playerBalanceInformationPage.Menu.ClickPlayGamesMenu();
            //gamePage = gameListPage.StartGame("Poker");
            ////place second bet - must deduct 50%+50% = 100% wagering contribution
            //gamePage.PlaceInitialBet(13, "initial game action");
            ////Thread.Sleep(5000);
            ////_driver.Navigate().Refresh();
            ////Assert.AreEqual("Balance: $1,000.00", gamePage.Balance);
            ////win bet
            //gamePage.WinBet(amount: 13);
            ////Thread.Sleep(5000);
            ////_driver.Navigate().Refresh();
            ////Assert.AreEqual("Balance: $1,013.00", gamePage.Balance);

            //make sure that we ve got bonus on Main Balance
            //memberWebsiteLoginPage.NavigateToMemberWebsite();
            //playerProfilePage = memberWebsiteLoginPage.Login(player.Username, player.Password);
            playerBalanceInformationPage = playerProfilePage.Menu.ClickBalanceInformationMenu();
            //TODO: VladK - MB=1013 and BB=0.00, that is what we supposed to have on Player Balances after Betting
            Assert.AreEqual("1013.00", playerBalanceInformationPage.GetMainBalance);
            Assert.AreEqual("0.00", playerBalanceInformationPage.GetBonusBalance);

            //deactivate bonus
            DeactivateBonus();
        }


        private void DeactivateBonus()
        {
            _driver.LoginToAdminWebsiteAsSuperAdmin();
            var bonusManagerPage = _menu.ClickBonusMenuItem();
            bonusManagerPage.DeactivateBonus(_bonusName);
        }


        private void WaitForOfflineDeposit(Guid playerId, IUnityContainer container, TimeSpan timeout, decimal amount)
        {
            var paymentRepository = container.Resolve<IPaymentRepository>();
            var stopwatch = Stopwatch.StartNew();
            while (!paymentRepository.OfflineDeposits.Any(r => r.Amount == amount && r.PlayerId == playerId) && stopwatch.Elapsed < timeout)
            {
                Thread.Sleep(500);
            }
            if (!paymentRepository.OfflineDeposits.Any(r => r.Amount == amount && r.PlayerId == playerId))
            {
                throw new RegoException("Offline Deposit timeout 20 sec. " + stopwatch.Elapsed);
            }
            var anyDeposits = paymentRepository.OfflineDeposits.Any(p => p.PlayerId == playerId && p.Amount == amount);
            Trace.WriteLine(string.Format("Any Deposits there?: {0}, for Player: {1} created in thread #{2} on {3}", anyDeposits, playerId, Thread.CurrentThread.ManagedThreadId, stopwatch.Elapsed));

            var deposit = paymentRepository.OfflineDeposits.Single(p => p.PlayerId == playerId && p.Amount == amount);
            Trace.WriteLine(string.Format("For Deposit: {0}, for Player: {1} created in thread #{2} on {3}", deposit.Id, playerId, Thread.CurrentThread.ManagedThreadId, stopwatch.Elapsed));
        }

        private void WaitForBonusRedemtion(Guid playerId, string bonusName, IUnityContainer container, TimeSpan timeout)
        {
            var repository = container.Resolve<IBonusRepository>();
            var stopwatch = Stopwatch.StartNew();
            while (   (   (repository.Players.Single(p => p.Id == playerId)).Wallets.SelectMany(w => w.BonusesRedeemed).Any(a => a.ActivationState == ActivationStatus.Claimable && a.Bonus.Name == bonusName) != true) && (stopwatch.Elapsed) < timeout   )
            {
                Thread.Sleep(500);
            }
            if (repository.Players.Single(p => p.Id == playerId).Wallets.SelectMany(w => w.BonusesRedeemed).Any(a => a.ActivationState == ActivationStatus.Claimable && a.Bonus.Name == bonusName) != true)
            {
                throw new RegoException("Redemtion Bonus timeout 20sec. " + stopwatch.Elapsed);
            }
            var anyBonusRedemtion = repository.Players.Single(p => p.Id == playerId).Wallets.SelectMany(w => w.BonusesRedeemed).Any(a => a.ActivationState == ActivationStatus.Claimable && a.Bonus.Name == bonusName);
            Trace.WriteLine(string.Format("Any Redemtions there?: {0}, for Player: {1} created in thread #{2} on {3}", anyBonusRedemtion, playerId, Thread.CurrentThread.ManagedThreadId, stopwatch.Elapsed));

            var bonusRedemtion = repository.Players.Single(p => p.Id == playerId).Wallets.SelectMany(w => w.BonusesRedeemed).Single(a => a.ActivationState == ActivationStatus.Claimable && a.Bonus.Name == bonusName);
            Trace.WriteLine(string.Format("For Bonus Redemtion Status: {0}, for Player: {1} created in thread #{2} on {3}", bonusRedemtion.ActivationState, playerId, Thread.CurrentThread.ManagedThreadId, stopwatch.Elapsed));
        }

    }
}