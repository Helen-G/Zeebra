using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class SubmittedBankAccountForm : BackendPageBase
    {
        public SubmittedBankAccountForm(IWebDriver driver) : base(driver)
        {
        }

        public const string FormXPath = "//div[contains(@class, 'tab-content') and not(contains(@style, 'display: none'))]" +
                                        "/div/div[@data-view='payments/bank-accounts/add' and @data-active-view='true']/form";

        public string ConfirmationMessage
        {
            get { return _driver.FindElementValue(By.XPath("//div[@class='alert alert-success']")); }
        }

        public string BrandNameValue
        {
            get
            {
                return _driver.FindElementValue(By.XPath(FormXPath + "//div[contains(@data-bind, 'with: form.fields.brand')]/p[contains(@data-bind, 'text: display')]")); 
            }
        }

        public string BankAccountIdValue
        {
            get
            {
                return _driver.FindElementValue(By.XPath(FormXPath + "//p[contains(@data-bind, 'text: fields.accountId')]"));
            }
        }

        public string BankAccountNameValue
        {
            get
            {
                return _driver.FindElementValue(By.XPath(FormXPath + "//p[contains(@data-bind, 'text: fields.accountName')]"));
            }
        }

        public string BankAccountNumberValue
        {
            get
            {
                return _driver.FindElementValue(By.XPath(FormXPath + "//p[contains(@data-bind, 'text: fields.accountNumber')]"));
            }
        }

        public string BankNameValue
        {
            get
            {
                return _driver.FindElementValue(By.XPath(FormXPath + "//p[contains(@data-bind, 'text: bankName')]"));
            }
        }

        public string ProvinceValue
        {
            get
            {
                return _driver.FindElementValue(By.XPath(FormXPath + "//p[contains(@data-bind, 'text: fields.province')]"));
            }
        }

        public string BranchValue
        {
            get
            {
                return _driver.FindElementValue(By.XPath(FormXPath + "//p[contains(@data-bind, 'text: fields.branch')]"));
            }
        }

        public BankAccountManagerPage SwitchToList()
        {
            var bankAccountstab = _driver.FindElementWait(By.XPath("//ul[@data-view='layout/document-container/tabs']//span[text()='Bank Accounts']"));
            bankAccountstab.Click();
            var page = new BankAccountManagerPage(_driver);
            return page;
        }
        
    }
}