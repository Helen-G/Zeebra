using System.Globalization;
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
    class OfflineDepositConfirmTests : SeleniumBaseForAdminWebsite
    {
        private RegistrationDataForMemberWebsite _data;
        private static string _username;
        private OfflineDepositConfirmPage _offlineDepositConfirmPage;
        private string _referenceCode;
        private const decimal DepositAmount = 100.25M;

        public override void BeforeAll()
        {
            base.BeforeAll();
            _data = _container.Resolve<PlayerTestHelper>().CreatePlayerForMemberWebsite(currencyCode: "CAD");
            _username = _data.Username;
        }

        public override void BeforeEach()
        {
            base.BeforeEach();
            _driver.Logout();
            var dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();
            var playerManagerPage = dashboardPage.Menu.ClickPlayerManagerMenuItem();
            playerManagerPage.SelectPlayer(_username);

            var offlineDepositForm = playerManagerPage.OpenOfflineDepositRequestForm();
            var submittedOfflineDeposit = offlineDepositForm.Submit(DepositAmount);
            _referenceCode = submittedOfflineDeposit.ReferenceCode;
            _offlineDepositConfirmPage = submittedOfflineDeposit.Menu.ClickOfflineDepositConfirmMenuItem();
        }

       [Test]
        public void Player_ids_are_required_if_player_and_account_holder_names_are_different()
        {
            _offlineDepositConfirmPage.SelectOfflineDepositRequest(_username, _referenceCode);
            var depositConfirmForm = _offlineDepositConfirmPage.ClickConfirmButton();
            var validDepositConfirmData = TestDataGenerator.CreateValidDepositConfirmData("Test");
            depositConfirmForm.SubmitValidDepositConfirm(validDepositConfirmData);

            Assert.AreEqual(depositConfirmForm.IdFrontImageValidationMessage.Displayed, true);
            Assert.AreEqual(depositConfirmForm.IdFrontImageValidationMessage.Text, "Please upload the front page of the player's ID.");

            Assert.AreEqual(depositConfirmForm.IdBackImageValidationMessage.Displayed, true);
            Assert.AreEqual(depositConfirmForm.IdBackImageValidationMessage.Text, "Please upload the back page of the player's ID.");
        }

        [Test]
        public void Player_ids_uploading_on_deposit_confirmation_page()
        {
            _offlineDepositConfirmPage.SelectOfflineDepositRequest(_username, _referenceCode);
            var depositConfirmForm = _offlineDepositConfirmPage.ClickConfirmButton();
            var validDepositConfirmData = TestDataGenerator.CreateValidDepositConfirmData("Wrong player name");
            var submittedDepositConfirm = depositConfirmForm.SubmitValidDepositConfirm(validDepositConfirmData, true);

            Assert.AreEqual(depositConfirmForm.IdFrontImageValidationMessage.Displayed, false);
            Assert.AreEqual(depositConfirmForm.IdFrontImageValidationMessage.Text, "");

            Assert.AreEqual(depositConfirmForm.IdBackImageValidationMessage.Displayed, false);
            Assert.AreEqual(depositConfirmForm.IdBackImageValidationMessage.Text, "");

            Assert.AreEqual(submittedDepositConfirm.GetConfirmationMessage, "Offline deposit request has been confirmed successfully");
            Assert.AreEqual(submittedDepositConfirm.ReferenceCode, _referenceCode);
            Assert.AreEqual(submittedDepositConfirm.BankAccountID, depositConfirmForm.BankAccountID);
            Assert.AreEqual(submittedDepositConfirm.Amount, validDepositConfirmData.Amount.ToString(CultureInfo.InvariantCulture));
            Assert.AreEqual(submittedDepositConfirm.Remark, validDepositConfirmData.Remarks);
        }

    }
}
