using System;
using AFT.RegoV2.ApplicationServices.Payment;
using AFT.RegoV2.ApplicationServices.Player;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Common;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Payment;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Payment.Validators;
using AFT.RegoV2.Core.Player.Data;
using AFT.RegoV2.Core.Player.Validators;
using AFT.RegoV2.Domain.Payment.Commands;
using AFT.RegoV2.Domain.Payment.Data;
using AFT.RegoV2.Domain.Security.Interfaces;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Helpers;
using AFT.RegoV2.Tests.Common.TestDoubles;
using FluentAssertions;
using Microsoft.Practices.Unity;
using Moq;
using NUnit.Framework;
using ServiceStack.ServiceHost;
using VipLevel = AFT.RegoV2.Core.Player.Data.VipLevel;

namespace AFT.RegoV2.Tests.Unit.Payment
{
    internal class OfflineDepositTests : AdminWebsiteUnitTestsBase
    {
        private OfflineDepositCommands _commandsHandler;
        private Mock<IPlayerQueries> _playerQueriesMock;
        private Mock<IPlayerService> _playerServiceMock;
        private Core.Player.Data.Player _player;
        private BankAccount _bankAccount;
        private PaymentQueries _paymentQueries;

        public override void BeforeEach()
        {
            base.BeforeEach();

            var paymentRepositoryMock = new FakePaymentRepository();
            _playerQueriesMock = new Mock<IPlayerQueries>();
            _playerServiceMock = new Mock<IPlayerService>();
            var brandQueries = Container.Resolve<BrandQueries>();
            _paymentQueries = new PaymentQueries(paymentRepositoryMock, _playerQueriesMock.Object, brandQueries);
            var fileStorageMock = new Mock<IFileStorage>();
            var busMock = new Mock<IEventBus>();
            var walletCommandsMock = new Mock<IWalletCommands>();
            var offlineDepositValidator = new Mock<IOfflineDepositValidator>();
            var securityProvider = Container.Resolve<ISecurityProvider>();
            var playerIdentityValidator = new Mock<IPlayerIdentityValidator>();
            playerIdentityValidator.Setup(o => o.Validate(It.IsAny<Guid>(), It.IsAny<TransactionType>()));

            _commandsHandler = new OfflineDepositCommands(
                paymentRepositoryMock,
                _paymentQueries,
                fileStorageMock.Object,
                busMock.Object,
                walletCommandsMock.Object,
                offlineDepositValidator.Object,
                securityProvider,
                playerIdentityValidator.Object);

            _player = new Core.Player.Data.Player
            {
                Id = Guid.NewGuid(),
                FirstName = "Fry",
                LastName = "Philip",
                CurrencyCode = "CAD",
                BrandId = Guid.NewGuid(),
                VipLevel = new VipLevel()
                {
                    Id = Guid.NewGuid()
                }
            };
            _bankAccount = new BankAccount { Id = Guid.NewGuid(), CurrencyCode = "CAD", Status = BankAccountStatus.Active };

            paymentRepositoryMock.BankAccounts.Add(_bankAccount);
            _playerQueriesMock.Setup(x => x.GetPlayer(_player.Id)).Returns(_player);
            _playerServiceMock.Setup(x => x.GetVipLevelIdByPlayerId(_player.Id)).Returns(_player.VipLevel.Id.ToString);
            offlineDepositValidator
                .Setup(o => o.ValidatePaymentSetting(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<decimal>()))
                .Callback((Guid param1, Guid param2, decimal param3) => { });
        }

        [Test]
        public void Submit_command_should_throw_exception_if_player_not_found()
        {
            // Arrange

            // Act
            Action act = () => _commandsHandler.Submit(new OfflineDepositRequest());

            // Assert
            act.ShouldThrow<ArgumentException>().WithMessage("Player not found");
        }

