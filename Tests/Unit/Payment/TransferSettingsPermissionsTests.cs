using System;
using AFT.RegoV2.ApplicationServices.Security;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Domain.Payment.ApplicationServices;
using AFT.RegoV2.Domain.Payment.ApplicationServices.Data;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Helpers;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Payment
{
    internal class TransferSettingsPermissionsTests : PermissionsTestsBase
    {
        private TransferSettingsCommands _transferSettingsCommands;

        public override void BeforeEach()
        {
            base.BeforeEach();

            _transferSettingsCommands = Container.Resolve<TransferSettingsCommands>();
        }

        [Test]
        public void Cannot_execute_Commands_without_permissions()
        {
            // Arrange
            LogWithNewUser(Modules.TransferSettings, Permissions.View);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _transferSettingsCommands.AddSettings(new SaveTransferSettingsCommand()));
            Assert.Throws<InsufficientPermissionsException>(() => _transferSettingsCommands.UpdateSettings(new SaveTransferSettingsCommand()));
            Assert.Throws<InsufficientPermissionsException>(() => _transferSettingsCommands.Enable(new Guid(), "", "12345"));
            Assert.Throws<InsufficientPermissionsException>(() => _transferSettingsCommands.Disable(new Guid(), "", "12345"));
        }

        [Test]
        public void Cannot_add_transfer_settings_with_invalid_brand()
        {
            // Arrange
            var saveTransferSettingsCommand = CreateNewTransferSettingsData();
            LogWithNewUser(Modules.TransferSettings, Permissions.Add);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _transferSettingsCommands.AddSettings(saveTransferSettingsCommand));
        }

        [Test]
        public void Cannot_update_transfer_settings_with_invalid_brand()
        {
            // Arrange
            var saveTransferSettingsCommand = CreateNewTransferSettingsData();
            var transferSettingId = _transferSettingsCommands.AddSettings(saveTransferSettingsCommand);
            saveTransferSettingsCommand.Id = transferSettingId;
            LogWithNewUser(Modules.TransferSettings, Permissions.Edit);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _transferSettingsCommands.UpdateSettings(saveTransferSettingsCommand));
        }

        [Test]
        public void Cannot_enable_transfer_settings_with_invalid_brand()
        {
            // Arrange
            var saveTransferSettingsCommand = CreateNewTransferSettingsData();
            var transferSettingId = _transferSettingsCommands.AddSettings(saveTransferSettingsCommand);
            LogWithNewUser(Modules.TransferSettings, Permissions.Activate);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() =>
                _transferSettingsCommands.Enable(transferSettingId, TestDataGenerator.GetRandomTimeZone().Id, "remark"));
        }

        [Test]
        public void Cannot_disable_transfer_settings_with_invalid_brand()
        {
            // Arrange
            var saveTransferSettingsCommand = CreateNewTransferSettingsData();
            var transferSettingId = _transferSettingsCommands.AddSettings(saveTransferSettingsCommand);
            LogWithNewUser(Modules.TransferSettings, Permissions.Deactivate);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() =>
                _transferSettingsCommands.Disable(transferSettingId, TestDataGenerator.GetRandomTimeZone().Id, "remark"));
        }

        private SaveTransferSettingsCommand CreateNewTransferSettingsData()
        {
            var brandTestHelper = Container.Resolve<BrandTestHelper>();

            var licensee = brandTestHelper.CreateLicensee();
            var brand = brandTestHelper.CreateBrand(licensee, isActive: true);

            var saveSettingsCommand = new SaveTransferSettingsCommand
            {
                Licensee = licensee.Id,
                Brand = brand.Id,
                MinAmountPerTransaction = 1,
                MaxAmountPerTransaction = 100,
                MaxAmountPerDay = 500,
                MaxTransactionPerDay = 1,
                MaxTransactionPerWeek = 5,
                MaxTransactionPerMonth = 10,
                TimezoneId = TestDataGenerator.GetRandomTimeZone().Id
            };

            return saveSettingsCommand;
        }
    }
}