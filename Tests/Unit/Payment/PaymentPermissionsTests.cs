using System;
using AFT.RegoV2.ApplicationServices.Payment;
using AFT.RegoV2.ApplicationServices.Security;
using AFT.RegoV2.Core.Payment.ApplicationServices;
using AFT.RegoV2.Core.Payment.Data;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Domain.Payment;
using AFT.RegoV2.Domain.Payment.Commands;
using AFT.RegoV2.Domain.Security.Interfaces;
using AFT.RegoV2.Infrastructure.DependencyResolution;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Helpers;
using AFT.RegoV2.Tests.Common.TestDoubles;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Payment
{
    internal class PaymentPermissionsTests : PermissionsTestsBase
    {
        private PaymentQueries _paymentQueries;

        public override void BeforeEach()
        {
            base.BeforeEach();

            _paymentQueries = Container.Resolve<PaymentQueries>();
        }

        [Test]
        public void Cannot_execute_PaymentQueries_without_permissions()
        {
            // Arrange
            LogWithNewUser(Modules.OfflineDepositRequests, Permissions.Add);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _paymentQueries.GetDepositByIdForConfirmation(new Guid()));
            Assert.Throws<InsufficientPermissionsException>(() => _paymentQueries.GetDepositByIdForViewRequest(new Guid()));
            Assert.Throws<InsufficientPermissionsException>(() => _paymentQueries.GetBanks());
            Assert.Throws<InsufficientPermissionsException>(() => _paymentQueries.GetPaymentSettings());
            Assert.Throws<InsufficientPermissionsException>(() => _paymentQueries.GetPaymentLevelsAsQueryable());
            Assert.Throws<InsufficientPermissionsException>(() => _paymentQueries.GetTransferSettings());
            Assert.Throws<InsufficientPermissionsException>(() => _paymentQueries.GetPlayerForNewBankAccount(new Guid()));
            Assert.Throws<InsufficientPermissionsException>(() => _paymentQueries.GetOfflineDeposits());
            Assert.Throws<InsufficientPermissionsException>(() => _paymentQueries.GetDepositsAsConfirmed());
            Assert.Throws<InsufficientPermissionsException>(() => _paymentQueries.GetDepositsAsVerified());
            Assert.Throws<InsufficientPermissionsException>(() => _paymentQueries.GetPlayerBankAccounts());
            Assert.Throws<InsufficientPermissionsException>(() => _paymentQueries.GetPlayerBankAccountForEdit(new Guid()));
            Assert.Throws<InsufficientPermissionsException>(() => _paymentQueries.GetPlayerBankAccountForSetCurrent(new Guid()));

        }

        [Test]
        public void Cannot_get_deposit_by_id_for_view_request_with_invalid_brand()
        {
            // Arrange
            var offlineDepositId = CreateNewOfflineDeposit();
            LogWithNewUser(Modules.OfflineDepositRequests, Permissions.View);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _paymentQueries.GetDepositByIdForViewRequest(offlineDepositId));
        }

        [Test]
        public void Cannot_get_deposit_by_id_for_confirmation_with_invalid_brand()
        {
            // Arrange
            var offlineDepositId = CreateNewOfflineDeposit();
            LogWithNewUser(Modules.OfflineDepositConfirmation, Permissions.Confirm);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _paymentQueries.GetDepositByIdForConfirmation(offlineDepositId));
        }

        [Test]
        public void Cannot_get_player_for_new_bank_account_with_invalid_brand()
        {
            // Arrange
            var playerId = CreateNewPlayerBankAccount().Player.Id;
            LogWithNewUser(Modules.PlayerBankAccount, Permissions.Add);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _paymentQueries.GetPlayerForNewBankAccount(playerId));
        }

        [Test]
        public void Cannot_get_player_bank_account_for_edit_with_invalid_brand()
        {
            // Arrange
            var playerBankAccountId = CreateNewPlayerBankAccount().Id;
            LogWithNewUser(Modules.PlayerBankAccount, Permissions.Edit);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _paymentQueries.GetPlayerBankAccountForEdit(playerBankAccountId));
        }

        [Test]
        public void Cannot_get_player_bank_account_for_set_current_with_invalid_brand()
        {
            // Arrange
            var playerBankAccountId = CreateNewPlayerBankAccount().Id;
            LogWithNewUser(Modules.PlayerBankAccount, Permissions.Edit);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _paymentQueries.GetPlayerBankAccountForSetCurrent(playerBankAccountId));
        }

        private Guid CreateNewOfflineDeposit()
        {
            var paymentTestHelper = Container.Resolve<PaymentTestHelper>();

            var playerId = CreateNewPlayer();
            var offlineDeposit = paymentTestHelper.CreateOfflineDeposit(playerId, 1M);

            return offlineDeposit.Id;
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
    }
}