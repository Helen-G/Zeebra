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
    internal class PlayerDepositApproveTests : SeleniumBaseForAdminWebsite
    {
        private RegistrationDataForMemberWebsite _data;
        private static string _username;
        private string _referenceCode;
        private string _amount;
        private const string Fee = "10";
        private PlayerDepositApprovePage _playerDepositApprovePage;

        public override void BeforeAll()
        {
            base.BeforeAll();
            _data = _container.Resolve<PlayerTestHelper>().CreatePlayerForMemberWebsite("CAD");
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
            var submittedOfflineDeposit = offlineDepositForm.Submit(100.25M);
            _referenceCode = submittedOfflineDeposit.ReferenceCode;
            _amount = submittedOfflineDeposit.Amount;

            var offlineDepositRequestsPage = submittedOfflineDeposit.Menu.ClickOfflineDepositConfirmMenuItem();
            offlineDepositRequestsPage.SelectOfflineDepositRequest(_username, _referenceCode);
            var depositConfirmForm = offlineDepositRequestsPage.ClickConfirmButton();
            var validDepositConfirmData = TestDataGenerator.CreateValidDepositConfirmData(_data.FullName);
            var viewOfflineDepositConfirmForm = depositConfirmForm.SubmitValidDepositConfirm(validDepositConfirmData);

            var playerDepositVerifyForm = viewOfflineDepositConfirmForm.Menu.ClickPlayerDepositVerifyItem();
            playerDepositVerifyForm.FilterGrid(_username);
            playerDepositVerifyForm.SelectConfirmedDeposit(_referenceCode);
            var verifyForm = playerDepositVerifyForm.OpenVerifyForm();
            var viewVerifyForm = verifyForm.Submit();
            _playerDepositApprovePage = viewVerifyForm.Menu.ClickPlayerDepositApproveItem();
        }

      [Test]
        public void Can_reject_deposit()
        {
            _playerDepositApprovePage.FilterGrid(_username);
            _playerDepositApprovePage.SelectVerifiedDeposit(_referenceCode);
            var rejectForm = _playerDepositApprovePage.OpenRejectForm();
            var depositStatusBeforeApproval = rejectForm.Status;
            var submittedRejectForm = rejectForm.Submit(_amount, Fee);
            var depositStatusAfterRejection = submittedRejectForm.Status;

            Assert.AreEqual("Rejected", depositStatusAfterRejection);
            Assert.AreNotEqual(depositStatusBeforeApproval, depositStatusAfterRejection);
            Assert.AreEqual("Offline deposit request has been rejected", rejectForm.ConfirmationMessage);
        }

    }
}
