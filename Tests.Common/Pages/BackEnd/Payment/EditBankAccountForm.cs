using System.Linq;
using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;
using OpenQA.Selenium.Support.UI;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class EditBankAccountForm : BackendPageBase
    {
        public EditBankAccountForm(IWebDriver driver) : base(driver) {}

        public SubmittedBankAccountForm Submit(BankAccountData data, string currencyValue, string bank)
        {
            SelectCurrency(
                loadingComplete:By.XPath("//label[contains(@for, 'bank-account-currency')]"), 
                currencyListSelector:By.XPath("//select[contains(@id, 'bank-account-currency')]"), 
                currency:currencyValue);
            _clearButton.Click();

            _bankAccountID.SendKeys(data.ID);
            _bankAccountName.SendKeys(data.Name);
            _bankAccountNumber.SendKeys(data.Number);
            _bankAccountType.SendKeys(data.Type);
            _bankAccountProvince.SendKeys(data.Province);
            _bankAccountBranch.SendKeys(data.Branch);
            _bankAccountRemarks.SendKeys(data.Remarks);
            _saveButton.Click();
            var submittedForm = new SubmittedBankAccountForm(_driver);
            return submittedForm;
        }

        private void SelectCurrency(By loadingComplete, By currencyListSelector, string currency)
        {
            _driver.FindElementWait(loadingComplete);
            if (_driver.FindElements(currencyListSelector).Count(x => x.Displayed && x.Enabled) > 0)
            {
                var currencyList = _driver.FindElementWait(currencyListSelector);
                var currencyField = new SelectElement(currencyList);
                currencyField.SelectByText(currency);
            }
        }

#pragma warning disable 649

        [FindsBy(How = How.XPath, Using = "//input[contains(@data-bind, 'value: fields.accountId')]")]
        private IWebElement _bankAccountID;

        [FindsBy(How = How.XPath, Using = "//input[contains(@data-bind, 'value: fields.accountName')]")]
        private IWebElement _bankAccountName;

        [FindsBy(How = How.XPath, Using = "//input[contains(@data-bind, 'fields.accountNumber')]")]
        private IWebElement _bankAccountNumber;

        [FindsBy(How = How.XPath, Using = "//input[contains(@data-bind, 'fields.accountType')]")]
        private IWebElement _bankAccountType;

        [FindsBy(How = How.XPath, Using = "//input[contains(@data-bind, 'fields.province')]")] 
        private IWebElement _bankAccountProvince;

        [FindsBy(How = How.XPath, Using = "//input[contains(@data-bind, 'value: fields.branch')]")]
        private IWebElement _bankAccountBranch;

        [FindsBy(How = How.XPath, Using = "//textarea[contains(@data-bind, 'value: fields.remarks')]")]
        private IWebElement _bankAccountRemarks;

        [FindsBy(How = How.XPath, Using = "//button[text()='Save']")]
        private IWebElement _saveButton;

        [FindsBy(How = How.XPath, Using = "//button[contains(@data-bind, 'click: clear')]")]
        private IWebElement _clearButton;

#pragma warning restore 649
    }
}