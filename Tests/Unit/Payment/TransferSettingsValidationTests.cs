using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Services.Security;
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
    class TransferSettingsValidationTests : AdminWebsiteUnitTestsBase
    {
        private TransferSettingsCommands _commands;
        private IPaymentRepository _paymentRepository;

        public override void BeforeEach()
        {
            base.BeforeEach();
            var bus = Container.Resolve<IEventBus>();
            _paymentRepository = Container.Resolve<IPaymentRepository>();
            var securityProvider = Container.Resolve<ISecurityProvider>();
            _commands = new TransferSettingsCommands(_paymentRepository, bus, securityProvider);
            Container.Resolve<SecurityTestHelper>().SignInUser(new Core.Security.Data.User() { Username = TestDataGenerator.GetRandomString() });
        }

        [Test]
        public void Should_throw_exception_if_negative_values_in_MinAmountPerTransaction()
        {
            // Arrange
            var saveSettingsCommand = new SaveTransferSettingsCommand();
            saveSettingsCommand.MinAmountPerTransaction = -1;
            saveSettingsCommand.TimezoneId = TestDataGenerator.GetRandomTimeZone().Id;

            // Act
            Action action = () => _commands.AddSettings(saveSettingsCommand);

            //Assert
            action.ShouldThrow<RegoException>().WithMessage("MinAmountPerTransactionError");
        }

        [Test]
        public void Should_throw_exception_if_negative_values_in_MaxAmountPerTransaction()
        {
            // Arrange
            var saveSettingsCommand = new SaveTransferSettingsCommand();
            saveSettingsCommand.MaxAmountPerTransaction = -1;
            saveSettingsCommand.TimezoneId = TestDataGenerator.GetRandomTimeZone().Id;

            // Act
            Action action = () => _commands.AddSettings(saveSettingsCommand);

            //Assert
            action.ShouldThrow<RegoException>().WithMessage("MaxAmountPerTransactionError");
        }

        [Test]
        public void Should_throw_exception_if_negative_values_in_MaxAmountPerDay()
        {
            // Arrange
            var saveSettingsCommand = new SaveTransferSettingsCommand();
            saveSettingsCommand.MaxAmountPerDay = -1;
            saveSettingsCommand.TimezoneId = TestDataGenerator.GetRandomTimeZone().Id;

            // Act
            Action action = () => _commands.AddSettings(saveSettingsCommand);

            //Assert
            action.ShouldThrow<RegoException>().WithMessage("MaxAmountPerDayError");
        }

        [Test]
        public void Should_throw_exception_if_negative_values_in_MaxTransactionPerDay()
        {
            // Arrange
            var saveSettingsCommand = new SaveTransferSettingsCommand();
            saveSettingsCommand.MaxTransactionPerDay = -1;
            saveSettingsCommand.TimezoneId = TestDataGenerator.GetRandomTimeZone().Id;

            // Act
            Action action = () => _commands.AddSettings(saveSettingsCommand);

            //Assert
            action.ShouldThrow<RegoException>().WithMessage("MaxTransactionPerDayError");
        }

        [Test]
        public void Should_throw_exception_if_negative_values_in_MaxTransactionPerWeek()
        {
            // Arrange
            var saveSettingsCommand = new SaveTransferSettingsCommand();
            saveSettingsCommand.MaxTransactionPerWeek = -1;
            saveSettingsCommand.TimezoneId = TestDataGenerator.GetRandomTimeZone().Id;

            // Act
            Action action = () => _commands.AddSettings(saveSettingsCommand);

            //Assert
            action.ShouldThrow<RegoException>().WithMessage("MaxTransactionPerWeekError");
        }

        [Test]
        public void Should_throw_exception_if_negative_values_in_MaxTransactionPerMonth()
        {
            // Arrange
            var saveSettingsCommand = new SaveTransferSettingsCommand();
            saveSettingsCommand.MaxTransactionPerMonth = -1;
            saveSettingsCommand.TimezoneId = TestDataGenerator.GetRandomTimeZone().Id;

            // Act
            Action action = () => _commands.AddSettings(saveSettingsCommand);

            //Assert
            action.ShouldThrow<RegoException>().WithMessage("MaxTransactionPerMonthError");
        }

        [Test]
        public void Should_throw_exception_if_MaxAmountPerTransaction_less_MinAmountPerTransaction_in_add_transferSettings()
        {
            // Arrange
            var saveSettingsCommand = new SaveTransferSettingsCommand();
            saveSettingsCommand.MinAmountPerTransaction = 2;
            saveSettingsCommand.MaxAmountPerTransaction = 1;
            saveSettingsCommand.TimezoneId = TestDataGenerator.GetRandomTimeZone().Id;

            // Act
            Action action = () => _commands.AddSettings(saveSettingsCommand);

            //Assert
            action.ShouldThrow<RegoException>().WithMessage("MaxminAmountPerTransactionError");
        }
        
        [Test]
        public void Should_throw_exception_if_MaxAmountPerTransaction_equal_MinAmountPerTransaction_in_add_transferSettings()
        {
            // Arrange
            var saveSettingsCommand = new SaveTransferSettingsCommand();
            saveSettingsCommand.MinAmountPerTransaction = 1;
            saveSettingsCommand.MaxAmountPerTransaction = 1;
            saveSettingsCommand.TimezoneId = TestDataGenerator.GetRandomTimeZone().Id;

            // Act
            Action action = () => _commands.AddSettings(saveSettingsCommand);

            //Assert
            action.ShouldThrow<RegoException>().WithMessage("MaxminAmountPerTransactionError");
        }

        [Test]
        public void Should_not_throw_exception_if_MaxAmountPerTransaction_zero_in_add_transferSettings()
        {
            // Arrange
            var saveSettingsCommand = new SaveTransferSettingsCommand();
            saveSettingsCommand.MinAmountPerTransaction = 1;
            saveSettingsCommand.MaxAmountPerTransaction = 0;
            saveSettingsCommand.TimezoneId = TestDataGenerator.GetRandomTimeZone().Id;

            // Act
            Action action = () => _commands.AddSettings(saveSettingsCommand);

            //Assert
            action.ShouldNotThrow();
        }

        [Test]
        public void Should_throw_exception_if_MinAmountPerTransaction_greater_than_MaxTransactionPerDay_in_add_transferSettings()
        {
            // Arrange
            var saveSettingsCommand = new SaveTransferSettingsCommand();
            saveSettingsCommand.MinAmountPerTransaction = 2;
            saveSettingsCommand.MaxAmountPerDay = 1;
            saveSettingsCommand.TimezoneId = TestDataGenerator.GetRandomTimeZone().Id;

            // Act
            Action action = () => _commands.AddSettings(saveSettingsCommand);

            //Assert
            action.ShouldThrow<RegoException>().WithMessage("MinAmountPerTransactionErrorAmountPerDay");
        }

        [Test]
        public void Should_throw_exception_if_MaxAmountPerTransaction_greater_than_MaxTransactionPerDay_in_add_transferSettings()
        {
            // Arrange
            var saveSettingsCommand = new SaveTransferSettingsCommand();
            saveSettingsCommand.MaxAmountPerTransaction = 2;
            saveSettingsCommand.MaxAmountPerDay = 1;
            saveSettingsCommand.TimezoneId = TestDataGenerator.GetRandomTimeZone().Id;

            // Act
            Action action = () => _commands.AddSettings(saveSettingsCommand);

            //Assert
            action.ShouldThrow<RegoException>().WithMessage("MaxAmountPerTransactionErrorAmountPerDay");
        }

        [Test]
        public void Should_not_throw_exception_with_zero_values_in_add_transferSettings()
        {
            // Arrange
            var saveSettingsCommand = new SaveTransferSettingsCommand();
            saveSettingsCommand.TimezoneId = TestDataGenerator.GetRandomTimeZone().Id; 

            // Act
            Action action = () => _commands.AddSettings(saveSettingsCommand);

            //Assert
            action.ShouldNotThrow();
        }

        [Test]
        public void Should_throw_exception_if_MaxTransactionsPerDay_greater_than_MaxTransactionsPerWeek_in_add_transferSettings()
        {
            // Arrange
            var saveSettingsCommand = new SaveTransferSettingsCommand();
            saveSettingsCommand.MaxTransactionPerDay = 2;
            saveSettingsCommand.MaxTransactionPerWeek = 1;
            saveSettingsCommand.TimezoneId = TestDataGenerator.GetRandomTimeZone().Id;

            // Act
            Action action = () => _commands.AddSettings(saveSettingsCommand);

            //Assert
            action.ShouldThrow<RegoException>().WithMessage("MaxTransactionPerWeekErrorPerDay");
        }

        [Test]
        public void Should_throw_exception_if_MaxTransactionsPerWeek_greater_than_MaxTransactionsPerMonth_in_add_transferSettings()
        {
            // Arrange
            var saveSettingsCommand = new SaveTransferSettingsCommand();
            saveSettingsCommand.MaxTransactionPerWeek = 2;
            saveSettingsCommand.MaxTransactionPerMonth = 1;
            saveSettingsCommand.TimezoneId = TestDataGenerator.GetRandomTimeZone().Id;

            // Act
            Action action = () => _commands.AddSettings(saveSettingsCommand);

            //Assert
            action.ShouldThrow<RegoException>().WithMessage("MaxTransactionPerMonthErrorPerWeek");
        }

        [Test]
        public void Should_throw_exception_if_MaxTransactionsPerDay_greater_than_MaxTransactionsPerMonth_in_add_transferSettings()
        {
            // Arrange
            var saveSettingsCommand = new SaveTransferSettingsCommand();
            saveSettingsCommand.MaxTransactionPerDay = 2;
            saveSettingsCommand.MaxTransactionPerMonth = 1;
            saveSettingsCommand.TimezoneId = TestDataGenerator.GetRandomTimeZone().Id;

            // Act
            Action action = () => _commands.AddSettings(saveSettingsCommand);

            //Assert
            action.ShouldThrow<RegoException>().WithMessage("MaxTransactionPerMonthErrorPerDay");
        }
    }
}