        [Test]
        public void Submit_command_should_throw_exception_if_bank_account_not_found()
        {
            // Arrange
            _playerQueriesMock.Setup(x => x.GetPlayer(_player.Id)).Returns(_player);

            // Act
            Action act = () => _commandsHandler.Submit(new OfflineDepositRequest { PlayerId = _player.Id });

            // Assert
            act.ShouldThrow<ArgumentException>().WithMessage("Bank account not found");
        }

        [Test]
        public void Submit_command_should_throw_exception_if_bank_account_and_player_accounts_have_different_currencies()
        {
            // Arrange
            _bankAccount.CurrencyCode = "UAH";

            // Act
            Action act = () => _commandsHandler.Submit(new OfflineDepositRequest { PlayerId = _player.Id, BankAccountId = _bankAccount.Id });

            // Assert
            act.ShouldThrow<ArgumentException>().WithMessage("app:payment.deposit.differentCurrenciesErrorMessage");
        }

        [Test]
        public void Submit_command_should_create_new_offline_deposit()
        {
            // Arrange

            // Act
            var offlineDeposit = _commandsHandler.Submit(new OfflineDepositRequest
            {
                PlayerId = _player.Id,
                BankAccountId = _bankAccount.Id,
                Amount = 1020.3M
            });

            // Assert
            offlineDeposit.Id.Should().NotBeEmpty();
            offlineDeposit.Created.Should().BeCloseTo(DateTime.Now, 1000);
            offlineDeposit.TransactionNumber.Should().NotBeEmpty();
            offlineDeposit.Status.ShouldBeEquivalentTo(OfflineDepositStatus.New);
            offlineDeposit.PaymentMethod.ShouldBeEquivalentTo(PaymentMethod.OfflineBank);
            offlineDeposit.DepositType.ShouldBeEquivalentTo(DepositType.Offline);
            offlineDeposit.NotificationMethod.ShouldBeEquivalentTo(NotificationMethod.Email);
            offlineDeposit.Amount.ShouldBeEquivalentTo(1020.3M);
            offlineDeposit.BrandId.ShouldBeEquivalentTo(_player.BrandId);
            offlineDeposit.PlayerId.ShouldBeEquivalentTo(_player.Id);
            offlineDeposit.BankAccountId.ShouldBeEquivalentTo(_bankAccount.Id);
            offlineDeposit.CurrencyCode.ShouldBeEquivalentTo(_bankAccount.CurrencyCode);
        }

        [Test]
        public void Confirm_command_requires_copies_of_player_ids_if_player_and_account_holder_names_are_different()
        {
            // Arrange
            var offlineDeposit = GetNewOfflineDeposit(1530.15M, "OD999888777");
            var offlineDepositConfirm = new OfflineDepositConfirm();
            offlineDepositConfirm.Id = offlineDeposit.Id;
            offlineDepositConfirm.PlayerAccountName = "Test";

            // Act
            Action act0 = () => _commandsHandler.Confirm(offlineDepositConfirm, null, null, null);
            Action act1 = () => _commandsHandler.Confirm(offlineDepositConfirm, new byte[1], null, null);
            Action act2 = () => _commandsHandler.Confirm(offlineDepositConfirm, null, new byte[1], null);

            // Assert
            act0.ShouldThrow<ArgumentException>().WithMessage("Front and back copy of ID or receipt should be uploaded.");
            act1.ShouldThrow<ArgumentException>().WithMessage("Front and back copy of ID or receipt should be uploaded.");
            act2.ShouldThrow<ArgumentException>().WithMessage("Front and back copy of ID or receipt should be uploaded.");
        }

