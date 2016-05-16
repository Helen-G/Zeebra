using System.Linq;
using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class NewBankAccountForm : BackendPageBase
    {
        public NewBankAccountForm(IWebDriver driver) : base(driver) { }

        public const string FormXPath = "//li[contains(@class, 'active') and not(contains(@class, 'inactive'))]";
        public string Title
        {
            get
            {
                return _driver.FindElementValue(By.XPath(FormXPath + "//span[text()='New Bank Account']")); 
            }
        }

        public SubmittedBankAccountForm Submit(string brandName, string bankAccountId, string bankAccountName, string bankAccountNumber,
            string bankName, string branchProvince)
        {
            SelectLicenseeBrand(By.XPath("//label[contains(@for, 'bank-account-licensee')]"),
                By.XPath("//select[contains(@id, 'bank-account-licensee')]"), "Flycow", By.XPath("//select[contains(@id, 'bank-account-brand')]"), brandName);

            //ignored until currency list is redesigned
            //var currencyList = _driver.FindElementWait(By.XPath("//select[contains(@id, 'payment-level-currency')]"));
            //var currencyField = new SelectElement(currencyList);
            //currencyField.SelectByText(currency);

            var bankAccountIdField =
                _driver.FindElementWait(By.XPath("//input[contains(@data-bind, 'value: fields.accountId')]"));
            bankAccountIdField.SendKeys(bankAccountId);
            var bankAccountNameField =
                _driver.FindElementWait(By.XPath("//input[contains(@data-bind, 'value: fields.accountName')]"));
            bankAccountNameField.SendKeys(bankAccountName);
            var bankAccountNumberField =
                _driver.FindElementWait(By.XPath("//input[contains(@data-bind, 'value: fields.accountNumber')]"));
            bankAccountNumberField.SendKeys(bankAccountNumber);
            //var bankNameList =
            //    _driver.FindElementWait(By.XPath("//select[contains(@id, 'bank-account-bank')]"));
            //var bankNameField = new SelectElement(bankNameList);
            //bankNameField.SelectByText(bankName);

            var provinceField = _driver.FindElementWait(By.XPath("//input[contains(@data-bind, 'value: fields.province')]"));
            provinceField.SendKeys(branchProvince);
            var branchField = _driver.FindElementWait(By.XPath("//input[contains(@data-bind, 'value: fields.branch')]"));
            branchField.SendKeys(branchProvince);
            _driver.ScrollPage(0, 500);
            var submitButton =
                _driver.FindElementWait(By.XPath("//div[@data-view='payments/bank-accounts/add']//button[text()='Save']"));
            submitButton.Click();
            
            var page = new SubmittedBankAccountForm(_driver);
            return page;
        }

        public SubmittedBankAccountForm SubmitWithLicensee(string licensee, string brand, string bankAccountId, string bankAccountName, string bankAccountNumber,
            string bankName, string branchProvince, string currency = null)
        {
            SelectLicenseeBrand(By.XPath("//label[contains(@for, 'bank-account-licensee')]"), 
                By.XPath("//select[contains(@id, 'bank-account-licensee')]"), licensee, By.XPath("//select[contains(@id, 'bank-account-brand')]"), brand);

            const string currencyFieldXPath = "//select[contains(@id, 'bank-account-currency')]";

            if (currency != null && _driver.FindElements(By.XPath(currencyFieldXPath)).Count(x => x.Displayed && x.Enabled) > 0)
            {
                var currencyList = _driver.FindElementWait(By.XPath(currencyFieldXPath));
                var currencyField = new SelectElement(currencyList);
                currencyField.SelectByText(currency);   
            }            

            var bankAccountIdField =
                _driver.FindElementWait(By.XPath("//input[contains(@data-bind, 'value: fields.accountId')]"));
            bankAccountIdField.SendKeys(bankAccountId);
            var bankAccountNameField =
                _driver.FindElementWait(By.XPath("//input[contains(@data-bind, 'value: fields.accountName')]"));
            bankAccountNameField.SendKeys(bankAccountName);
            var bankAccountNumberField =
                _driver.FindElementWait(By.XPath("//input[contains(@data-bind, 'value: fields.accountNumber')]"));
            bankAccountNumberField.SendKeys(bankAccountNumber);
            
            var provinceField = _driver.FindElementWait(By.XPath("//input[contains(@data-bind, 'value: fields.province')]"));
            provinceField.SendKeys(branchProvince);
            var branchField = _driver.FindElementWait(By.XPath("//input[contains(@data-bind, 'value: fields.branch')]"));
            branchField.SendKeys(branchProvince);
            _driver.ScrollPage(0, 500);
            var submitButton =
                _driver.FindElementWait(By.XPath("//div[@data-view='payments/bank-accounts/add']//button[text()='Save']"));
            submitButton.Click();

            var page = new SubmittedBankAccountForm(_driver);
            return page;
        }
    }
}