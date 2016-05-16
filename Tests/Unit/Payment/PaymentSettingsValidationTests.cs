using System;
using AFT.RegoV2.Core.Services.Security;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Domain.Payment;
using AFT.RegoV2.Domain.Payment.ApplicationServices;
using AFT.RegoV2.Domain.Payment.ApplicationServices.Data;
using AFT.RegoV2.Domain.Payment.Data;
using AFT.RegoV2.Domain.Security.Interfaces;
using AFT.RegoV2.Shared;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Helpers;
using FluentAssertions;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Payment
{
    class PaymentSettingsValidationTests : AdminWebsiteUnitTestsBase
    {
        private PaymentSettingsCommands _commands;
        private IPaymentRepository _paymentRepository;

        public override void BeforeEach()
        {
            base.BeforeEach();
            _paymentRepository = Container.Resolve<IPaymentRepository>();
            var securityProvider = Container.Resolve<ISecurityProvider>();
            var eventBus = Container.Resolve<IEventBus>();
            _commands = new PaymentSettingsCommands(_paymentRepository, securityProvider, eventBus);
        }

        [Test]
        public void Should_throw_exception_if_negative_values_in_MinAmountPerTransaction()
        {
            // Arrange
            var saveSettingsCommand = new SavePaymentSettingsCommand();
            saveSettingsCommand.MinAmountPerTransaction = -1;
            
            // Act
            Action action = () => _commands.AddSettings(saveSettingsCommand);

            //Assert
            action.ShouldThrow<RegoException>().WithMessage("MinAmountPerTransactionError");
        }

        [Test]
        public void Should_throw_exception_if_negative_values_in_MaxAmountPerTransaction()
        {
            // Arrange
            var saveSettingsCommand = new SavePaymentSettingsCommand();
            saveSettingsCommand.MaxAmountPerTransaction = -1;

            // Act
            Action action = () => _commands.AddSettings(saveSettingsCommand);

            //Assert
            action.ShouldThrow<RegoException>().WithMessage("MaxAmountPerTransactionError");
        }

        [Test]
        public void Should_throw_exception_if_negative_values_in_MaxAmountPerDay()
        {
            // Arrange
            var saveSettingsCommand = new SavePaymentSettingsCommand();
            saveSettingsCommand.MaxAmountPerDay = -1;

            // Act
            Action action = () => _commands.AddSettings(saveSettingsCommand);

            //Assert
            action.ShouldThrow<RegoException>().WithMessage("MaxAmountPerDayError");
        }

        [Test]
        public void Should_throw_exception_if_negative_values_in_MaxTransactionPerDay()
        {
            // Arrange
            var saveSettingsCommand = new SavePaymentSettingsCommand();
            saveSettingsCommand.MaxTransactionPerDay = -1;

            // Act
            Action action = () => _commands.AddSettings(saveSettingsCommand);

            //Assert
            action.ShouldThrow<RegoException>().WithMessage("MaxTransactionPerDayError");
        }

        [Test]
        public void Should_throw_exception_if_negative_values_in_MaxTransactionPerWeek()
        {
            // Arrange
            var saveSettingsCommand = new SavePaymentSettingsCommand();
            saveSettingsCommand.MaxTransactionPerWeek = -1;

            // Act
            Action action = () => _commands.AddSettings(saveSettingsCommand);

            //Assert
            action.ShouldThrow<RegoException>().WithMessage("MaxTransactionPerWeekError");
        }

        [Test]
        public void Should_throw_exception_if_negative_values_in_MaxTransactionPerMonth()
        {
            // Arrange
            var saveSettingsCommand = new SavePaymentSettingsCommand();
            saveSettingsCommand.MaxTransactionPerMonth = -1;

            // Act
            Action action = () => _commands.AddSettings(saveSettingsCommand);

            //Assert
            action.ShouldThrow<RegoException>().WithMessage("MaxTransactionPerMonthError");
        }
        
        [Test]
        public void Should_throw_exception_if_MaxAmountPerTransaction_less_MinAmountPerTransaction_in_add_paymentSettings()
        {
            // Arrange
            var saveSettingsCommand = new SavePaymentSettingsCommand();
            saveSettingsCommand.MinAmountPerTransaction = 2;
            saveSettingsCommand.MaxAmountPerTransaction = 1;

            // Act
            Action action = () => _commands.AddSettings(saveSettingsCommand);

            //Assert
            action.ShouldThrow<RegoException>().WithMessage("MaxminAmountPerTransactionError");
        }

        [Test]
        public void Should_throw_exception_if_MaxAmountPerTransaction_less_MinAmountPerTransaction_in_update_paymentSettings()
        {
            // Arrange
            var saveSettingsCommand = new SavePaymentSettingsCommand();
            saveSettingsCommand.MinAmountPerTransaction = 2;
            saveSettingsCommand.MaxAmountPerTransaction = 1;

            // Act
            Action action = () => _commands.UpdateSettings(saveSettingsCommand);

            //Assert
            action.ShouldThrow<RegoException>().WithMessage("MaxminAmountPerTransactionError");
        }

        [Test]
        public void Should_throw_exception_if_MaxAmountPerTransaction_equal_MinAmountPerTransaction_in_add_paymentSettings()
        {
            // Arrange
            var saveSettingsCommand = new SavePaymentSettingsCommand();
            saveSettingsCommand.MinAmountPerTransaction = 1;
            saveSettingsCommand.MaxAmountPerTransaction = 1;

            // Act
            Action action = () => _commands.AddSettings(saveSettingsCommand);

            //Assert
            action.ShouldThrow<RegoException>().WithMessage("MaxminAmountPerTransactionError");
        }

        [Test]
        public void Should_not_throw_exception_if_MaxAmountPerTransaction_zero_in_add_paymentSettings()
        {
            // Arrange
            var saveSettingsCommand = new SavePaymentSettingsCommand();
            saveSettingsCommand.MinAmountPerTransaction = 1;
            saveSettingsCommand.MaxAmountPerTransaction = 0;

            // Act
            Action action = () => _commands.AddSettings(saveSettingsCommand);

            //Assert
            action.ShouldThrow<RegoException>().Where(x =>
                    !x.Message.Contains("MaxminAmountPerTransactionError"));
        }

        [Test]
        public void Should_not_throw_exception_if_MaxAmountPerTransaction_zero_in_update_paymentSettings()
        {
            // Arrange
            Container.Resolve<SecurityTestHelper>().SignInUser(new Core.Security.Data.User() { Username = "TestUser" });
            var settings = new PaymentSettings();
            settings.Id = new Guid("84c60b9f-16ad-49e0-bb9a-0e7670054dd5");
            settings.MinAmountPerTransaction = 1;
            settings.MaxAmountPerTransaction = 1;
            _paymentRepository.PaymentSettings.Add(settings);

            var saveSettingsCommand = new SavePaymentSettingsCommand();
            saveSettingsCommand.Id = settings.Id;
            saveSettingsCommand.MinAmountPerTransaction = 1;
            saveSettingsCommand.MaxAmountPerTransaction = 0;

            // Act
            Action action = () => _commands.UpdateSettings(saveSettingsCommand);

            //Assert
            action.ShouldNotThrow();
        }

        [Test]
        public void Should_throw_exception_if_MinAmountPerTransaction_greater_than_MaxTransactionPerDay_in_add_paymentSettings()
        {
            // Arrange
            var saveSettingsCommand = new SavePaymentSettingsCommand();
            saveSettingsCommand.MinAmountPerTransaction = 2;
            saveSettingsCommand.MaxAmountPerDay = 1;

            // Act
            Action action = () => _commands.AddSettings(saveSettingsCommand);

            //Assert
            action.ShouldThrow<RegoException>().WithMessage("MinAmountPerTransactionErrorAmountPerDay");
        }

        [Test]
        public void Should_throw_exception_if_MaxAmountPerTransaction_greater_than_MaxTransactionPerDay_in_add_paymentSettings()
        {
            // Arrange
            var saveSettingsCommand = new SavePaymentSettingsCommand();
            saveSettingsCommand.MaxAmountPerTransaction = 2;
            saveSettingsCommand.MaxAmountPerDay = 1;

            // Act
            Action action = () => _commands.AddSettings(saveSettingsCommand);

            //Assert
            action.ShouldThrow<RegoException>().WithMessage("MaxAmountPerTransactionErrorAmountPerDay");
        }

        [Test]
        public void Should_throw_exception_if_MaxTransactionsPerDay_greater_than_MaxTransactionsPerWeek_in_add_paymentSettings()
        {
            // Arrange
            var saveSettingsCommand = new SavePaymentSettingsCommand();
            saveSettingsCommand.MaxTransactionPerDay = 2;
            saveSettingsCommand.MaxTransactionPerWeek = 1;

            // Act
            Action action = () => _commands.AddSettings(saveSettingsCommand);

            //Assert
            action.ShouldThrow<RegoException>().WithMessage("MaxTransactionPerWeekErrorPerDay");
        }

        [Test]
        public void Should_throw_exception_if_MaxTransactionsPerWeek_greater_than_MaxTransactionsPerMonth_in_add_paymentSettings()
        {
            // Arrange
            var saveSettingsCommand = new SavePaymentSettingsCommand();
            saveSettingsCommand.MaxTransactionPerWeek = 2;
            saveSettingsCommand.MaxTransactionPerMonth = 1;

            // Act
            Action action = () => _commands.AddSettings(saveSettingsCommand);

            //Assert
            action.ShouldThrow<RegoException>().WithMessage("MaxTransactionPerMonthErrorPerWeek");
        }

        [Test]
        public void Should_throw_exception_if_MaxTransactionsPerDay_greater_than_MaxTransactionsPerMonth_in_add_paymentSettings()
        {
            // Arrange
            var saveSettingsCommand = new SavePaymentSettingsCommand();
            saveSettingsCommand.MaxTransactionPerDay = 2;
            saveSettingsCommand.MaxTransactionPerMonth = 1;

            // Act
            Action action = () => _commands.AddSettings(saveSettingsCommand);

            //Assert
            action.ShouldThrow<RegoException>().WithMessage("MaxTransactionPerMonthErrorPerDay");
        }
    }
}
