using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;
using OpenQA.Selenium.Support.UI;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class NewPaymentSettingsForm : BackendPageBase
    {
        public NewPaymentSettingsForm(IWebDriver driver) : base(driver){}

        public SubmittedPaymentSettingsForm Submit(PaymentSettingsData data)
        {
            SelectLicenseeBrand(By.XPath("//label[contains(@for, 'payment-settings-licensee')]"),
                By.XPath("//select[contains(@id, 'payment-settings-licensee')]"), data.Licensee, By.XPath("//select[contains(@id, 'payment-settings-brand')]"), data.Brand);
            var currency = new SelectElement(_currencyField);
            currency.SelectByText(data.Currency);
            var paymentType = new SelectElement(_paymentTypeField);
            paymentType.SelectByText(data.PaymentType);
            if (data.VipLevel != null)
            {
                var vipLevel = new SelectElement(_vipLevel);
                vipLevel.SelectByText(data.VipLevel);
            }
            if (data.PaymentMethod != null)
            {
                var paymentMethod = new SelectElement(_paymentMethod);
                paymentMethod.SelectByText(data.PaymentMethod);
            }
            _minAmountPerTransaction.Clear();
            _minAmountPerTransaction.SendKeys(data.MinAmountPerTransaction);
            _maxAmountPerTransaction.Clear();
            _maxAmountPerTransaction.SendKeys(data.MaxAmountPerTransaction);
            _maxAmountPerDay.SendKeys(data.MaxAmountPerDay);
            _maxTransactionPerDay.SendKeys(data.MaxTransactionsPerDay);
            _maxTransactionPerWeek.SendKeys(data.MaxTransactionsPerWeek);
            _maxTransactionPerMonth.SendKeys(data.MaxTransactionsPerMonth);
            _driver.ScrollPage(0, 600);
            _saveButton.Click();
            var submittedForm = new SubmittedPaymentSettingsForm(_driver);
            return submittedForm;
        }

        public PaymentSettingsPage SwitchToList()
        {
            var bankAccountstab = _driver.FindElementWait(By.XPath("//ul[@data-view='layout/document-container/tabs']//span[text()='Settings']"));
            bankAccountstab.Click();
            var page = new PaymentSettingsPage(_driver);
            return page;
        }

#pragma warning disable 649
        [FindsBy(How = How.XPath, Using = "//select[contains(@id, 'payment-settings-currency')]")]
        private IWebElement _currencyField;

        [FindsBy(How = How.XPath, Using = "//select[contains(@id, 'payment-settings-payment-type')]")]
        private IWebElement _paymentTypeField;

        [FindsBy(How = How.XPath, Using = "//select[contains(@id, 'payment-settings-vip-level')]")]
        private IWebElement _vipLevel;

        [FindsBy(How = How.XPath, Using = "//select[contains(@id, 'payment-settings-payment-method')]")]
        private IWebElement _paymentMethod;

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

        [FindsBy(How = How.XPath, Using = "//div[@data-view='payments/settings/details']//button[text()='Save']")]
        private IWebElement _saveButton;
#pragma warning restore 649


    }
}