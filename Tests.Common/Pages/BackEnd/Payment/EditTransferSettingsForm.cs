using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;
using OpenQA.Selenium.Support.UI;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class EditTransferSettingsForm : BackendPageBase
    {
        public EditTransferSettingsForm(IWebDriver driver) : base(driver){}

        public SubmittedTransferSettingsForm Submit(TransferSettingsData data)
        {
            _clearButton.Click();
            //_minAmountPerTransaction.Clear();
            _minAmountPerTransaction.SendKeys(data.MinAmountPerTransaction);
            //_maxAmountPerTransaction.Clear();
            _maxAmountPerTransaction.SendKeys(data.MaxAmountPerTransaction);
            //_maxAmountPerDay.Clear();
            _maxAmountPerDay.SendKeys(data.MaxAmountPerDay);
            //_maxTransactionPerDay.Clear();
            _maxTransactionPerDay.SendKeys(data.MaxTransactionsPerDay);
            //_maxTransactionPerWeek.Clear();
            _maxTransactionPerWeek.SendKeys(data.MaxTransactionsPerWeek);
            //_maxTransactionPerMonth.Clear();
            _maxTransactionPerMonth.SendKeys(data.MaxTransactionsPerMonth);
            _driver.ScrollPage(0, 600);
            _saveButton.Click();
            var submittedForm = new SubmittedTransferSettingsForm(_driver);
            return submittedForm;
        }

        public TransferSettingsPage SwitchToList()
        {
            var bankAccountstab = _driver.FindElementWait(By.XPath("//ul[@data-view='layout/document-container/tabs']//span[text()='Settings']"));
            bankAccountstab.Click();
            var page = new TransferSettingsPage(_driver);
            return page;
        }

#pragma warning disable 649
        [FindsBy(How = How.XPath, Using = "//input[contains(@data-bind, 'value: fields.minAmountPerTransaction')]")]
        private IWebElement _minAmountPerTransaction;

        [FindsBy(How = How.XPath, Using = "//input[contains(@data-bind, 'value: fields.maxAmountPerTransaction')]")]
        private IWebElement _maxAmountPerTransaction;

        [FindsBy(How = How.XPath, Using = "//input[contains(@data-bind, 'value: fields.maxAmountPerDay')]")]
        private IWebElement _maxAmountPerDay;

        [FindsBy(How = How.XPath, Using = "//input[contains(@data-bind, 'value: fields.maxTransactionPerDay')]")]
        private IWebElement _maxTransactionPerDay;

        [FindsBy(How = How.XPath, Using = "//input[contains(@data-bind, 'value: fields.maxTransactionPerWeek')]")]
        private IWebElement _maxTransactionPerWeek;

        [FindsBy(How = How.XPath, Using = "//input[contains(@data-bind, 'value: fields.maxTransactionPerMonth')]")]
        private IWebElement _maxTransactionPerMonth;

        [FindsBy(How = How.XPath, Using = "//div[@data-view='payments/transfer-settings/details']//button[text()='Save']")]
        private IWebElement _saveButton;

        [FindsBy(How = How.XPath, Using = "//div[@data-view='payments/transfer-settings/details']//button[text()='Clear']")]
        private IWebElement _clearButton;
#pragma warning restore 649

    }

}
