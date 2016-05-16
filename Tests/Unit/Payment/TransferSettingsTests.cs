using System;
using System.Linq;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Services.Security;
using AFT.RegoV2.Domain.BoundedContexts.Payment.ApplicationServices.Exceptions;
using AFT.RegoV2.Domain.Payment;
using AFT.RegoV2.Domain.Payment.ApplicationServices;
using AFT.RegoV2.Domain.Payment.ApplicationServices.Data;
using AFT.RegoV2.Domain.Payment.Data;
using AFT.RegoV2.Domain.Security.Interfaces;
using AFT.RegoV2.Shared;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Helpers;
using FluentAssertions;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Payment
{
    class TransferSettingsTests : AdminWebsiteUnitTestsBase
    {
        private TransferSettingsCommands _commands;
        private IPaymentRepository _paymentRepository;
        private ISecurityProvider _securityProvider;

        public override void BeforeEach()
        {
            base.BeforeEach();
            var bus = Container.Resolve<IEventBus>();
            _paymentRepository = Container.Resolve<IPaymentRepository>();
            _securityProvider = Container.Resolve<ISecurityProvider>();
            _commands = new TransferSettingsCommands(_paymentRepository, bus, _securityProvider);
            Container.Resolve<SecurityTestHelper>().SignInUser(new Core.Security.Data.User() { Username = TestDataGenerator.GetRandomString() });
        }

        [Test]
        public void Enable_transfer_settings_test()
        {
            // Arrange
            var transferSettings = new TransferSettings();
            transferSettings.Id = new Guid("84c60b9f-16ad-49e0-bb9a-0e7670054dd5");
            _paymentRepository.TransferSettings.Add(transferSettings);

            // Act
            _commands.Enable(transferSettings.Id, TestDataGenerator.GetRandomTimeZone().Id, "remark");

            //Assert
            var settings = _paymentRepository.TransferSettings.Single(x => x.Id == transferSettings.Id);
            settings.Enabled.Should().BeTrue();
            settings.EnabledBy.ShouldBeEquivalentTo(_securityProvider.User.UserName);
            settings.EnabledDate.Should().BeCloseTo(DateTime.Now, 5000);
        }

        [Test]
        public void Disable_transfer_settings_test()
        {
            // Arrange
            var transferSettings = new TransferSettings();
            transferSettings.Id = new Guid("84c60b9f-16ad-49e0-bb9a-0e7670054dd5");
            transferSettings.Enabled = true;

            //required for event processing
            //will chnged in the future
            transferSettings.Brand = new Domain.Payment.Data.Brand
            {
                Name = TestDataGenerator.GetRandomString(),
                LicenseeName = TestDataGenerator.GetRandomString()
            };
            transferSettings.TransferType = TransferFundType.FundIn;

            _paymentRepository.TransferSettings.Add(transferSettings);

            // Act
            _commands.Disable(transferSettings.Id, TestDataGenerator.GetRandomTimeZone().Id, "remark");

            //Assert
            var settings = _paymentRepository.TransferSettings.Single(x => x.Id == transferSettings.Id);
            settings.Enabled.Should().BeFalse();
            settings.DisabledBy.ShouldBeEquivalentTo(_securityProvider.User.UserName);
            settings.DisabledDate.Should().BeCloseTo(DateTime.Now, 5000);
        }

        [Test]
        public void Update_transfer_settgins_limits_test()
        {
            // Arrange
            var transferSettings = new TransferSettings();
            transferSettings.Id = new Guid("84c60b9f-16ad-49e0-bb9a-0e7670054dd5");
            _paymentRepository.TransferSettings.Add(transferSettings);

            var saveTransferSettingsCommand = new SaveTransferSettingsCommand();
            saveTransferSettingsCommand.Id = transferSettings.Id;
            saveTransferSettingsCommand.MinAmountPerTransaction = 10;
            saveTransferSettingsCommand.MaxAmountPerTransaction = 20;
            saveTransferSettingsCommand.MaxAmountPerDay = 30;
            saveTransferSettingsCommand.MaxTransactionPerDay = 40;
            saveTransferSettingsCommand.MaxTransactionPerWeek = 50;
            saveTransferSettingsCommand.MaxTransactionPerMonth = 60;
            saveTransferSettingsCommand.TimezoneId = TestDataGenerator.GetRandomTimeZone().Id;

            // Act
            _commands.UpdateSettings(saveTransferSettingsCommand);

            //Assert
            var settings = _paymentRepository.TransferSettings.Single(x => x.Id == transferSettings.Id);
            settings.MinAmountPerTransaction.ShouldBeEquivalentTo(10);
            settings.MaxAmountPerTransaction.ShouldBeEquivalentTo(20);
            settings.MaxAmountPerDay.ShouldBeEquivalentTo(30);
            settings.MaxTransactionPerDay.ShouldBeEquivalentTo(40);
            settings.MaxTransactionPerWeek.ShouldBeEquivalentTo(50);
            settings.MaxTransactionPerMonth.ShouldBeEquivalentTo(60);
            settings.UpdatedBy.ShouldBeEquivalentTo(_securityProvider.User.UserName);
            settings.UpdatedDate.Should().BeCloseTo(DateTime.Now, 5000);
        }

        [Test]
        public void Should_throw_exception_if_try_to_add_same_settings()
        {
            // Arrange

            var timeZoneId = TestDataGenerator.GetRandomTimeZone().Id;

            var savePaymentSettingsCommand1 = new SaveTransferSettingsCommand();
            //required field
            savePaymentSettingsCommand1.TimezoneId = timeZoneId;
            _commands.AddSettings(savePaymentSettingsCommand1);
            
            var savePaymentSettingsCommand2 = new SaveTransferSettingsCommand();
            //required field
            savePaymentSettingsCommand1.TimezoneId = timeZoneId;
            
            // Act
            Action action = () => _commands.AddSettings(savePaymentSettingsCommand2);

            //Assert
            action.ShouldThrow<RegoException>().WithMessage("AlreadyExistsError");
        }
    }
}
