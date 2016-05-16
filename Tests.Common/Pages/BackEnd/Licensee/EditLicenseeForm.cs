using System;
using System.Collections.Generic;
using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;
using OpenQA.Selenium.Support.UI;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class EditLicenseeForm : BackendPageBase
    {
        public EditLicenseeForm(IWebDriver driver) : base(driver) {}

        private const string BaseXPath =
            "//div[contains(@class, 'tab-content') and not(contains(@style, 'display: none'))]//*[@data-view='licensee-manager/edit']";

        public ViewLicenseeForm EditAssignedProducts(string product)
        {          
            _driver.SelectFromMultiSelect("productsAssignControl", product);

            var remarksField = _driver.FindElementWait(By.XPath("//textarea[contains(@id, 'licensee-remarks')]"));
            remarksField.SendKeys("remarks");

            var saveButton = _driver.FindElementWait(By.XPath("//button[text()='Save']"));
            saveButton.Click();
            var submittedForm = new ViewLicenseeForm(_driver);
            return submittedForm;
        }

        public ViewLicenseeForm Submit(LicenseeData licenseeData)
        {
            _driver.ScrollPage(800, 0);

            if(licenseeData.LicenseeName != null)
                _licenseeName.SendKeys(licenseeData.LicenseeName);

            if(licenseeData.CompanyName != null)
                _companyName.SendKeys(licenseeData.CompanyName);

            if (!licenseeData.AffiliateSystem)
                _affiliateSystem.Click();

            if(licenseeData.ContractStart != null)
                _contractStart.SendKeys(licenseeData.ContractStart);
            
            if (licenseeData.ContractEnd != null)
            {
                if (_openEnded.Selected)
                    _openEnded.Click();

                _contractEnd.Clear();
                _contractEnd.SendKeys(licenseeData.ContractEnd);
            }
           
            if (licenseeData.Email != null)
                _email.SendKeys(licenseeData.Email);

           if (licenseeData.Timezone != null)
            {
                var list = new SelectElement(_timezone);
                list.SelectByText(licenseeData.Timezone);
            }

            if (licenseeData.NumberOfAllowedBrands != null)
                _numberOfAllowedBrands.SendKeys(licenseeData.NumberOfAllowedBrands);

            if (licenseeData.NumberOfAllowedWebsitesPerBrand != null)
                _numberOfAllowedWebsitesPerBrand.SendKeys(licenseeData.NumberOfAllowedWebsitesPerBrand);

            if (licenseeData.AvailableProducts != null)
            {
                licenseeData.AvailableProducts.ForEach(x => _driver.SelectFromMultiSelect("productsAssignControl", x));
            }

            if (licenseeData.AvailableCurrencies != null)
            {
                licenseeData.AvailableCurrencies.ForEach(x => _driver.SelectFromMultiSelect("currenciesAssignControl", x));
            }

            if (licenseeData.AvailableCountries != null)
            {
                licenseeData.AvailableCountries.ForEach(x => _driver.SelectFromMultiSelect("countriesAssignControl", x));
            }

            if (licenseeData.AvailableLanguages != null)
            {
                licenseeData.AvailableLanguages.ForEach(x => _driver.SelectFromMultiSelect("languagesAssignControl", x));
            }

            _remarks.SendKeys(licenseeData.Remarks);
            _saveButton.Click();
            var form = new ViewLicenseeForm(_driver);
            form.Initialize();
            return form;
        }

        public void ClearFieldsOnForm()
        {
            const string editLicenseeFormXPath = "licensee-manager/edit";
            base.ClearFieldsOnForm(editLicenseeFormXPath);
        }

#pragma warning disable 649
        [FindsBy(How = How.XPath, Using = BaseXPath + "//input[contains(@id, 'licensee-name')]")]
        private IWebElement _licenseeName;

        [FindsBy(How = How.XPath, Using = BaseXPath + "//input[contains(@id, 'licensee-company-name')]")]
        private IWebElement _companyName;

        [FindsBy(How = How.XPath, Using = BaseXPath + "//input[contains(@id, 'licensee-affiliate-system')]")]
        private IWebElement _affiliateSystem;

        [FindsBy(How = How.XPath, Using = BaseXPath + "//input[contains(@id, 'licensee-contract-start')]")]
        private IWebElement _contractStart;

        [FindsBy(How = How.XPath, Using = BaseXPath + "//input[contains(@id, 'licensee-contract-end')]")]
        private IWebElement _contractEnd;

        [FindsBy(How = How.XPath, Using = BaseXPath + "//input[contains(@id, 'licensee-email')]")]
        private IWebElement _email;

        [FindsBy(How = How.XPath, Using = BaseXPath + "//input[contains(@id, 'licensee-open-ended')]")]
        private IWebElement _openEnded;

        [FindsBy(How = How.XPath, Using = BaseXPath + "//div[@data-bind='with: form.fields.timeZoneId']/select")]
        private IWebElement _timezone;

        [FindsBy(How = How.XPath, Using = BaseXPath + "//input[contains(@id, 'licensee-brand-count')]")]
        private IWebElement _numberOfAllowedBrands;

        [FindsBy(How = How.XPath, Using = BaseXPath + "//input[contains(@id, 'licensee-website-count')]")]
        private IWebElement _numberOfAllowedWebsitesPerBrand;

        [FindsBy(How = How.XPath, Using = BaseXPath + "//textarea[contains(@id, 'licensee-remarks')]")]
        private IWebElement _remarks;

        [FindsBy(How = How.XPath, Using = BaseXPath + "//button[text()='Save']")]
        private IWebElement _saveButton;

#pragma warning restore 649
    }
}