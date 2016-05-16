using System;
using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class NewPaymentLevelForm : BackendPageBase
    {
        public NewPaymentLevelForm(IWebDriver driver) : base(driver) { }

        public const string FormXPath = "//li[contains(@class, 'active') and not(contains(@class, 'inactive'))]";
        
        public string Title
        {
            get { return _driver.FindElementValue(By.XPath(FormXPath + "//span[text()='New Payment Level']")); } 
        }

        public SubmittedPaymentLevelForm Submit(string brandName, string paymentLevelCode, string paymentLevelName, string bankAccountId, bool defaultPaymentLevel = true)
        {
            SelectLicenseeBrand(By.XPath("//label[contains(@for, 'payment-level-licensee')]"),
                By.XPath("//select[contains(@id, 'payment-level-licensee')]"), "Flycow", By.XPath("//select[contains(@id, 'payment-level-brand')]"), brandName);
            var paymentLevelCodeField = _driver.FindElementWait(By.XPath("//input[contains(@id, 'payment-level-code')]"));
            paymentLevelCodeField.SendKeys(paymentLevelCode);
            var paymentLevelNameField = _driver.FindElementWait(By.XPath("//input[contains(@id, 'payment-level-name')]"));
            paymentLevelNameField.SendKeys(paymentLevelName);

            if (defaultPaymentLevel)
            {
                var isPaymentLevelDefault = _driver.FindElementWait(By.XPath("//input[contains(@id, 'payment-level-default')]"));
                isPaymentLevelDefault.Click();
            }

            var searchbox =
                _driver.FindElementWait(By.XPath("//input[contains(@id, 'payment-level-bank-account-id-search')]"));
            searchbox.SendKeys(bankAccountId);
            var searchButton = _driver.FindElementWait(By.XPath("//button[contains(@id, 'payment-level-bank-account-search-button')]"));
            searchButton.Click();
            _driver.ScrollPage(0, 200);
            var bankAccountRecord = string.Format("//td[text()=\'{0}\']/preceding-sibling::td/input", bankAccountId);
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(45));
            var bankAccountCheckbox = _driver.FindElementWait(By.XPath(bankAccountRecord));
            wait.Until(d => bankAccountCheckbox.Displayed);
            bankAccountCheckbox.Click();
             _driver.ScrollPage(0, 700);
            var submitButton =
                _driver.FindElementWait(By.XPath("//div[@data-view='payments/level-manager/edit']//button[text()='Save']"));
            submitButton.Click();
            var submittedPaymentLevelForm = new SubmittedPaymentLevelForm(_driver);
            return submittedPaymentLevelForm;
        }

        public SubmittedPaymentLevelForm SubmitWithLicensee(string licensee, string brandName, string paymentLevelCode, string paymentLevelName, string bankAccountId, string currency)
        {
            SelectLicenseeBrand(By.XPath("//label[contains(@for, 'payment-level-licensee')]"),
               By.XPath("//select[contains(@id, 'payment-level-licensee')]"), licensee, By.XPath("//select[contains(@id, 'payment-level-brand')]"), brandName);

            var currencyList = _driver.FindElementWait(By.XPath("//select[contains(@id, 'payment-level-currency')]"));
            var currencyField = new SelectElement(currencyList);
            currencyField.SelectByText(currency);

            var paymentLevelCodeField = _driver.FindElementWait(By.XPath("//input[contains(@id, 'payment-level-code')]"));
            paymentLevelCodeField.SendKeys(paymentLevelCode);
            var paymentLevelNameField = _driver.FindElementWait(By.XPath("//input[contains(@id, 'payment-level-name')]"));
            paymentLevelNameField.SendKeys(paymentLevelName);

            var isPaymentLevelDefault = _driver.FindElementWait(By.XPath("//input[contains(@id, 'payment-level-default')]"));
            isPaymentLevelDefault.Click();

            var searchbox =
                _driver.FindElementWait(By.XPath("//input[contains(@id, 'payment-level-bank-account-id-search')]"));
            searchbox.SendKeys(bankAccountId);
            var searchButton = _driver.FindElementWait(By.XPath("//button[contains(@id, 'payment-level-bank-account-search-button')]"));
            searchButton.Click();
            var bankAccountRecord = string.Format("//td[text()=\'{0}\']/preceding-sibling::td/input", bankAccountId);
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(45));
            var bankAccountCheckbox = _driver.FindElementWait(By.XPath(bankAccountRecord));
            wait.Until(d => bankAccountCheckbox.Displayed);
            bankAccountCheckbox.Click();
            _driver.ScrollPage(0, 900);
            var submitButton =
                _driver.FindElementWait(By.XPath("//div[@data-view='payments/level-manager/edit']//button[text()='Save']"));
            submitButton.Click();
            var submittedPaymentLevelForm = new SubmittedPaymentLevelForm(_driver);
            return submittedPaymentLevelForm;
        }
    }
}
