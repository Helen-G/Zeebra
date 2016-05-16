using System.Linq;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Extensions;
using AFT.RegoV2.Tests.Common.Pages.BackEnd;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Selenium.Payment
{
    class TransferSettingsTests : SeleniumBaseForAdminWebsite
    {
        private TransferSettingsPage _transferSettingsPage;
        private NewTransferSettingsForm _newTransferSettingsForm;
        private DashboardPage _dashboardPage;
        private VipLevelData _vipLevelData;
        private const string DefaultLicensee = "Flycow";
        private const string DefaultBrand = "138";
        private const string DefaultCurrency = "CAD";
        private const string DefaultProductWallet = "Product 138";

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

            //generate Transfer settings data
            var transferSettingsData = TestDataGenerator.CreateValidTransferSettingsData(
                 DefaultLicensee,
                 DefaultBrand,
                 DefaultCurrency,
                 DefaultProductWallet,
                 transferFundType: "Fund In", //TransferFundType.FundIn.ToString(),
                 vipLevel: _vipLevelData.Name,
                 minAmountPerTrans: "10",
                 maxAmountPerTrans: "200",
                 maxAmountPerDay: "4000",
                 maxTransactionsPerDay: "100",
                 maxTransactionsPerWeek: "2000",
                 maxTransactionsPerMonth: "10000"
                );

            //create Transfer settings
            _transferSettingsPage = submittedVipLevelForm.Menu.ClickTransferSettingsMenuItem();
            _newTransferSettingsForm = _transferSettingsPage.OpenNewTransferSettingsForm();
            _newTransferSettingsForm.Submit(transferSettingsData);
            _newTransferSettingsForm.CloseTab("View Transfer Settings");
        }

        public override void BeforeEach()
        {
            base.BeforeEach();
            _driver.Logout();
            _dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();
            _transferSettingsPage = _dashboardPage.Menu.ClickTransferSettingsMenuItem();
        }

        [Test]
        public void Can_edit_transfer_settings()
        {
            //generate edited Transfer settings data
            var transferSettingsDataEdited = TestDataGenerator.CreateValidTransferSettingsData(
                 DefaultLicensee,
                 DefaultBrand,
                 DefaultCurrency,
                 DefaultProductWallet,
                 transferFundType: "Fund In", //TransferFundType.FundIn.ToString(),
                 vipLevel: _vipLevelData.Name,
                 minAmountPerTrans: "1",
                 maxAmountPerTrans: "20",
                 maxAmountPerDay: "400",
                 maxTransactionsPerDay: "4",
                 maxTransactionsPerWeek: "19",
                 maxTransactionsPerMonth: "199"
                );

            //edit Transfer settings 
            var editTransferSettingsForm = _transferSettingsPage.OpenEditTransferSettingsForm(DefaultBrand, DefaultCurrency, _vipLevelData.Name);
            var submittedEditTransferSettingsForm = editTransferSettingsForm.Submit(transferSettingsDataEdited);

            Assert.AreEqual("Transfer settings have been successfully updated", submittedEditTransferSettingsForm.ConfirmationMessage);
            Assert.AreEqual(DefaultLicensee, submittedEditTransferSettingsForm.Licensee);
            Assert.AreEqual(DefaultBrand, submittedEditTransferSettingsForm.Brand);
            Assert.AreEqual(DefaultCurrency, submittedEditTransferSettingsForm.Currency);
            Assert.AreEqual(transferSettingsDataEdited.TransferFundType, submittedEditTransferSettingsForm.TransferType);
            Assert.AreEqual(transferSettingsDataEdited.VipLevel, submittedEditTransferSettingsForm.VipLevel);
            Assert.AreEqual(transferSettingsDataEdited.ProductWallet, submittedEditTransferSettingsForm.ProductWallet);
            Assert.AreEqual(transferSettingsDataEdited.MinAmountPerTransaction, submittedEditTransferSettingsForm.MinAmountPerTransaction);
            Assert.AreEqual(transferSettingsDataEdited.MaxAmountPerTransaction, submittedEditTransferSettingsForm.MaxAmountPerTransaction);
            Assert.AreEqual(transferSettingsDataEdited.MaxAmountPerDay, submittedEditTransferSettingsForm.MaxAmountPerDay);
            Assert.AreEqual(transferSettingsDataEdited.MaxTransactionsPerDay, submittedEditTransferSettingsForm.MaxTransactionsPerDay);
            Assert.AreEqual(transferSettingsDataEdited.MaxTransactionsPerWeek, submittedEditTransferSettingsForm.MaxTransactionsPerWeek);
            Assert.AreEqual(transferSettingsDataEdited.MaxTransactionsPerMonth, submittedEditTransferSettingsForm.MaxTransactionsPerMonth);

        }


        [Test]
        public void Can_activate_deactive_transfer_settings()
        {
            var activateDialog = _transferSettingsPage.Activate(DefaultBrand, DefaultCurrency, _vipLevelData.Name, "remark");
            Assert.That(activateDialog.ConfirmationMessage, Is.StringContaining("has been successfully activated"));

            activateDialog.Close();
            _driver.Navigate().Refresh();
            _transferSettingsPage = _dashboardPage.Menu.ClickTransferSettingsMenuItem();
            var transferSettingsStatus = _transferSettingsPage.GetStatus(DefaultBrand, DefaultCurrency, _vipLevelData.Name);
            Assert.AreEqual("Active", transferSettingsStatus);

            var deactivateDialog = _transferSettingsPage.Deactivate(DefaultBrand, DefaultCurrency, _vipLevelData.Name, "remark");
            Assert.That(deactivateDialog.ConfirmationMessage, Is.StringContaining("has been successfully deactivated"));
            deactivateDialog.Close();

            _driver.Navigate().Refresh();
            _transferSettingsPage = _dashboardPage.Menu.ClickTransferSettingsMenuItem();
            transferSettingsStatus = _transferSettingsPage.GetStatus(DefaultBrand, DefaultCurrency, _vipLevelData.Name);
            Assert.AreEqual("Inactive", transferSettingsStatus);
        }

    }
}
