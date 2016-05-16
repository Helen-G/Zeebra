using AFT.RegoV2.ApplicationServices.Security;
using AFT.RegoV2.Core.Payment.ApplicationServices;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Helpers;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Payment
{
    internal class PaymentLevelPermissionsTests : PermissionsTestsBase
    {
        private PaymentLevelCommands _paymentLevelCommands;

        public override void BeforeEach()
        {
            base.BeforeEach();

            _paymentLevelCommands = Container.Resolve<PaymentLevelCommands>();
        }

        [Test]
        public void Cannot_execute_PaymentLevelCommands_without_permissions()
        {
            // Arrange
            LogWithNewUser(Modules.PaymentLevelManager, Permissions.View);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _paymentLevelCommands.Save(new EditPaymentLevel()));
        }

        [Test]
        public void Cannot_save_payment_level_with_invalid_brand()
        {
            // Arrange
            var brandTestHelper = Container.Resolve<BrandTestHelper>();
            var paymentTestHelper = Container.Resolve<PaymentTestHelper>();

            var licensee = brandTestHelper.CreateLicensee();
            var brand = brandTestHelper.CreateBrand(licensee, isActive: true);
            var paymentLevel = paymentTestHelper.CreatePaymentLevel(brand.Id, brand.DefaultCurrency);

            var data = new EditPaymentLevel
            {
                Id = paymentLevel.Id,
                Brand = brand.Id,
                Name = TestDataGenerator.GetRandomString(6, "0123456789ABCDEF"),
                Code = TestDataGenerator.GetRandomString(6, "0123456789ABCDEF"),
                Currency = "CAD",
                EnableOfflineDeposit = true,
                IsDefault = false,
                BankAccounts = null
            };

            LogWithNewUser(Modules.PaymentLevelManager, Permissions.Edit);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _paymentLevelCommands.Save(data));
        }
    }
}