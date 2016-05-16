using System.Linq;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Extensions;
using AFT.RegoV2.Tests.Common.Helpers;
using AFT.RegoV2.Tests.Common.Pages.BackEnd;
using AFT.RegoV2.Tests.Common.Pages.BackEnd.Fraud;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Selenium
{
    class BrandManagerTests : SeleniumBaseForAdminWebsite
    {
        private BrandManagerPage _brandManagerPage;
        private DashboardPage _dashboardPage;
        
        public override void BeforeEach()
        {
            base.BeforeEach();
            _driver.Logout();
            _dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();
            _brandManagerPage = _dashboardPage.Menu.ClickBrandManagerItem();
        }

        [Test]
        public void Can_add_and_activate_brand()
        {            
            // create a brand
            const string licensee = "Flycow";
            var randomString = TestDataGenerator.GetRandomString(4);
            var brandName = "brand-" + randomString;
            var brandCode = randomString;
            var playerPrefix = TestDataGenerator.GetRandomAlphabeticString(3);
            const string brandType = "Credit";
            const string country = "Canada";
            const string currency = "CAD";
            const string languageCode = "en-GB";

            var newBrandForm = _brandManagerPage.OpenNewBrandForm();
            var submittedBrandForm = newBrandForm.Submit(brandName, brandCode, playerPrefix, brandType);
            Assert.AreEqual("The brand has been successfully created.", submittedBrandForm.ConfirmationMessage);
            Assert.AreEqual(licensee, submittedBrandForm.LicenseeValue);
            Assert.AreEqual(brandType, submittedBrandForm.BrandTypeValue);
            Assert.AreEqual(brandName, submittedBrandForm.BrandNameValue);
            Assert.AreEqual(brandCode, submittedBrandForm.BrandCodeValue);
            Assert.AreEqual(playerPrefix, submittedBrandForm.PlayerPrefix);
            submittedBrandForm.CloseTab("View Brand");

            _driver.Logout();
            _dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();
            _dashboardPage.BrandFilter.SelectAll();
            _brandManagerPage = _dashboardPage.Menu.ClickBrandManagerItem();

            //create wallet for brand
            var walletTemplateListPage = _dashboardPage.Menu.ClickWalletManagerMenuItem();
            var addWalletTemplateForm = walletTemplateListPage.OpenNewWalletForm();
            var submittedAddWalletTemplateForm = addWalletTemplateForm.Submit(licensee, brandName);
            Assert.AreEqual("The wallet has been successfully created", submittedAddWalletTemplateForm.ConfirmationMessage);
            submittedAddWalletTemplateForm.CloseTab("View Wallet");


            // assign a country to the brand
            var supportedCountriesPage = _brandManagerPage.Menu.ClickSupportedCountriesMenuItem();
            var assignCountryForm = supportedCountriesPage.OpenAssignCountriesForm();
            var submittedAssignCurrencyForm = assignCountryForm.Submit(licensee, brandName, country);
            Assert.AreEqual("The countries have been successfully assigned", submittedAssignCurrencyForm.ConfirmationMessage);
            submittedAssignCurrencyForm.CloseTab("View Assigned Countries");

            // assign a currency to the brand
            var supportedCurrenciesPage = submittedAssignCurrencyForm.Menu.ClickSupportedCurrenciesMenuItem();
            var assignCurrencyForm = supportedCurrenciesPage.OpenAssignCurrencyForm();
            var submittedAssignedCurrencyForm = assignCurrencyForm.Submit(licensee, brandName, currency);
            Assert.AreEqual("The currencies have been successfully assigned", submittedAssignedCurrencyForm.ConfirmationMessage);
            submittedAssignedCurrencyForm.CloseTab("View Assigned Currencies");

            // assign a language to the brand
            var supportedLanguagesPage = submittedAssignedCurrencyForm.Menu.ClickSupportedLanguagesMenuItem();
            var assignLanguageForm = supportedLanguagesPage.OpenAssignLanguageForm();
            
            var submittedAssignedLanguageForm = assignLanguageForm.Submit(licensee, brandName, languageCode);

            Assert.AreEqual("The languages have been successfully assigned", submittedAssignedLanguageForm.ConfirmationMessage);

            // assign a product
            var supportedProductsPage = submittedAssignedLanguageForm.Menu.ClickSupportedProductsMenuItem();
            var assignProductForm = supportedProductsPage.OpenManageProductsPage();
            var submittedAssignProductsForm = assignProductForm.AssignProducts(licensee, brandName, new[] {"Mock Casino"});
            Assert.AreEqual("The products have been successfully assigned", submittedAssignProductsForm.Confirmation);
            submittedAssignProductsForm.CloseTab("View Assigned Products");
            
            // create a bank for the brand
            var bankId = randomString;
            var bankName = "bank" + randomString;
            var banksManagerPage = submittedAssignProductsForm.Menu.ClickBanksItem();
            var newBankForm = banksManagerPage.OpenNewBankForm();
            var submittedBankForm = newBankForm.SubmitWithLicensee(licensee, brandName, bankId, bankName, country, remarks:"new bank");
            submittedBankForm.CloseTab("View Bank");
            
            // create a bank account for the brand
            var bankAccountName = "bankAccount" + randomString;
            var bankAccountNumber = TestDataGenerator.GetRandomBankAccountNumber(8);
            var bankAccountId = "ba" + randomString;
            var branchProvince = "brand" + randomString;
            
            var bankAccountsManagerPage = banksManagerPage.Menu.ClickBankAccountsItem();
            var newBankAccountForm = bankAccountsManagerPage.OpenNewBankAccountForm();
            var submittedBankAccountForm = newBankAccountForm.SubmitWithLicensee(licensee, brandName, bankAccountId, bankAccountName, bankAccountNumber,
                bankName, branchProvince);
            submittedBankAccountForm.CloseTab("View Bank Account");
            
            bankAccountsManagerPage = _dashboardPage.Menu.ClickBankAccountsItem();
            var activateDialog = bankAccountsManagerPage.OpenActivateBankAccountDialog(bankAccountName);
            var confirmDialog = activateDialog.ActivateBankAccount(remark:"activated");
            bankAccountsManagerPage = confirmDialog.Close();
            
            // create Default payment level for the brand
            var paymentLevelCode = "pl" + randomString;
            var paymentLevelName = "payment-level" + randomString;
            var paymentLevelsPage = bankAccountsManagerPage.Menu.ClickPaymentLevelsMenuItem();
            var newPaymentLevelForm = paymentLevelsPage.OpenNewPaymentLevelForm();
            var submittedPaymentLevelForm = newPaymentLevelForm.Submit(brandName, paymentLevelCode, paymentLevelName, bankAccountId);
            Assert.AreEqual("The payment level has been created.", submittedPaymentLevelForm.ConfirmationMessage);
            submittedPaymentLevelForm.CloseTab("View Payment Level");
            
            // create Default vip level for the brand
            var vipLevelData = TestDataGenerator.CreateValidVipLevelData(licensee, brandName, defaultForNewPlayers:true);

            var vipLevelManagerPage = paymentLevelsPage.Menu.ClickVipLevelManagerMenuItem();
            var newVipLevelForm = vipLevelManagerPage.OpenNewVipLevelForm();
            newVipLevelForm.EnterVipLevelDetails(vipLevelData);
            var submittedVipLevelForm = newVipLevelForm.Submit();

            Assert.AreEqual("VIP Level has been created successfully.", submittedVipLevelForm.ConfirmationMessage);

            //create a risk level for the brand
            var fraudManagerPage = vipLevelManagerPage.Menu.ClickFraudManager();
            var submittedRiskLevelForm = fraudManagerPage.CreateRiskLevel(new RiskLevelTestingDto
            {
                Licensee = licensee,
                Brand = brandName,                
                Name = TestDataGenerator.GetRandomAlphabeticString(5),
                Level = TestDataGenerator.GetRandomNumber(100),
                Remarks = TestDataGenerator.GetRandomAlphabeticString(10)
            });

            Assert.AreEqual("The FraudRiskLevel has been successfully created", submittedRiskLevelForm.ConfirmationMessage);
            submittedRiskLevelForm.CloseTab("New Fraud");

            //activate the Brand
            _brandManagerPage = fraudManagerPage.Menu.ClickBrandManagerItem();
            var brandActivateDialog = _brandManagerPage.OpenBrandActivateDialog(brandName);
            var brandActivatedConfirmDialog = brandActivateDialog.Activate("activated");
            Assert.AreEqual("This brand has been successfully activated.", brandActivatedConfirmDialog.ConfirmationMessage);
            brandActivatedConfirmDialog.Close();

            //check the Brand satus
            Assert.IsTrue(_brandManagerPage.HasActiveStatus(brandName));
        }

        [Test]
        public void Can_edit_brand()
        {
            var brandName = "brand-" + TestDataGenerator.GetRandomAlphabeticString(6);
            var brandCode = TestDataGenerator.GetRandomString(size: 4, charsToUse: TestDataGenerator.NumericChars);
            var playerPrefix = TestDataGenerator.GetRandomAlphabeticString(3);
            var newBrandForm = _brandManagerPage.OpenNewBrandForm();
            var submittedForm = newBrandForm.Submit(brandName, brandCode, playerPrefix);
            submittedForm.CloseTab("View Brand");

            _driver.Logout();
            _dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();
            _dashboardPage.BrandFilter.SelectAll();
            _brandManagerPage = _dashboardPage.Menu.ClickBrandManagerItem();

            var editBrandName = "brand-" + TestDataGenerator.GetRandomAlphabeticString(6);
            var editBrandCode = TestDataGenerator.GetRandomString(size: 4, charsToUse: TestDataGenerator.NumericChars);
            var editBrandForm = _brandManagerPage.OpenEditBrandForm(brandName);
            var submittedEditForm = editBrandForm.EditOnlyRequiredData("Deposit", editBrandName, editBrandCode);

            Assert.AreEqual(editBrandCode, submittedEditForm.BrandCodeValue);
            Assert.AreEqual(editBrandName, submittedEditForm.BrandNameValue);
        }

        [Test]
        public void Cannot_activate_brand_without_country_currency_language_vip_level()
        {
            var brand = "brand-" + TestDataGenerator.GetRandomAlphabeticString(6);
            var brandCode = TestDataGenerator.GetRandomString(size:4, charsToUse:TestDataGenerator.NumericChars);
            var playerPrefix = TestDataGenerator.GetRandomAlphabeticString(3);

            var newBrandForm = _brandManagerPage.OpenNewBrandForm();
            var submittedForm = newBrandForm.Submit(brand, brandCode, playerPrefix);
            submittedForm.CloseTab("View Brand");

            _driver.Logout();
            _dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();
            _dashboardPage.BrandFilter.SelectAll();
            _brandManagerPage = _dashboardPage.Menu.ClickBrandManagerItem();
            
            var activateDialog = _brandManagerPage.OpenBrandActivateDialog(brand);
            activateDialog.TryToActivate("approved");
            var validationMessages = activateDialog.GetErrorMessages().ToArray();
            
            Assert.That(validationMessages.Length, Is.EqualTo(7));
            Assert.That(validationMessages[0].Text, Is.EqualTo("A wallet must be assigned prior to activation."));
            Assert.That(validationMessages[1].Text, Is.EqualTo("A default VIP level is required prior to activation."));
            Assert.That(validationMessages[2].Text, Is.EqualTo("A country must be assigned prior to activation."));
            Assert.That(validationMessages[3].Text, Is.EqualTo("A currency must be assigned prior to activation."));
            Assert.That(validationMessages[4].Text, Is.EqualTo("A language must be assigned prior to activation."));
            Assert.That(validationMessages[5].Text, Is.EqualTo("A product must be assigned prior to activation."));
            Assert.That(validationMessages[6].Text, Is.EqualTo("A risk level is required prior to activation."));
        }

        [Test]
        public void Can_view_brand()
        {
            const string defaultLicensee = "Flycow";
            var brandQueries = _container.Resolve<BrandQueries>();
            var licensee = brandQueries.GetLicensees().First(x => x.Name == defaultLicensee);
           
            var brandTestHelper = _container.Resolve<BrandTestHelper>();
            var brand = brandTestHelper.CreateBrand(licensee);

            _driver.Navigate().Refresh();
            _dashboardPage.BrandFilter.SelectAll();
            _brandManagerPage = _dashboardPage.Menu.ClickBrandManagerItem();

            var viewBrandForm = _brandManagerPage.OpenViewBrandForm(brand.Name);
            
            Assert.AreEqual(licensee.Name, viewBrandForm.Licensee);
            Assert.AreEqual(brand.Name, viewBrandForm.BrandName);
            Assert.AreEqual(brand.Type.ToString(), viewBrandForm.BrandType);
            Assert.AreEqual(brand.Code, viewBrandForm.BrandCode);
            Assert.AreEqual(brand.Status.ToString(), viewBrandForm.Status);
            Assert.AreEqual(brand.PlayerPrefix, viewBrandForm.PlayerPrefix);
        }
    }
}
