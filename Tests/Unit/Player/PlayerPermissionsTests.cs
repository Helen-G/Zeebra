using System;
using System.Globalization;
using System.Linq;
using AFT.RegoV2.ApplicationServices.Player;
using AFT.RegoV2.ApplicationServices.Security;
using AFT.RegoV2.Core.ApplicationServices.Player;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Domain.Player.Data;
using AFT.RegoV2.Core.Player.Data;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Helpers;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Player
{
    internal class PlayerPermissionsTests : PermissionsTestsBase
    {
        private PlayerQueries _playerQueries;
        private PlayerCommands _playerCommands;

        public override void BeforeEach()
        {
            base.BeforeEach();

            _playerQueries = Container.Resolve<PlayerQueries>();
            _playerCommands = Container.Resolve<PlayerCommands>();
        }

        [Test]
        public void Cannot_execute_PlayerQueries_without_permissions()
        {
            // Arrange
            LogWithNewUser(Modules.PlayerManager, Permissions.Edit);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _playerQueries.GetPlayer(new Guid()));
            Assert.Throws<InsufficientPermissionsException>(async () => await _playerQueries.GetPlayerAsync(new Guid()));
            Assert.Throws<InsufficientPermissionsException>(() => _playerQueries.GetPlayerForWithdraw(new Guid()));
            Assert.Throws<InsufficientPermissionsException>(() => _playerQueries.GetPlayers());
        }

        [Test]
        public void Cannot_execute_PlayerCommands_without_permissions()
        {
            // Arrange
            LogWithNewUser(Modules.PlayerManager, Permissions.View);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _playerCommands.Edit(new EditPlayerData()));
            Assert.Throws<InsufficientPermissionsException>(() => _playerCommands.ChangePlayerVipLevel(new Guid(), new Guid()));
            Assert.Throws<InsufficientPermissionsException>(() => _playerCommands.AssignVip(new Core.Player.Data.Player(), new VipLevel()));
            Assert.Throws<InsufficientPermissionsException>(() => _playerCommands.SetStatus(new Guid(), true));
            Assert.Throws<InsufficientPermissionsException>(() => _playerCommands.Register(new RegistrationData()));
            Assert.Throws<InsufficientPermissionsException>(() => _playerCommands.ChangeVipLevel(new Guid(), new Guid(), "Some remark"));
        }

        // TODO: Figure out why a lot of tests fails when Guid is changed to PlayerId in GetPlayer method
        //[Test]
        //public void Cannot_get_player_with_invalid_brand()
        //{
        //    // Arrange
        //    var playerId = CreateNewPlayer().Id;
        //    LogWithNewUser(Modules.PlayerManager, Permissions.View);

        //    // Act
        //    Assert.Throws<InsufficientPermissionsException>(() => _playerQueries.GetPlayer(playerId));
        //}

        [Test]
        public void Cannot_get_player_async_with_invalid_brand()
        {
            // Arrange
            var playerId = CreateNewPlayer().Id;
            LogWithNewUser(Modules.PlayerManager, Permissions.View);

            // Act
            Assert.Throws<InsufficientPermissionsException>(async () => await _playerQueries.GetPlayerAsync(playerId));
        }

        [Test]
        public void Cannot_get_player_for_withdraw_with_invalid_brand()
        {
            // Arrange
            var playerId = CreateNewPlayer().Id;
            LogWithNewUser(Modules.OfflineWithdrawalRequest, Permissions.Add);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _playerQueries.GetPlayerForWithdraw(playerId));
        }

        [Test]
        public void Cannot_edit_player_with_invalid_brand()
        {
            // Arrange
            var player = CreateNewPlayer();
            var data = new EditPlayerData
            {
                PlayerId = player.Id,
                FirstName = player.FirstName,
                LastName = player.LastName,
                MailingAddressCity = player.MailingAddressCity,
                MailingAddressPostalCode = player.MailingAddressPostalCode,
                CountryCode = player.CountryCode,
                Email = player.Email,
                DateOfBirth = "1980/01/01",
                MailingAddressLine1 = "#305-1250 Homer Street",
                PhoneNumber = "16046287716",
                PaymentLevelId = new Guid(),
                Title = player.Title,
                Gender = player.Gender
            };
            LogWithNewUser(Modules.PlayerManager, Permissions.Edit);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _playerCommands.Edit(data));
        }

        [Test]
        public void Cannot_change_player_vip_level_with_invalid_brand()
        {
            // Arrange
            var oldVipLevelId = CreateNewVipLevel();
            var newVipLevelId = CreateNewVipLevel();
            LogWithNewUser(Modules.PlayerManager, Permissions.AssignVipLevel);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _playerCommands.ChangePlayerVipLevel(oldVipLevelId, newVipLevelId));
        }

        [Test]
        public void Cannot_assign_vip_level_with_invalid_brand()
        {
            // Arrange
            var player = CreateNewPlayer();
            var vipLevel = new VipLevel() { BrandId = player.BrandId };
            LogWithNewUser(Modules.PlayerManager, Permissions.AssignVipLevel);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _playerCommands.AssignVip(player, vipLevel));
        }

        [Test]
        public void Cannot_set_status_with_invalid_brand()
        {
            // Arrange
            var playerId = CreateNewPlayer().Id;
            LogWithNewUser(Modules.PlayerManager, Permissions.Activate);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _playerCommands.SetStatus(playerId, true));
        }

        [Test]
        public void Cannot_register_with_invalid_brand()
        {
            // Arrange
            var brandTestHelper = Container.Resolve<BrandTestHelper>();

            var licensee = brandTestHelper.CreateLicensee();
            var brand = brandTestHelper.CreateBrand(licensee, isActive: true);
            var data = new RegistrationData
            {
                FirstName = TestDataGenerator.GetRandomString(),
                LastName = TestDataGenerator.GetRandomString(10),
                Email = TestDataGenerator.GetRandomEmail(),
                PhoneNumber = TestDataGenerator.GetRandomString(12, TestDataGenerator.NumericChars),
                MailingAddressLine1 = "Address Line 1",
                MailingAddressLine2 = "Address Line 2",
                MailingAddressLine3 = "Address Line 3",
                MailingAddressLine4 = "Address Line 4",
                MailingAddressCity = "Test City",
                MailingAddressPostalCode = TestDataGenerator.GetRandomString(5, TestDataGenerator.NumericChars),
                PhysicalAddressLine1 = "Physical Address Line 1",
                PhysicalAddressLine2 = "Physical Address Line 2",
                PhysicalAddressLine3 = "Physical Address Line 3",
                PhysicalAddressLine4 = "Physical Address Line 4",
                PhysicalAddressCity = "Physical Test City",
                PhysicalAddressPostalCode = TestDataGenerator.GetRandomString(5, TestDataGenerator.NumericChars),
                CountryCode = brand.BrandCountries.First().CountryCode,
                CurrencyCode = "CAD",
                CultureCode = brand.DefaultCulture,
                Username = TestDataGenerator.GetRandomString(12),
                Password = "123adasd",
                PasswordConfirm = "123adasd",
                DateOfBirth =
                    TestDataGenerator.GetDateOfBirthOver18().ToString("yyyy/MM/dd", CultureInfo.InvariantCulture),
                BrandId = brand.Id.ToString(),
                Gender = Gender.Male.ToString(),
                Title = Title.Mr.ToString(),
                ContactPreference = ContactMethod.Phone.ToString(),
                SecurityQuestionId = TestDataGenerator.GetRandomSecurityQuestion(),
                SecurityAnswer = "Security Answer " + TestDataGenerator.GetRandomString(),
                AccountStatus = AccountStatus.Active.ToString(),
                IdStatus = "Verified"
            };
            LogWithNewUser(Modules.PlayerManager, Permissions.Add);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _playerCommands.Register(data));
        }

        [Test]
        public void Cannot_change_vip_level_with_invalid_brand()
        {
            // Arrange
            var player = CreateNewPlayer();
            var vipLevel = new VipLevel() { BrandId = player.BrandId };
            LogWithNewUser(Modules.PlayerManager, Permissions.AssignVipLevel);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _playerCommands.ChangeVipLevel(player.Id, vipLevel.Id, "Some remark"));
        }

        private Core.Player.Data.Player CreateNewPlayer()
        {
            var brandTestHelper = Container.Resolve<BrandTestHelper>();
            var playerTestHelper = Container.Resolve<PlayerTestHelper>();

            var licensee = brandTestHelper.CreateLicensee();
            var brand = brandTestHelper.CreateBrand(licensee, isActive: true);
            var player = playerTestHelper.CreatePlayer(true, brand.Id);

            return player;
        }

        private Guid CreateNewVipLevel()
        {
            var brandTestHelper = Container.Resolve<BrandTestHelper>();

            var licensee = brandTestHelper.CreateLicensee();
            var brand = brandTestHelper.CreateBrand(licensee, isActive: true);
            var vipLevel = brandTestHelper.CreateVipLevel(brand.Id, 0, false);

            return vipLevel.Id;
        }
    }
}