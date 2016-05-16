using System;
using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class ExemptionForm : BackendPageBase
    {
        public ExemptionForm(IWebDriver driver) : base(driver)
        {
        }

        public void Submit()
        {
            var exemptionCheckBox = _driver.FindElementWait(By.XPath("//input[contains(@data-bind, 'checked: fields.exempt.value')]"));
            exemptionCheckBox.Click();

            var exemptTo = DateTime.Now + TimeSpan.FromDays(7);
            var exemptToTextBox = _driver.FindElementWait(By.XPath("//input[contains(@id, 'withdrawal-verification-exemption-exempt-to')]"));
            exemptToTextBox.Clear();
            exemptToTextBox.SendKeys(exemptTo.ToString("yyyy-MM-dd"));
            
            var submitButton = _driver.FindElementWait(By.XPath("//button[contains(@data-bind, 'submitExemption')]"));
            submitButton.Click();
        }
    }
}