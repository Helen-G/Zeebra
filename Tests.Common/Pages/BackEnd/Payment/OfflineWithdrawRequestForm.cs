using System;
using AFT.RegoV2.Domain.Payment.Data;
using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;
using OpenQA.Selenium.Support.UI;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class OfflineWithdrawRequestForm : BackendPageBase
    {
        public OfflineWithdrawRequestForm(IWebDriver driver) : base(driver)
        {
        }

        public SubmittedOfflineWithdrawRequestForm Submit(OfflineWithdrawRequestData data)
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(45));
            var provinceField = _driver.FindElementWait(By.XPath("//input[contains(@id, 'withdraw-request-amount')]"));
            wait.Until(d => provinceField.Displayed && provinceField.Enabled);

            _amount.SendKeys(data.Amount);
            _remarks.SendKeys(data.Remarks);
            var saveButton = _driver.FindElementWait(By.XPath("//button[text()='Save']"));
            saveButton.Click();
            var form = new SubmittedOfflineWithdrawRequestForm(_driver);
            form.Initialize();
            return form;
        }

        public void TryToSubmit(string amount, NotificationMethod notificationMethod)
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(45));
            var provinceField = _driver.FindElementWait(By.XPath("//input[contains(@id, 'withdraw-request-amount')]"));
            wait.Until(d => provinceField.Displayed && provinceField.Enabled);

            _amount.SendKeys(amount);
            _remarks.SendKeys(TestDataGenerator.GetRandomString(5));
            var saveButton = _driver.FindElementWait(By.XPath("//button[text()='Save']"));
            saveButton.Click();
        }

        public string ValidationMessage
        {
            get
            {
                return
                    _driver.FindElementValue(By.XPath("//div[contains(@data-view, 'payments/withdrawal/request')]/div"));
            }
        }

#pragma warning disable 649
        [FindsBy(How = How.XPath, Using = "//input[contains(@id, 'withdraw-request-amount')]")]
        private IWebElement _amount;

        [FindsBy(How = How.XPath, Using = "//textarea[contains(@id, 'withdraw-request-remarks')]")]
        private IWebElement _remarks;
#pragma warning restore 649

    }
}