        [Test]
        public void Confirm_offline_deposit()
        {
            // Arrange
            var offlineDeposit = GetNewOfflineDeposit(1020.05M, "OD12345678");
            var depositConfirm = new OfflineDepositConfirm();
            depositConfirm.Id = offlineDeposit.Id;
            depositConfirm.PlayerAccountName = "Fry Philip";
            depositConfirm.PlayerAccountNumber = "Test PlayerAccountName";
            depositConfirm.ReferenceNumber = "Test PlayerAccountName";
            depositConfirm.Amount = 2345.56M;
            depositConfirm.TransferType = TransferType.DifferentBank;
            depositConfirm.OfflineDepositType = DepositMethod.ATM;

            // Act
            _commandsHandler.Confirm(depositConfirm, null, null, null);

            // Assert
            offlineDeposit.PlayerAccountName.ShouldBeEquivalentTo(depositConfirm.PlayerAccountName);
            offlineDeposit.PlayerAccountNumber.ShouldBeEquivalentTo(depositConfirm.PlayerAccountNumber);
            offlineDeposit.ReferenceNumber.ShouldBeEquivalentTo(depositConfirm.ReferenceNumber);
            offlineDeposit.Amount.ShouldBeEquivalentTo(depositConfirm.Amount);
            offlineDeposit.TransferType.ShouldBeEquivalentTo(depositConfirm.TransferType);
            offlineDeposit.DepositMethod.ShouldBeEquivalentTo(depositConfirm.OfflineDepositType);
            offlineDeposit.Status.ShouldBeEquivalentTo(OfflineDepositStatus.Processing);
        }

        [Test]
        public void Verify_offline_deposit()
        {
            // Arrange
            Container.Resolve<SecurityTestHelper>().SignInUser();
            var offlineDeposit = GetNewOfflineDeposit(1020.05M, "OD12345678");
            _commandsHandler.Confirm(new OfflineDepositConfirm { Id = offlineDeposit.Id, PlayerAccountName = "Fry Philip" }, null, null, null);

            // Act
            _commandsHandler.Verify(offlineDeposit.Id, "Verify remark");

            // Assert
            offlineDeposit.Remark.ShouldBeEquivalentTo("Verify remark");
            offlineDeposit.Status.ShouldBeEquivalentTo(OfflineDepositStatus.Verified);
        }

        [Test]
        public void Unverify_offline_deposit()
        {
            // Arrange
            Container.Resolve<SecurityTestHelper>().SignInUser();
            var offlineDeposit = GetNewOfflineDeposit(1020.05M, "OD12345678");
            _commandsHandler.Confirm(new OfflineDepositConfirm { Id = offlineDeposit.Id, PlayerAccountName = "Fry Philip" }, null, null, null);

            // Act
            _commandsHandler.Unverify(offlineDeposit.Id, "Unverify remark", UnverifyReasons.D0001);

            // Assert
            offlineDeposit.Remark.ShouldBeEquivalentTo("Unverify remark");
            offlineDeposit.Status.ShouldBeEquivalentTo(OfflineDepositStatus.Unverified);
        }

        [Test]
        public void Approve_offline_deposit()
        {
            // Arrange
            Container.Resolve<SecurityTestHelper>().SignInUser();
            var offlineDeposit = GetNewOfflineDeposit(1020.05M, "OD12345678");
            _commandsHandler.Confirm(new OfflineDepositConfirm { Id = offlineDeposit.Id, PlayerAccountName = "Fry Philip" }, null, null, null);
            _commandsHandler.Verify(offlineDeposit.Id, "Verify remark");
            var offlineDepositApprove = new OfflineDepositApprove();
            offlineDepositApprove.Id = offlineDeposit.Id;
            offlineDepositApprove.ActualAmount = 9988.77M;
            offlineDepositApprove.Fee = 10.50M;
            offlineDepositApprove.PlayerRemark = "Player remark";
            offlineDepositApprove.Remark = "Approve remark";

            // Act
            _commandsHandler.Approve(offlineDepositApprove);

            // Assert
            offlineDeposit.ActualAmount.ShouldBeEquivalentTo(9988.77M);
            offlineDeposit.Fee.ShouldBeEquivalentTo(10.50M);
            offlineDeposit.PlayerRemark.ShouldBeEquivalentTo("Player remark");
            offlineDeposit.Remark.ShouldBeEquivalentTo("Approve remark");
            offlineDeposit.Status.ShouldBeEquivalentTo(OfflineDepositStatus.Approved);
        }

