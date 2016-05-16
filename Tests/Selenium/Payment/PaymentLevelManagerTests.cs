using System;
using System.Linq;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Brand.Data;
using AFT.RegoV2.Domain.Payment.Data;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Extensions;
using AFT.RegoV2.Tests.Common.Helpers;
using AFT.RegoV2.Tests.Common.Pages.BackEnd;
using Microsoft.Practices.Unity;
using NUnit.Framework;
using Brand = AFT.RegoV2.Domain.Payment.Data.Brand;

namespace AFT.RegoV2.Tests.Selenium
{
    class PaymentLevelManagerTests : SeleniumBaseForAdminWebsite
    {
        private DashboardPage _dashboardPage;
        private PaymentLevelsPage _paymentLevelsPage;
        private BrandTestHelper _brandTestHelper;
        private PaymentTestHelper _paymentTestHelper;
        private BrandQueries _brandQueries;
        private Currency _currency;
        private Core.Brand.Data.Brand _brand;
        private BankAccount _bankAccount;
        private string _brandCurrency;

        public override void BeforeAll()
        {
            base.BeforeAll();
            
            //create a brand for a default licensee
            _brandTestHelper = _container.Resolve<BrandTestHelper>();
            var defaultLicenseeId = _brandTestHelper.GetDefaultLicensee();
            _currency = _brandTestHelper.CreateCurrency("ZAR", "South African Rand");
            _brand = _brandTestHelper.CreateBrand(defaultLicenseeId, null, null, _currency);

            // create a bank account for the brand
            _paymentTestHelper = _container.Resolve<PaymentTestHelper>();
            _bankAccount = _paymentTestHelper.CreateBankAccount(_brand.Id, _currency.Code);

            _brandQueries = _container.Resolve<BrandQueries>();
            _brandCurrency = _brandQueries.GetCurrenciesByBrand(_brand.Id).Select(c => c.Code).First();
        }

        public override void BeforeEach()
        {
            base.BeforeEach();
            _driver.Logout();
            _dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();
            _dashboardPage.BrandFilter.SelectAll();
            _paymentLevelsPage = _dashboardPage.Menu.ClickPaymentLevelsMenuItem();
        }

        [Test]
        public void Can_edit_payment_level()
        {
            //create a payment level for the brand
            var paymentLevel = _paymentTestHelper.CreatePaymentLevel(_brand.Id, _brandCurrency);
            var newPaymentLevelName = paymentLevel.Name + "1";
            var editForm = _paymentLevelsPage.OpenEditForm(paymentLevel.Name);
            var submittedEditForm = editForm.Submit(newPaymentLevelName);

            Assert.AreEqual("The payment level has been updated.", submittedEditForm.ConfirmationMessage);
            Assert.AreEqual(newPaymentLevelName, submittedEditForm.PaymentLevelName);
        }

        [Test]
        public void Cannot_create_two_default_payment_levels_for_brand_and_currency()
        {
            //create a payment level for the brand
            var paymentLevel = _paymentTestHelper.CreatePaymentLevel(_brand.Id, _brandCurrency);

            //try to create another default payment level
            var paymentLevelCode = paymentLevel.Code +"2";
            var paymentLevelName = paymentLevel.Code + "2";
            var newPaymentLevelForm =_paymentLevelsPage.OpenNewPaymentLevelForm();
            var submittedPaymentLevelForm2 = newPaymentLevelForm.Submit(_brand.Name, paymentLevelCode, paymentLevelName, _bankAccount.AccountId);
            
            Assert.AreEqual("Default payment level for the brand and currency combination already exists.", submittedPaymentLevelForm2.ErrorMessage);
        }
    }
}
