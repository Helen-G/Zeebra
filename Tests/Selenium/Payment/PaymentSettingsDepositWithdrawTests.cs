using AFT.RegoV2.ApplicationServices.Player;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Domain.Payment.Data;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Extensions;
using AFT.RegoV2.Tests.Common.Helpers;
using AFT.RegoV2.Tests.Common.Pages.BackEnd;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Selenium
{
    class PaymentSettingsDepositWithdrawTests : SeleniumBaseForAdminWebsite
    {
        private PlayerManagerPage _playerManagerPage;
        private DashboardPage _dashboardPage;
        private string _playerUsername;

        public override void BeforeAll()
        {
            base.BeforeAll();
            var brandTestHelper = _container.Resolve<BrandTestHelper>();
            var paymentTestHelper = _container.Resolve<PaymentTestHelper>();
            var playerTestHelper = _container.Resolve<PlayerTestHelper>();
            var playerQueries = _container.Resolve<PlayerQueries>();
            var defaultLicenseeId = brandTestHelper.GetDefaultLicensee();
            
            //create a brand for a default licensee
            var brand = brandTestHelper.CreateBrand(defaultLicenseeId, null, null, null, true);
            paymentTestHelper.CreatePaymentSettings(brand, PaymentType.Deposit);
            paymentTestHelper.CreatePaymentSettings(brand, PaymentType.Withdraw);

            // create a player with a bound bank account for a brand
            var playerId = playerTestHelper.CreatePlayer(null, true, brand.Id);
            _playerUsername = playerQueries.GetPlayer(playerId).Username;
            paymentTestHelper.CreatePlayerBankAccount(playerId, true);
        }

        public override void BeforeEach()
        {
            base.BeforeEach();
            _driver.Logout();
            _dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();
            _dashboardPage.BrandFilter.SelectAll();
            _playerManagerPage = _dashboardPage.Menu.ClickPlayerManagerMenuItem();
        }

        [TestCase(9, "Deposit failed. The entered amount is below the allowed value. Minimum value is 10.00.")]
        [TestCase(201, "Deposit failed. The entered amount exceeds the allowed value. Maximum value is 200.00.")]
        public void Cannot_deposit_amount_outside_payment_settings_boundaries_on_admin_website(decimal depositRequestAmount, string expectedMessage)
        {
            //create a deposit request
            _playerManagerPage.SelectPlayer(_playerUsername);
            var offlineDepositRequestForm = _playerManagerPage.OpenOfflineDepositRequestForm();
            offlineDepositRequestForm.TryToSubmit(depositRequestAmount);

            Assert.That(offlineDepositRequestForm.ErrorMessage, Is.StringContaining(expectedMessage));
        }

        [TestCase(9, "Withdrawal failed. The entered amount is below the allowed value. Minimum value is 10.00.")]
        [TestCase(201, "Withdrawal failed. The entered amount exceeds the allowed value. Maximum value is 200.00.")]
        public void Cannot_withdraw_amount_outside_payment_settings_boundaries_on_mamber_website(decimal withdrawRequestAmount, string expectedMessage)
        {
            // create offline withdrawal request for a player
            var offlineWithdrawRequestForm = _playerManagerPage.OpenOfflineWithdrawRequestForm(_playerUsername);
            offlineWithdrawRequestForm.TryToSubmit(withdrawRequestAmount.ToString(), NotificationMethod.Email);

            Assert.AreEqual(expectedMessage, offlineWithdrawRequestForm.ValidationMessage);
        }
    }
}