        [Test]
        public void Reject_offline_deposit()
        {
            // Arrange
            Container.Resolve<SecurityTestHelper>().SignInUser();
            var offlineDeposit = GetNewOfflineDeposit(1020.05M, "OD12345678");
            _commandsHandler.Confirm(new OfflineDepositConfirm { Id = offlineDeposit.Id, PlayerAccountName = "Fry Philip" }, null, null, null);
            _commandsHandler.Verify(offlineDeposit.Id, "Verify remark");

            // Act
            _commandsHandler.Reject(offlineDeposit.Id, "Reject remark");

            // Assert
            offlineDeposit.Remark.ShouldBeEquivalentTo("Reject remark");
            offlineDeposit.Status.ShouldBeEquivalentTo(OfflineDepositStatus.Rejected);
        }

        [Test]
        public void Confirm_offline_deposit_with_copies_of_player_ids()
        {
            // Arrange
            var offlineDeposit = GetNewOfflineDeposit(1020.05M, "OD12345678");
            var depositConfirm = new OfflineDepositConfirm();
            depositConfirm.Id = offlineDeposit.Id;
            depositConfirm.PlayerAccountName = "Test PlayerAccountName";
            depositConfirm.PlayerAccountNumber = "Test PlayerAccountName";
            depositConfirm.ReferenceNumber = "Test PlayerAccountName";
            depositConfirm.Amount = 2345.56M;
            depositConfirm.TransferType = TransferType.DifferentBank;
            depositConfirm.OfflineDepositType = DepositMethod.ATM;
            depositConfirm.IdFrontImage = "Test IdFrontImage.png";
            depositConfirm.IdBackImage = "Test IdBackImage.jpg";

            // Act
            _commandsHandler.Confirm(depositConfirm, new byte[] { 1, 2, 3 }, new byte[] { 4, 5, 6 }, null);

            // Assert
            offlineDeposit.PlayerAccountName.ShouldBeEquivalentTo(depositConfirm.PlayerAccountName);
            offlineDeposit.PlayerAccountNumber.ShouldBeEquivalentTo(depositConfirm.PlayerAccountNumber);
            offlineDeposit.ReferenceNumber.ShouldBeEquivalentTo(depositConfirm.ReferenceNumber);
            offlineDeposit.Amount.ShouldBeEquivalentTo(depositConfirm.Amount);
            offlineDeposit.TransferType.ShouldBeEquivalentTo(depositConfirm.TransferType);
            offlineDeposit.DepositMethod.ShouldBeEquivalentTo(depositConfirm.OfflineDepositType);
            offlineDeposit.IdFrontImage.ShouldBeEquivalentTo(offlineDeposit.Id + "-FrontId.png");
            offlineDeposit.IdBackImage.ShouldBeEquivalentTo(offlineDeposit.Id + "-BackId.jpg");
            offlineDeposit.Status.ShouldBeEquivalentTo(OfflineDepositStatus.Processing);
        }

        OfflineDeposit GetNewOfflineDeposit(decimal amount, string transactionNumber)
        {
            var offlineDeposit = _commandsHandler.Submit(new OfflineDepositRequest
            {
                PlayerId = _player.Id,
                BankAccountId = _bankAccount.Id,
                Amount = 1020.3M
            });
            offlineDeposit.TransactionNumber = transactionNumber;
            offlineDeposit.Player = new Core.Payment.Data.Player();
            offlineDeposit.Player.Id = _player.Id;
            offlineDeposit.Player.Username = _player.Username;
            offlineDeposit.Player.FirstName = _player.FirstName;
            offlineDeposit.Player.LastName = _player.LastName;
            offlineDeposit.Brand = new Domain.Payment.Data.Brand();
            return offlineDeposit;
        }
    }
}
