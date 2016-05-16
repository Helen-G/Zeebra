using System.Linq;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Extensions;
using AFT.RegoV2.Tests.Common.Pages.BackEnd;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Selenium
{
    [Ignore("Until VladS. investigation for BeforeAll - AFTREGO-2869")]
    class PaymentSettingsTests : SeleniumBaseForAdminWebsite
    {
        private PaymentSettingsPage _paymentSettingsPage;
        private NewPaymentSettingsForm _newPaymentSettingsForm;
        private DashboardPage _dashboardPage;
        private VipLevelData _vipLevelData;
        private const string DefaultLicensee = "Flycow";
        private const string DefaultBrand = "138";
        private const string DefaultCurrency = "CAD";
        private const string DefaultPaymentMethod = "Offline - BoC1";


        public override void BeforeAll()
        {
            base.BeforeAll();
            //create vip level for a brand
            _dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();
            var vipLevelManagerPage = _dashboardPage.Menu.ClickVipLevelManagerMenuItem();
            var newVipLevelPage = vipLevelManagerPage.OpenNewVipLevelForm();
            _vipLevelData = TestDataGenerator.CreateValidVipLevelData(DefaultLicensee, DefaultBrand, false);
            var submittedVipLevelForm = newVipLevelPage.Submit(_vipLevelData);
            submittedVipLevelForm.CloseTab("View VIP Level");

            //generate payment settings data
            var paymentSettingsData = TestDataGenerator.CreateValidPaymentSettingsData(
                 DefaultLicensee,
                 DefaultBrand,
                 DefaultCurrency,
                 DefaultPaymentMethod,
                 paymentType: PaymentType.Withdraw.ToString(),
                 vipLevel: _vipLevelData.Name,
                 minAmountPerTrans: "10",
                 maxAmountPerTrans: "200",
                 maxAmountPerDay: "4000",
                 maxTransactionsPerDay: "100",
                 maxTransactionsPerWeek: "2000",
                 maxTransactionsPerMonth: "10000"
                );

            //create payment settings
            _paymentSettingsPage = submittedVipLevelForm.Menu.ClickPaymentSettingsMenuItem();
            _newPaymentSettingsForm = _paymentSettingsPage.OpenNewPaymentSettingsForm();
            _newPaymentSettingsForm.Submit(paymentSettingsData);
            _newPaymentSettingsForm.CloseTab("View Payment Settings");
        }

        public override void BeforeEach()
        {
            base.BeforeEach();
            _driver.Logout();
            _dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();
            _paymentSettingsPage = _dashboardPage.Menu.ClickPaymentSettingsMenuItem();
        }

        [Test]
        public void Can_edit_payment_settings()
        {
            //ganerate edited payment settings data
            var paymentSettingsDataEdited = TestDataGenerator.CreateValidPaymentSettingsData
               (
                 DefaultLicensee,
                 DefaultBrand,
                 DefaultCurrency,
                 DefaultPaymentMethod,
                 paymentType: PaymentType.Withdraw.ToString(),
                 vipLevel: _vipLevelData.Name,
                 minAmountPerTrans: "1",
                 maxAmountPerTrans: "20",
                 maxAmountPerDay: "400",
                 maxTransactionsPerDay: "4",
                 maxTransactionsPerWeek: "19",
                 maxTransactionsPerMonth: "199"
                );

            //edit payment settings 
            var editPaymentSettingsForm = _paymentSettingsPage.OpenEditPaymentSettingsForm(DefaultBrand, DefaultCurrency, _vipLevelData.Name);
            var submittedEditPaymentSettingsForm = editPaymentSettingsForm.Submit(paymentSettingsDataEdited);

            Assert.AreEqual("Payment settings have been successfully updated.", submittedEditPaymentSettingsForm.ConfirmationMessage);
            Assert.AreEqual(DefaultBrand, submittedEditPaymentSettingsForm.Brand);
            Assert.AreEqual(DefaultLicensee, submittedEditPaymentSettingsForm.Licensee);
            Assert.AreEqual(DefaultCurrency, submittedEditPaymentSettingsForm.Currency);
            Assert.AreEqual(paymentSettingsDataEdited.PaymentType, submittedEditPaymentSettingsForm.PaymentType);
            Assert.AreEqual(paymentSettingsDataEdited.VipLevel, submittedEditPaymentSettingsForm.VipLevel);
            Assert.AreEqual(paymentSettingsDataEdited.PaymentMethod, submittedEditPaymentSettingsForm.PaymentMethod);
            Assert.AreEqual(paymentSettingsDataEdited.MinAmountPerTransaction, submittedEditPaymentSettingsForm.MinAmountPerTransaction);
            Assert.AreEqual(paymentSettingsDataEdited.MaxAmountPerTransaction, submittedEditPaymentSettingsForm.MaxAmountPerTransaction);
            Assert.AreEqual(paymentSettingsDataEdited.MaxAmountPerDay, submittedEditPaymentSettingsForm.MaxAmountPerDay);
            Assert.AreEqual(paymentSettingsDataEdited.MaxTransactionsPerDay, submittedEditPaymentSettingsForm.MaxTransactionsPerDay);
            Assert.AreEqual(paymentSettingsDataEdited.MaxTransactionsPerWeek, submittedEditPaymentSettingsForm.MaxTransactionsPerWeek);
            Assert.AreEqual(paymentSettingsDataEdited.MaxTransactionsPerMonth, submittedEditPaymentSettingsForm.MaxTransactionsPerMonth);
        }

        [Test]
        public void Can_activate_deactive_payment_settings()
        {
            var activateDialog = _paymentSettingsPage.Activate(DefaultBrand, DefaultCurrency, _vipLevelData.Name, "remark");
            Assert.That(activateDialog.ConfirmationMessage, Is.StringContaining("has been successfully activated"));
            
            activateDialog.Close();
            _driver.Navigate().Refresh();
            _paymentSettingsPage = _dashboardPage.Menu.ClickPaymentSettingsMenuItem();
            var paymentSettingsStatus = _paymentSettingsPage.GetStatus(DefaultBrand, DefaultCurrency, _vipLevelData.Name);
            Assert.AreEqual("Active", paymentSettingsStatus);

            var deactivateDialog = _paymentSettingsPage.Deactivate(DefaultBrand, DefaultCurrency, _vipLevelData.Name, "remark");
             _driver.Navigate().Refresh();
             _paymentSettingsPage = _dashboardPage.Menu.ClickPaymentSettingsMenuItem();
            paymentSettingsStatus = _paymentSettingsPage.GetStatus(DefaultBrand, DefaultCurrency, _vipLevelData.Name);
            Assert.AreEqual("Inactive", paymentSettingsStatus);
        }

    }
}
