using System;
using System.Linq;
using AFT.RegoV2.ApplicationServices.Security;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Payment.ApplicationServices;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Domain.Payment.ApplicationServices;
using AFT.RegoV2.Domain.Payment.ApplicationServices.Data;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Helpers;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Payment
{
    internal class PaymentSettingsPermissionsTests : PermissionsTestsBase
    {
        private PaymentSettingsCommands _paymentSettingsCommands;
        private PaymentSettingsQueries _paymentSettingsQueries;

        public override void BeforeEach()
        {
            base.BeforeEach();

            _paymentSettingsCommands = Container.Resolve<PaymentSettingsCommands>();
            _paymentSettingsQueries = Container.Resolve<PaymentSettingsQueries>();
        }

        [Test]
        public void Cannot_execute_PaymentSettingsQueries_without_permissions()
        {
            // Arrange
            LogWithNewUser(Modules.PaymentSettings, Permissions.View);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _paymentSettingsQueries.SaveSetting(new SavePaymentSettingsCommand()));

        }

        [Test]
        public void Cannot_execute_PaymentSettingsCommands_without_permissions()
        {
            // Arrange
            LogWithNewUser(Modules.PaymentSettings, Permissions.View);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _paymentSettingsCommands.Enable(new Guid(), "Some remark"));
            Assert.Throws<InsufficientPermissionsException>(() => _paymentSettingsCommands.Disable(new Guid(), "Some remark"));

        }

        [Test]
        public void Cannot_enable_payment_settings_with_invalid_brand()
        {
            // Arrange
            var paymentSettingsId = CreatePaymentSettings();
            LogWithNewUser(Modules.PaymentSettings, Permissions.Activate);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _paymentSettingsCommands.Enable(paymentSettingsId, "Some remark"));
        }

        [Test]
        public void Cannot_disable_payment_settings_with_invalid_brand()
        {
            // Arrange
            var paymentSettingsId = CreatePaymentSettings();
            LogWithNewUser(Modules.PaymentSettings, Permissions.Deactivate);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _paymentSettingsCommands.Disable(paymentSettingsId, "Some remark"));
        }

        [Test]
        public void Cannot_save_payment_setting_with_invalid_brand()
        {
            // Arrange
            var brandTestHelper = Container.Resolve<BrandTestHelper>();
            var paymentTestHelper = Container.Resolve<PaymentTestHelper>();
            
            var licensee = brandTestHelper.CreateLicensee();
            var brand = brandTestHelper.CreateBrand(licensee, isActive: true);
            
            var brandBankAccount = paymentTestHelper.CreateBankAccount(brand.Id, brand.DefaultCurrency);

            var model = new SavePaymentSettingsCommand
            {
                Id = Guid.Empty,
                Licensee = brand.Licensee.Id,
                Brand = brand.Id,
                PaymentType = PaymentType.Deposit,
                PaymentMethod = brandBankAccount.Id,
                Currency = brand.BrandCurrencies.First().CurrencyCode,
                VipLevel = brand.DefaultVipLevelId.ToString(),
                MinAmountPerTransaction = 10,
                MaxAmountPerTransaction = 200,
                MaxTransactionPerDay = 10,
                MaxTransactionPerWeek = 20,
                MaxTransactionPerMonth = 30
            };

            LogWithNewUser(Modules.PaymentSettings, Permissions.Add);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _paymentSettingsQueries.SaveSetting(model));
        }

        private Guid CreatePaymentSettings()
        {
            var brandTestHelper = Container.Resolve<BrandTestHelper>();
            var paymentTestHelper = Container.Resolve<PaymentTestHelper>();

            var licensee = brandTestHelper.CreateLicensee();
            var brand = brandTestHelper.CreateBrand(licensee, isActive: true);
            var paymentSettings = paymentTestHelper.CreatePaymentSettings(brand, PaymentType.Deposit, false);

            return paymentSettings.Id;
        }
    }
}