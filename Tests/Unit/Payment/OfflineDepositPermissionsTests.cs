using System;
using AFT.RegoV2.ApplicationServices.Payment;
using AFT.RegoV2.ApplicationServices.Security;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Domain.Payment.Commands;
using AFT.RegoV2.Domain.Payment.Data;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Helpers;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Payment
{
    internal class OfflineDepositPermissionsTests : PermissionsTestsBase
    {
        private OfflineDepositCommands _offlineDepositCommands;
        private Core.Player.Data.Player _player;
        private BankAccount _bankAccount;

        public override void BeforeEach()
        {
            base.BeforeEach();

            _offlineDepositCommands = Container.Resolve<OfflineDepositCommands>();

            var brandTestHelper = Container.Resolve<BrandTestHelper>();
            var paymentTestHelper = Container.Resolve<PaymentTestHelper>();
            var playerTestHelper = Container.Resolve<PlayerTestHelper>();

            var licensee = brandTestHelper.CreateLicensee();
            var brand = brandTestHelper.CreateBrand(licensee, isActive: true);

            _player = playerTestHelper.CreatePlayer(true, brand.Id);
            _bankAccount = paymentTestHelper.CreateBankAccount(brand.Id, brand.DefaultCurrency);
        }

        [Test]
        public void Cannot_execute_OfflineDepositCommands_without_permissions()
        {
            // Arrange
            LogWithNewUser(Modules.OfflineDepositRequests, Permissions.View);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _offlineDepositCommands.Submit(new OfflineDepositRequest()));
            Assert.Throws<InsufficientPermissionsException>(() => _offlineDepositCommands.Confirm(new OfflineDepositConfirm(), new byte[1], new byte[1], new byte[1]));
            Assert.Throws<InsufficientPermissionsException>(() => _offlineDepositCommands.Verify(new Guid(), "Verify remark"));
            Assert.Throws<InsufficientPermissionsException>(() => _offlineDepositCommands.Unverify(new Guid(), "Unverify remark", UnverifyReasons.D0001));
            Assert.Throws<InsufficientPermissionsException>(() => _offlineDepositCommands.Approve(new OfflineDepositApprove()));
            Assert.Throws<InsufficientPermissionsException>(() => _offlineDepositCommands.Reject(new Guid(), "Reject remark"));
        }

        [Test]
        public void Cannot_submit_offline_deposit_with_invalid_brand()
        {
            // Arrange
            LogWithNewUser(Modules.OfflineDepositRequests, Permissions.Add);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() =>
                _offlineDepositCommands.Submit(new OfflineDepositRequest
                {
                    PlayerId = _player.Id,
                    BankAccountId = _bankAccount.Id,
                    Amount = 1020.3M
                })
                );
        }

        [Test]
        public void Cannot_confirm_offline_deposit_with_invalid_brand()
        {
            // Arrange
            var offlineDeposit = GetNewOfflineDeposit(1020.05M, "OD12345678");
            var depositConfirm = new OfflineDepositConfirm
            {
                Id = offlineDeposit.Id,
                PlayerAccountName = "Fry Philip",
                PlayerAccountNumber = "Test PlayerAccountName",
                ReferenceNumber = "Test PlayerAccountName",
                Amount = 2345.56M,
                TransferType = TransferType.DifferentBank,
                OfflineDepositType = DepositMethod.ATM,
                BankId = _bankAccount.Bank.Id
            };

            LogWithNewUser(Modules.OfflineDepositConfirmation, Permissions.Confirm);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() =>
                _offlineDepositCommands.Confirm(depositConfirm, new byte[1], new byte[1], new byte[1]));
        }

        [Test]
        public void Cannot_verify_offline_deposit_with_invalid_brand()
        {
            // Arrange
            var offlineDeposit = GetNewOfflineDeposit(1020.05M, "OD12345678");
            _offlineDepositCommands.Confirm(new OfflineDepositConfirm { Id = offlineDeposit.Id, PlayerAccountName = "Fry Philip" }, new byte[1], new byte[1], new byte[1]);

            // Act
            LogWithNewUser(Modules.OfflineDepositVerification, Permissions.Verify);

            Assert.Throws<InsufficientPermissionsException>(() =>
                _offlineDepositCommands.Verify(offlineDeposit.Id, "Verify remark"));
        }

        [Test]
        public void Cannot_unverify_offline_deposit_with_invalid_brand()
        {
            // Arrange
            var offlineDeposit = GetNewOfflineDeposit(1020.05M, "OD12345678");
            _offlineDepositCommands.Confirm(new OfflineDepositConfirm { Id = offlineDeposit.Id, PlayerAccountName = "Fry Philip" }, new byte[1], new byte[1], new byte[1]);

            // Act
            LogWithNewUser(Modules.OfflineDepositVerification, Permissions.Unverify);

            Assert.Throws<InsufficientPermissionsException>(() =>
                _offlineDepositCommands.Unverify(offlineDeposit.Id, "Unverify remark", UnverifyReasons.D0001));
        }

        [Test]
        public void Cannot_approve_offline_deposit_with_invalid_brand()
        {
            // Arrange
            var offlineDeposit = GetNewOfflineDeposit(1020.05M, "OD12345678");
            _offlineDepositCommands.Confirm(new OfflineDepositConfirm { Id = offlineDeposit.Id, PlayerAccountName = "Fry Philip" }, new byte[1], new byte[1], new byte[1]);
            _offlineDepositCommands.Verify(offlineDeposit.Id, "Verify remark");
            var offlineDepositApprove = new OfflineDepositApprove
            {
                Id = offlineDeposit.Id,
                ActualAmount = 9988.77M,
                Fee = 10.50M,
                PlayerRemark = "Player remark",
                Remark = "Approve remark"
            };

            // Act
            LogWithNewUser(Modules.OfflineDepositApproval, Permissions.Approve);

            Assert.Throws<InsufficientPermissionsException>(() =>
                _offlineDepositCommands.Approve(offlineDepositApprove));
        }

        [Test]
        public void Cannot_reject_offline_deposit_with_invalid_brand()
        {
            // Arrange
            var offlineDeposit = GetNewOfflineDeposit(1020.05M, "OD12345678");
            _offlineDepositCommands.Confirm(new OfflineDepositConfirm { Id = offlineDeposit.Id, PlayerAccountName = "Fry Philip" }, new byte[1], new byte[1], new byte[1]);
            _offlineDepositCommands.Verify(offlineDeposit.Id, "Verify remark");

            // Act
            LogWithNewUser(Modules.OfflineDepositApproval, Permissions.Reject);

            Assert.Throws<InsufficientPermissionsException>(() =>
                _offlineDepositCommands.Reject(offlineDeposit.Id, "Reject remark"));
        }

        OfflineDeposit GetNewOfflineDeposit(decimal amount, string transactionNumber)
        {
            var offlineDeposit = _offlineDepositCommands.Submit(new OfflineDepositRequest
            {
                PlayerId = _player.Id,
                BankAccountId = _bankAccount.Id,
                Amount = amount
            });
            offlineDeposit.TransactionNumber = transactionNumber;
            offlineDeposit.Player = new Core.Payment.Data.Player
            {
                Id = _player.Id,
                Username = _player.Username,
                FirstName = _player.FirstName,
                LastName = _player.LastName
            };
            offlineDeposit.Brand = new Domain.Payment.Data.Brand();
            return offlineDeposit;
        }
    }
}
