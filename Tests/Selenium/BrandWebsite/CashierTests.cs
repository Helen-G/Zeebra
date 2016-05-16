using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Extensions;
using AFT.RegoV2.Tests.Common.Helpers;
using AFT.RegoV2.Tests.Common.Pages.FrontEnd;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Selenium.MemberWebsite
{
    class CashierTests : SeleniumBaseForMemberWebsite
    {
        private MemberWebsiteLoginPage _brandWebsiteLoginPage;
        private PlayerTestHelper _playerTestHelper;
        private PaymentTestHelper _paymentTestHelper;
        private RegistrationDataForMemberWebsite _player;
        private PlayerProfilePage _playerProfilePage;
        private BalanceDetailsPage _balanceDetailsPage;
        private const decimal DepositAmount = 200;

        public override void BeforeAll()
        {
            base.BeforeAll();
            //create a player
            _playerTestHelper = _container.Resolve<PlayerTestHelper>();
            _paymentTestHelper = _container.Resolve<PaymentTestHelper>();
            _player = _playerTestHelper.CreatePlayerForMemberWebsite(currencyCode: "CAD");

            //deposit money to the player's main balance
            _paymentTestHelper.MakeDeposit(_player.Username, DepositAmount);
            
            //navigate to brand website
            _brandWebsiteLoginPage = new MemberWebsiteLoginPage(_driver);
            _brandWebsiteLoginPage.NavigateToMemberWebsite();
        }

        public override void BeforeEach()
        {
            base.BeforeEach();
            _driver.Logout();
            //log in to player's profile
            _playerProfilePage = _brandWebsiteLoginPage.Login(_player.Username, _player.Password);
            _balanceDetailsPage = _playerProfilePage.Menu.ClickBalanceInformationMenu();
        }

        [Test]
        public void Can_submit_offline_deposit_request_on_member_website()
        {
            var offlineDepositRequestPage = _balanceDetailsPage.Menu.ClickOfflineDepositSubmenu();
            offlineDepositRequestPage.Submit(amount:"100.5", playerRemark:"my deposit");

            Assert.AreEqual("Offline deposit requested successfully.", offlineDepositRequestPage.ConfirmationMessage);
        }

        [Test]
        public void Can_fund_in_fund_out_amount_on_member_website()
        {
            const decimal amount =  DepositAmount / 4;
            var transferFundRequestPage = _balanceDetailsPage.Menu.ClickTransferFundSubMenu();
            transferFundRequestPage.FundIn(amount);

            Assert.That(transferFundRequestPage.ConfirmationMessage, Is.StringContaining("Transfer fund request sent successfully."));
            Assert.That(transferFundRequestPage.ConfirmationMessage, Is.StringContaining("Transfer ID:"));
            var productWalletAmount = string.Format(amount + ".00");
            Assert.AreEqual(productWalletAmount, transferFundRequestPage.Balance);

            transferFundRequestPage.FundOut(amount);
            Assert.That(transferFundRequestPage.ConfirmationMessage, Is.StringContaining("Transfer fund request sent successfully."));
            Assert.AreEqual("0.00", transferFundRequestPage.Balance);
        }


    }
}
