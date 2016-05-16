using System;
using System.Linq;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Services.Security;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Domain.Payment;
using AFT.RegoV2.Domain.Payment.ApplicationServices;
using AFT.RegoV2.Domain.Payment.ApplicationServices.Data;
using AFT.RegoV2.Domain.Payment.Data;
using AFT.RegoV2.Domain.Security.Interfaces;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Helpers;
using FluentAssertions;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Payment
{
    class PaymentSettingsTests : AdminWebsiteUnitTestsBase
    {
        private PaymentSettingsCommands _commands;
        private IPaymentRepository _paymentRepository;
        private ISecurityProvider _securityProvider;

        public override void BeforeEach()
        {
            base.BeforeEach();
            _paymentRepository = Container.Resolve<IPaymentRepository>();
            _securityProvider = Container.Resolve<ISecurityProvider>();
            var eventBus = Container.Resolve<IEventBus>();
            _commands = new PaymentSettingsCommands(_paymentRepository, _securityProvider, eventBus);
            Container.Resolve<SecurityTestHelper>().SignInUser(new Core.Security.Data.User() { Username = TestDataGenerator.GetRandomString() });
        }

        [Test]
        public void Can_add_payment_settings()
        {
            // Arrange
            var securityHelper = Container.Resolve<SecurityTestHelper>();
            securityHelper.SignInUser();
            var brand = Container.Resolve<BrandTestHelper>().CreateBrand(isActive: true);

            // Act
            var settings = Container.Resolve<PaymentTestHelper>().CreatePaymentSettings(brand, PaymentType.Deposit);

            //Assert
            settings.Should().NotBeNull();
            settings.BrandId.Should().NotBeEmpty();
            settings.PaymentType.ShouldBeEquivalentTo(PaymentType.Deposit);
            settings.VipLevel.ShouldBeEquivalentTo(brand.DefaultVipLevelId.ToString());
            settings.CurrencyCode.ShouldBeEquivalentTo(brand.BrandCurrencies.First().CurrencyCode);
            settings.PaymentGateway.Should().NotBeNull();
            settings.MinAmountPerTransaction.ShouldBeEquivalentTo(10);
            settings.MaxAmountPerTransaction.ShouldBeEquivalentTo(200);
            settings.MaxAmountPerDay.ShouldBeEquivalentTo(0);
            settings.MaxTransactionPerDay.ShouldBeEquivalentTo(10);
            settings.MaxTransactionPerWeek.ShouldBeEquivalentTo(20);
            settings.MaxTransactionPerMonth.ShouldBeEquivalentTo(30);
            settings.Enabled.Should().BeTrue();
            settings.CreatedBy.ShouldBeEquivalentTo(securityHelper.CurrentUser.UserName);
            settings.CreatedDate.Should().BeCloseTo(DateTime.Now, 60000);
            settings.UpdatedBy.Should().BeNull();
            settings.UpdatedDate.Should().NotHaveValue();
            settings.EnabledBy.Should().BeNull();
            settings.EnabledDate.Should().BeCloseTo(DateTime.Now, 60000);
            settings.DisabledBy.Should().BeNull();
            settings.DisabledDate.Should().NotHaveValue();
        }

        [Test]
        public void Should_throw_exception_if_bank_account_not_found()
        {
            // Arrange
            var savePaymentSettingsCommand = new SavePaymentSettingsCommand();
            //Add to pass Validator
            savePaymentSettingsCommand.MaxAmountPerTransaction = 1;

            // Act
            Action action = () => _commands.AddSettings(savePaymentSettingsCommand);

            //Assert
            action.ShouldThrow<Exception>().WithMessage("BankAccountNotFound");
        }

        [Test]
        public void Enable_payment_settings_test()
        {
            // Arrange
            var paymentSettings = new PaymentSettings();
            paymentSettings.Id = new Guid("50797305-5320-4086-BA1C-9D062DDF53D9");
            _paymentRepository.PaymentSettings.Add(paymentSettings);

            // Act
            _commands.Enable(paymentSettings.Id, "remark");

            //Assert
            var settings = _paymentRepository.PaymentSettings.Single(x => x.Id == paymentSettings.Id);
            settings.Enabled.Should().BeTrue();
            settings.EnabledBy.ShouldBeEquivalentTo(_securityProvider.User.UserName);
            settings.EnabledDate.Should().BeCloseTo(DateTime.Now, 5000);
        }

        [Test]
        public void Disable_payment_settings_test()
        {
            // Arrange
            var paymentSettings = new PaymentSettings();
            paymentSettings.Id = new Guid("50797305-5320-4086-BA1C-9D062DDF53D9");
            paymentSettings.Enabled = true;
            _paymentRepository.PaymentSettings.Add(paymentSettings);

            // Act
            _commands.Disable(paymentSettings.Id, "remark");

            //Assert
            var settings = _paymentRepository.PaymentSettings.Single(x => x.Id == paymentSettings.Id);
            settings.Enabled.Should().BeFalse();
            settings.DisabledBy.ShouldBeEquivalentTo(_securityProvider.User.UserName);
            settings.DisabledDate.Should().BeCloseTo(DateTime.Now, 5000);
        }

        [Test]
        public void Update_payment_settings_limits_test()
        {
            // Arrange
            var paymentSettings = new PaymentSettings();
            paymentSettings.Id = new Guid("50797305-5320-4086-BA1C-9D062DDF53D9");
            _paymentRepository.PaymentSettings.Add(paymentSettings);

            var savePaymentSettingsCommand = new SavePaymentSettingsCommand();
            savePaymentSettingsCommand.Id = paymentSettings.Id;
            savePaymentSettingsCommand.MinAmountPerTransaction = 10;
            savePaymentSettingsCommand.MaxAmountPerTransaction = 20;
            savePaymentSettingsCommand.MaxAmountPerDay = 30;
            savePaymentSettingsCommand.MaxTransactionPerDay = 40;
            savePaymentSettingsCommand.MaxTransactionPerWeek = 50;
            savePaymentSettingsCommand.MaxTransactionPerMonth = 60;

            // Act
            _commands.UpdateSettings(savePaymentSettingsCommand);

            //Assert
            var settings = _paymentRepository.PaymentSettings.Single(x => x.Id == paymentSettings.Id);
            settings.MinAmountPerTransaction.ShouldBeEquivalentTo(10);
            settings.MaxAmountPerTransaction.ShouldBeEquivalentTo(20);
            settings.MaxAmountPerDay.ShouldBeEquivalentTo(30);
            settings.MaxTransactionPerDay.ShouldBeEquivalentTo(40);
            settings.MaxTransactionPerWeek.ShouldBeEquivalentTo(50);
            settings.MaxTransactionPerMonth.ShouldBeEquivalentTo(60);
            settings.UpdatedBy.ShouldBeEquivalentTo(_securityProvider.User.UserName);
            settings.UpdatedDate.Should().BeCloseTo(DateTime.Now, 5000);
        }
    }
}
