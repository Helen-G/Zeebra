using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Extensions;
using AFT.RegoV2.Tests.Common.Helpers;
using AFT.RegoV2.Tests.Common.Pages.FrontEnd;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Selenium
{
    class GameMockTests : SeleniumBaseForGameWebsite
    {
        private RegistrationDataForMemberWebsite _playerData;
        private static string _username;
        private const decimal Amount = 10000.25M;
        private PlayerProfilePage _playerProfilePage;
        private GamePage _gamePage;
        private GameListPage _gameListPage;
        private MemberWebsiteLoginPage _memberWebsiteLoginPage;

        public override void BeforeEach()
        {
            base.BeforeEach();
            _driver.Logout();
            // register a player on member website
            _playerData = TestDataGenerator.CreateValidPlayerDataForMemberWebsite("CAD");
            _username = _playerData.Username;
            
            _memberWebsiteLoginPage = new MemberWebsiteLoginPage(_driver);
            _memberWebsiteLoginPage.NavigateToMemberWebsite();
            var registerPage = _memberWebsiteLoginPage.GoToRegisterPage();
            registerPage.Register(_playerData);



            //make a deposit
            _container.Resolve<PaymentTestHelper>().MakeDeposit(_username, Amount);
            _playerProfilePage = registerPage.GoToProfilePage();

            _gameListPage = _playerProfilePage.Menu.ClickPlayGamesMenu();
        }

        [Test]
        public void Can_run_game_website_and_make_bets()
        {
            _gamePage = _gameListPage.StartGame("Football");

            // check the player balance
            var initialBalance = _gamePage.Balance;
            Assert.AreEqual("Balance: $10,000.25", initialBalance);
            
            var expectedPlayerName = string.Format("Name: {0} {1}", _playerData.FirstName, _playerData.LastName);
            var playerName = _gamePage.PlayerName;
            Assert.AreEqual(expectedPlayerName, playerName);

            var expectedTag = "Tag: 138";
            Assert.AreEqual(expectedTag, _gamePage.Tag);

            // make a bet
            _gamePage.PlaceInitialBet(100, "description test");
            Assert.AreEqual("-100.00", _gamePage.BetAmount);

            // check a transaction's type and amount
            var txAmount = _gamePage.GetTransactionDetail(0, "amount");
            Assert.AreEqual("-100.00", txAmount);

            var txType = _gamePage.GetTransactionDetail(0, "type");
            Assert.AreEqual("Placed", txType);

            // check the player balance
            Assert.AreEqual("Balance: $9,900.25", _gamePage.Balance);

            // place another bet
            _gamePage.PlaceSubBet(50, "description test");
            Assert.AreEqual("-150.00", _gamePage.BetAmount);

            // check a transaction's type and amount
            txAmount = _gamePage.GetTransactionDetail(1, "amount");
            Assert.AreEqual("-50.00", txAmount);

            txType = _gamePage.GetTransactionDetail(1, "type");
            Assert.AreEqual("Placed", txType);
            Assert.AreEqual("Balance: $9,850.25", _gamePage.Balance);

            // win a bet
            _gamePage.WinBet(amount:300);
            Assert.AreEqual("150.00", _gamePage.BetAmount);
            Assert.AreEqual("Balance: $10,150.25", _gamePage.Balance);

            // adjust
            //TODO: Not implemented in wallet
            //_gamePage.AdjustTransaction(txNumber:2, amount:400);
            //Assert.AreEqual("550.00", _gamePage.BetAmount);
            //Assert.AreEqual("Balance: $10,550.25", _gamePage.Balance);

            // cancel
            _gamePage.CancelTransaction(txNumber:2, amount:300);
            Assert.AreEqual("-150.00", _gamePage.BetAmount);
            Assert.AreEqual("Balance: $9,850.25", _gamePage.Balance);

            // verify transactions on the player info tab
            var dashboard = _driver.LoginToAdminWebsiteAsSuperAdmin(); 
            var playersList = dashboard.Menu.ClickPlayerManagerMenuItem();
            var playerInfoPage = playersList.OpenPlayerInfoPage(_username);
            playerInfoPage.OpenTransactionsSection();

            var betPlacedAmounts = playerInfoPage.GetTransactionsMainAmount("Bet placed");

            Assert.Contains(-100m, betPlacedAmounts);
            Assert.Contains(-50m, betPlacedAmounts);
            Assert.AreEqual(300, playerInfoPage.GetTransactionMainAmount("Bet won"));
            //TODO: Not implemented in wallet
            //Assert.AreEqual(400, playerInfoPage.GetTransactionTotalAmount("Bet adjusted"));
            Assert.AreEqual(-300, playerInfoPage.GetTransactionMainAmount("Bet canceled"));
        }

        [Test]
        public void Can_run_secure_game_website()
        {
            _gamePage = _gameListPage.StartGame("Secure game");

            // check the player balance
            var initialBalance = _gamePage.Balance;
            Assert.AreEqual("Balance: $10,000.25", initialBalance);
        }
    }
}