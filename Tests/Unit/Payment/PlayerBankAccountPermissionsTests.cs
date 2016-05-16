using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.ApplicationServices.Payment;
using AFT.RegoV2.ApplicationServices.Player;
using AFT.RegoV2.ApplicationServices.Security;
using AFT.RegoV2.Core.Security.ApplicationServices;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Domain.Payment.Commands;
using AFT.RegoV2.Domain.Security.Interfaces;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Helpers;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Payment
{
    internal class PlayerBankAccountPermissionsTests : PermissionsTestsBase
    {
        private PlayerBankAccountCommands _playerBankAccountCommands;
        private PlayerBankAccountQueries _playerBankAccountQueries;

        public override void BeforeEach()
        {
            base.BeforeEach();

            _playerBankAccountCommands = Container.Resolve<PlayerBankAccountCommands>();
            _playerBankAccountQueries = Container.Resolve<PlayerBankAccountQueries>();
        }

        [Test]
        public void Cannot_execute_PlayerBankAccountQueries_without_permissions()
        {
            // Arrange
            LogWithNewUser(Modules.PlayerBankAccount, Permissions.Add);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _playerBankAccountQueries.GetPendingPlayerBankAccounts());
        }

        [Test]
        public void Cannot_execute_PlayerBankAccountCommands_without_permissions()
        {
            // Arrange
            LogWithNewUser(Modules.BankAccounts, Permissions.View);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _playerBankAccountCommands.Add(new EditPlayerBankAccountCommand()));
            Assert.Throws<InsufficientPermissionsException>(() => _playerBankAccountCommands.Edit(new EditPlayerBankAccountCommand()));
            Assert.Throws<InsufficientPermissionsException>(() => _playerBankAccountCommands.SetCurrent(new Guid()));
            Assert.Throws<InsufficientPermissionsException>(() => _playerBankAccountCommands.Verify(new Guid(), "Some remark"));
            Assert.Throws<InsufficientPermissionsException>(() => _playerBankAccountCommands.Reject(new Guid(), "Some remark"));
            Assert.Throws<InsufficientPermissionsException>(() => _playerBankAccountQueries.GetPlayerBankAccounts());
            Assert.Throws<InsufficientPermissionsException>(() => _playerBankAccountQueries.GetPlayerBankAccounts(new Guid()));
        }

        [Test]
        public void Cannot_add_player_bank_account_with_invalid_brand()
        {
            // Arrange
            var data = CreatePlayerBankAccountData();
            var playerId = CreateNewPlayer();
            data.PlayerId = playerId;
            data.Bank = CreateNewBank(playerId);

            var permissions = new Dictionary<string, string>
            {
                {Permissions.Add, Modules.PlayerBankAccount},
                {Permissions.View, Modules.PlayerManager}
            };

            LoginNewUserWithMultiplePermissions(permissions);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _playerBankAccountCommands.Add(data));
        }

        [Test]
        public void Cannot_edit_player_bank_account_with_invalid_brand()
        {
            // Arrange
            var data = CreatePlayerBankAccountData();
            var playerId = CreateNewPlayer();
            data.PlayerId = playerId;
            data.Bank = CreateNewBank(playerId);
            var playerBankAccount = _playerBankAccountCommands.Add(data);
            data.Id = playerBankAccount.Id;

            LogWithNewUser(Modules.PlayerBankAccount, Permissions.Edit);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _playerBankAccountCommands.Edit(data));
        }

        [Test]
        public void Cannot_set_current_player_bank_account_with_invalid_brand()
        {
            // Arrange
            var data = CreatePlayerBankAccountData();
            var playerId = CreateNewPlayer();
            data.PlayerId = playerId;
            data.Bank = CreateNewBank(playerId);
            var playerBankAccount = _playerBankAccountCommands.Add(data);
            LogWithNewUser(Modules.PlayerBankAccount, Permissions.Edit);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _playerBankAccountCommands.SetCurrent(playerBankAccount.Id));
        }

        [Test]
        public void Cannot_verify_player_bank_account_with_invalid_brand()
        {
            // Arrange
            var data = CreatePlayerBankAccountData();
            var playerId = CreateNewPlayer();
            data.PlayerId = playerId;
            data.Bank = CreateNewBank(playerId);
            var playerBankAccount = _playerBankAccountCommands.Add(data);
            LogWithNewUser(Modules.PlayerBankAccount, Permissions.Verify);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _playerBankAccountCommands.Verify(playerBankAccount.Id, "Some remark"));
        }

        [Test]
        public void Cannot_reject_player_bank_account_with_invalid_brand()
        {
            // Arrange
            var data = CreatePlayerBankAccountData();
            var playerId = CreateNewPlayer();
            data.PlayerId = playerId;
            data.Bank = CreateNewBank(playerId);
            var playerBankAccount = _playerBankAccountCommands.Add(data);
            LogWithNewUser(Modules.PlayerBankAccount, Permissions.Reject);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _playerBankAccountCommands.Reject(playerBankAccount.Id, "Some remark"));
        }

        [Test]
        public void Cannot_get_player_bank_accounts_by_player_id_with_invalid_brand()
        {
            // Arrange
            var data = CreatePlayerBankAccountData();
            var playerId = CreateNewPlayer();
            data.PlayerId = playerId;
            data.Bank = CreateNewBank(playerId);
            _playerBankAccountCommands.Add(data);
            _playerBankAccountCommands.Add(data);
            LogWithNewUser(Modules.PlayerBankAccount, Permissions.View);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _playerBankAccountQueries.GetPlayerBankAccounts(playerId));
        }

        private Guid CreateNewBank(Guid playerId)
        {
            var playerQueries = Container.Resolve<PlayerQueries>();
            var paymentTestHelper = Container.Resolve<PaymentTestHelper>();

            var player = playerQueries.GetPlayer(playerId);
            var bank = paymentTestHelper.CreateBank(player.BrandId, TestDataGenerator.GetRandomString(3));

            return bank.Id;
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

        private EditPlayerBankAccountCommand CreatePlayerBankAccountData()
        {
            var data = new EditPlayerBankAccountCommand
            {
                PlayerId = new Guid("66AF937B-63D3-4CD4-89C4-7EECAD4FBB05"),
                Bank = new Guid("46C5D75E-4DF4-41B0-AA71-1D5E8CDAA897"),
                AccountName = TestDataGenerator.GetRandomString(),
                AccountNumber = TestDataGenerator.GetRandomString(7, "0123456789"),
                Province = TestDataGenerator.GetRandomString(),
                City = TestDataGenerator.GetRandomString(),
                Branch = TestDataGenerator.GetRandomString(),
                SwiftCode = TestDataGenerator.GetRandomString(),
                Address = TestDataGenerator.GetRandomString()
            };

            return data;
        }

        private void LoginNewUserWithMultiplePermissions(IDictionary<string, string> multiplePermissions)
        {
            var permissionService = Container.Resolve<PermissionService>();
            var securityTestHelper = Container.Resolve<SecurityTestHelper>();
            var brandTestHelper = Container.Resolve<BrandTestHelper>();

            var licensee = brandTestHelper.CreateLicensee();
            var brand = brandTestHelper.CreateBrand(licensee, isActive: true);
            var permissions = multiplePermissions.Select(x => permissionService.GetPermission(x.Key, x.Value)).ToList();

            var role = securityTestHelper.CreateRole(new[] { licensee.Id }, permissions);
            const string password = "123456";
            var user = securityTestHelper.CreateUser(licensee.Id, new[] { brand }, password: password, roleId: role.Id);
            securityTestHelper.SignInUser(user);
        }
    }
}