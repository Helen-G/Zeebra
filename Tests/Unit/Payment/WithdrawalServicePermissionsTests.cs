using System;
using System.Globalization;
using System.Linq;
using AFT.RegoV2.ApplicationServices.Payment;
using AFT.RegoV2.ApplicationServices.Security;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Events;
using AFT.RegoV2.Core.Common.Events.Notifications;
using AFT.RegoV2.Core.Payment.Data;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Domain.BoundedContexts.Payment.Data;
using AFT.RegoV2.Domain.Security.Interfaces;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Helpers;
using AFT.RegoV2.Tests.Common.TestDoubles;
using AFT.RegoV2.WinService.Workers;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Payment
{
    internal class WithdrawalServicePermissionsTests : PermissionsTestsBase
    {
        private WithdrawalService _withdrawalService;
        private FakePaymentRepository _paymentRepository;
        private ISecurityProvider _securityProvider;

        public override void BeforeEach()
        {
            base.BeforeEach();

            _withdrawalService = Container.Resolve<WithdrawalService>();
            _paymentRepository = Container.Resolve<FakePaymentRepository>();
            _securityProvider = Container.Resolve<ISecurityProvider>();
            Container.Resolve<BonusWorker>().Start();
        }

        [Test]
        public void Cannot_execute_WithdrawalService_without_permissions()
        {
            // Arrange
            LogWithNewUser(Modules.OfflineDepositRequests, Permissions.View);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _withdrawalService.GetWithdrawalsForVerification());
            Assert.Throws<InsufficientPermissionsException>(() => _withdrawalService.GetWithdrawalsForAcceptance());
            Assert.Throws<InsufficientPermissionsException>(() => _withdrawalService.GetWithdrawalsForApproval());
            Assert.Throws<InsufficientPermissionsException>(() => _withdrawalService.GetWithdrawalsCanceled());
            Assert.Throws<InsufficientPermissionsException>(() => _withdrawalService.GetWithdrawalsFailedAutoWagerCheck());
            Assert.Throws<InsufficientPermissionsException>(() => _withdrawalService.GetWithdrawalsOnHold());
            Assert.Throws<InsufficientPermissionsException>(() => _withdrawalService.Request(new OfflineWithdrawRequest()));
            Assert.Throws<InsufficientPermissionsException>(() => _withdrawalService.Verify(new Guid(), "Some remark"));
            Assert.Throws<InsufficientPermissionsException>(() => _withdrawalService.Unverify(new Guid(), "Some remark"));
            Assert.Throws<InsufficientPermissionsException>(() => _withdrawalService.Approve(new Guid(), "Some remark"));
            Assert.Throws<InsufficientPermissionsException>(() => _withdrawalService.Reject(new Guid(), "Some remark"));
            Assert.Throws<InsufficientPermissionsException>(() => _withdrawalService.PassWager(new Guid(), "Some remark"));
            Assert.Throws<InsufficientPermissionsException>(() => _withdrawalService.FailWager(new Guid(), "Some remark"));
            Assert.Throws<InsufficientPermissionsException>(() => _withdrawalService.PassInvestigation(new Guid(), "Some remark"));
            Assert.Throws<InsufficientPermissionsException>(() => _withdrawalService.FailInvestigation(new Guid(), "Some remark"));
            Assert.Throws<InsufficientPermissionsException>(() => _withdrawalService.Accept(new Guid(), "Some remark"));
            Assert.Throws<InsufficientPermissionsException>(() => _withdrawalService.Revert(new Guid(), "Some remark"));
            Assert.Throws<InsufficientPermissionsException>(() => _withdrawalService.Cancel(new Guid(), "Some remark"));
            Assert.Throws<InsufficientPermissionsException>(() => _withdrawalService.SaveExemption(new Exemption()));
        }

        [Test]
        public void Cannot_request_offline_withdraw_with_invalid_brand()
        {
            // Arrange
            var playerBankAccount = CreateNewPlayerBankAccount();

            var offlineWithdrawalRequest = new OfflineWithdrawRequest
            {
                Amount = 1,
                NotificationType = NotificationType.None,
                BankAccountTime = _paymentRepository.PlayerBankAccounts.First().Created.ToString(),
                BankTime = _paymentRepository.Banks.First().Created.ToString(),
                PlayerBankAccountId = playerBankAccount.Id,
                Remarks = "asd",
                RequestedBy = _securityProvider.User.UserName
            };

            LogWithNewUser(Modules.OfflineWithdrawalRequest, Permissions.Add);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _withdrawalService.Request(offlineWithdrawalRequest));
        }

        [Test]
        public void Cannot_verify_offline_withdraw_with_invalid_brand()
        {
            // Arrange
            var offlineWithdrawId = CreateOfflineWithdraw();

            LogWithNewUser(Modules.OfflineWithdrawalVerification, Permissions.Verify);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _withdrawalService.Verify(offlineWithdrawId, "Some remark"));
        }

        [Test]
        public void Cannot_unverify_offline_withdraw_with_invalid_brand()
        {
            // Arrange
            var offlineWithdrawId = CreateOfflineWithdraw();

            LogWithNewUser(Modules.OfflineWithdrawalVerification, Permissions.Unverify);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _withdrawalService.Unverify(offlineWithdrawId, "Some remark"));
        }

        [Test]
        public void Cannot_approve_offline_withdraw_with_invalid_brand()
        {
            // Arrange
            var offlineWithdrawId = CreateOfflineWithdraw();

            LogWithNewUser(Modules.OfflineWithdrawalApproval, Permissions.Approve);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _withdrawalService.Approve(offlineWithdrawId, "Some remark"));
        }

        [Test]
        public void Cannot_reject_offline_withdraw_with_invalid_brand()
        {
            // Arrange
            var offlineWithdrawId = CreateOfflineWithdraw();

            LogWithNewUser(Modules.OfflineWithdrawalApproval, Permissions.Reject);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _withdrawalService.Reject(offlineWithdrawId, "Some remark"));
        }

        [Test]
        public void Cannot_pass_wager_offline_withdraw_with_invalid_brand()
        {
            // Arrange
            var offlineWithdrawId = CreateOfflineWithdraw();

            LogWithNewUser(Modules.OfflineWithdrawalWagerCheck, Permissions.Pass);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _withdrawalService.PassWager(offlineWithdrawId, "Some remark"));
        }

        [Test]
        public void Cannot_fail_wager_offline_withdraw_with_invalid_brand()
        {
            // Arrange
            var offlineWithdrawId = CreateOfflineWithdraw();

            LogWithNewUser(Modules.OfflineWithdrawalWagerCheck, Permissions.Fail);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _withdrawalService.FailWager(offlineWithdrawId, "Some remark"));
        }

        [Test]
        public void Cannot_pass_investigation_offline_withdraw_with_invalid_brand()
        {
            // Arrange
            var offlineWithdrawId = CreateOfflineWithdraw();

            LogWithNewUser(Modules.OfflineWithdrawalInvestigation, Permissions.Pass);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _withdrawalService.PassInvestigation(offlineWithdrawId, "Some remark"));
        }

        [Test]
        public void Cannot_fail_investigation_offline_withdraw_with_invalid_brand()
        {
            // Arrange
            var offlineWithdrawId = CreateOfflineWithdraw();

            LogWithNewUser(Modules.OfflineWithdrawalInvestigation, Permissions.Fail);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _withdrawalService.FailInvestigation(offlineWithdrawId, "Some remark"));
        }

        [Test]
        public void Cannot_accept_offline_withdraw_with_invalid_brand()
        {
            // Arrange
            var offlineWithdrawId = CreateOfflineWithdraw();

            LogWithNewUser(Modules.OfflineWithdrawalAcceptance, Permissions.Accept);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _withdrawalService.Accept(offlineWithdrawId, "Some remark"));
        }

        [Test]
        public void Cannot_revert_offline_withdraw_with_invalid_brand()
        {
            // Arrange
            var offlineWithdrawId = CreateOfflineWithdraw();

            LogWithNewUser(Modules.OfflineWithdrawalAcceptance, Permissions.Revert);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _withdrawalService.Revert(offlineWithdrawId, "Some remark"));
        }

        [Test]
        public void Cannot_cancel_offline_withdraw_with_invalid_brand()
        {
            // Arrange
            var offlineWithdrawId = CreateOfflineWithdraw();

            LogWithNewUser(Modules.OfflineWithdrawalAcceptance, Permissions.Revert);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _withdrawalService.Cancel(offlineWithdrawId, "Some remark"));
        }

        [Test]
        public void Cannot_save_exemption_offline_withdraw_with_invalid_brand()
        {
            // Arrange
            var playerBankAccount = CreateNewPlayerBankAccount();
            var exemption = new Exemption
            {
                PlayerId = playerBankAccount.Player.Id,
                Exempt = true,
                ExemptFrom = DateTime.Now.ToString(CultureInfo.InvariantCulture),
                ExemptTo = DateTime.Now.ToString(CultureInfo.InvariantCulture),
                ExemptLimit = 1
            };

            LogWithNewUser(Modules.OfflineWithdrawalExemption, Permissions.Exempt);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _withdrawalService.SaveExemption(exemption));
        }

        private Guid CreateNewPlayer()
        {
            var brandTestHelper = Container.Resolve<BrandTestHelper>();
            var playerTestHelper = Container.Resolve<PlayerTestHelper>();

            var licensee = brandTestHelper.CreateLicensee();
            var brand = brandTestHelper.CreateBrand(licensee, isActive: true);
            var playerId = playerTestHelper.CreatePlayer(null, true, brand.Id);

            return playerId;
        }

        private PlayerBankAccount CreateNewPlayerBankAccount()
        {
            var paymentTestHelper = Container.Resolve<PaymentTestHelper>();

            var playerId = CreateNewPlayer();
            var playerBankAccount = paymentTestHelper.CreatePlayerBankAccount(playerId);


            return playerBankAccount;
        }

        private Guid CreateOfflineWithdraw()
        {
            var paymentTestHelper = Container.Resolve<PaymentTestHelper>();

            var playerBankAccount = CreateNewPlayerBankAccount();
            paymentTestHelper.MakeDeposit(playerBankAccount.Player.Id, 1000);

            var offlineWithdrawalRequest = new OfflineWithdrawRequest
            {
                Amount = 1,
                NotificationType = NotificationType.None,
                BankAccountTime = _paymentRepository.PlayerBankAccounts.First().Created.ToString(),
                BankTime = _paymentRepository.Banks.First().Created.ToString(),
                PlayerBankAccountId = playerBankAccount.Id,
                Remarks = "asd",
                RequestedBy = _securityProvider.User.UserName
            };

            var offlineWithdrawalResponse = _withdrawalService.Request(offlineWithdrawalRequest);

            return offlineWithdrawalResponse.Id;
        }
    }
}