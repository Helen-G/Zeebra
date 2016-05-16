using System.Data.Entity;
using System.Linq;
using AFT.RegoV2.ApplicationServices.Player;
using AFT.RegoV2.Core.Bonus.Data.ViewModels;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Brand.Data;
using AFT.RegoV2.Core.Bonus;
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
    class BalanceTests : SeleniumBaseForAdminWebsite
    {
        private RegistrationDataForMemberWebsite _playerData;
        private static string _username;
        private static string _password;
        private const decimal Amount = 100.25M;
        private PlayerProfilePage _playerProfilePage;
        private GamePage _gamePage;
        private GameListPage _gameListPage;
        private MemberWebsiteLoginPage _memberWebsiteLoginPage;
        private PlayerTestHelper _playerTestHelper;
        private DashboardPage _dashboardPage ;
        private BonusTestHelper _bonusTestHelper;
        private const string DefaultBrand = "138";
        private Brand _brand;
        private BrandQueries _brandQueries;

        public override void BeforeAll()
        {
            base.BeforeAll();
            _brandQueries = _container.Resolve<BrandQueries>();
            _brand = _brandQueries.GetBrands().First(x =>x.Name == DefaultBrand);
            _playerTestHelper = _container.Resolve<PlayerTestHelper>();
            _bonusTestHelper = _container.Resolve<BonusTestHelper>();
        }
        
        public override void BeforeEach()
        {
            base.BeforeEach();
            _driver.Logout();
            _dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();
        }

        [Test]
        public void Can_make_offline_deposit_and_view_updated_balance_on_game_page()
        {
            // register a player on member website
            _playerData = _container.Resolve<PlayerTestHelper>().CreatePlayerForMemberWebsite("CAD");
            _username = _playerData.Username;
            _password = _playerData.Password;
            
            //check empty balance of the player
            _memberWebsiteLoginPage = new MemberWebsiteLoginPage(_driver);
            _memberWebsiteLoginPage.NavigateToMemberWebsite();
            _playerProfilePage = _memberWebsiteLoginPage.Login(_username, _password);

            _gameListPage = _playerProfilePage.Menu.ClickPlayGamesMenu();
            _gamePage = _gameListPage.StartGame("Football");
            var initialBalance = _gamePage.Balance;
            Assert.AreEqual("Balance: $0.00", initialBalance);
            
            var expectedPlayerName = string.Format("Name: {0} {1}", _playerData.FirstName, _playerData.LastName);
            var playerName = _gamePage.PlayerName;
            Assert.AreEqual(expectedPlayerName, playerName);

            // login to admin website and make an offline deposit request
            _driver.Manage().Cookies.DeleteAllCookies();
            var dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();
            _driver.MakeOfflineDeposit(_username, Amount, _playerData.FullName);

            var playerManagerPage = dashboardPage.Menu.ClickPlayerManagerMenuItem();
            var playerInfoPage = playerManagerPage.OpenPlayerInfoPage(_username);
            playerInfoPage.OpenTransactionsSection();
            
            Assert.AreEqual(Amount, playerInfoPage.GetTransactionMainAmount("Deposit"));
            
            // check the balance on the member website
            _memberWebsiteLoginPage = new MemberWebsiteLoginPage(_driver);
            _memberWebsiteLoginPage.NavigateToMemberWebsite();
            
            _playerProfilePage = _memberWebsiteLoginPage.Login(_username, _password);
            var gameListPage = _playerProfilePage.Menu.ClickPlayGamesMenu();
            _gamePage = gameListPage.StartGame("Football");
            var currentBalance = _gamePage.Balance;
            
            Assert.AreEqual("Balance: $100.25", currentBalance);
        }

        [Test]
        public void Can_make_offline_deposit_and_view_main_balance_on_player_info()
        {
            const decimal depositAmount = 300.25M;

            // create a player
            var player = _playerTestHelper.CreatePlayerForMemberWebsite(currencyCode: "CAD");

            // make offline deposit for the player
            var paymentTestHelper = _container.Resolve<PaymentTestHelper>();
            paymentTestHelper.MakeDeposit(player.Username, depositAmount);

            // check the player's balance
            var playerManagerPage = _dashboardPage.Menu.ClickPlayerManagerMenuItem();
            var playerInfoPage = playerManagerPage.OpenPlayerInfoPage(player.Username);
            playerInfoPage.OpenBalanceInformationSection();

            Assert.AreEqual(depositAmount.ToString(), playerInfoPage.GetMainBalance());
        }

        [Test]
        public void Can_redeem_bonus_and_view_bonus_balance_on_player_info()
        {
            var bonusName = TestDataGenerator.GetRandomString(5);
            const decimal depositAmount = 300.25M;

            // create a bonus
            var bonusRepository = _container.Resolve<IBonusRepository>();
            var defaultBrand = bonusRepository.Brands.AsNoTracking().SingleOrDefault(p => p.Id == _brand.Id);
            var bonusTemplate = _bonusTestHelper.CreateFirstDepositTemplate(bonusName, defaultBrand, IssuanceMode.AutomaticWithCode);
            var bonus = _bonusTestHelper.CreateBonus(bonusTemplate);

            // create a player
            var player = _playerTestHelper.CreatePlayerForMemberWebsite(currencyCode: "CAD");
            var playerQueries = _container.Resolve<PlayerQueries>();
            var playerId = playerQueries.GetPlayerByUsername(player.Username).Id;

            // make offline deposit and claim the bonus
            var paymentTestHelper = _container.Resolve<PaymentTestHelper>();
            paymentTestHelper.MakeDepositSelenium(playerId, depositAmount, bonus.Code);

            var playerManagerPage = _dashboardPage.Menu.ClickPlayerManagerMenuItem();
            var playerInfoPage = playerManagerPage.OpenPlayerInfoPage(player.Username);
            playerInfoPage.OpenBalanceInformationSection();

            Assert.AreEqual("25", playerInfoPage.GetBonusBalance());
        }
    }
}
