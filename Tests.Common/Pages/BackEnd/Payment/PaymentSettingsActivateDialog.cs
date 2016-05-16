using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class PaymentSettingsActivateDeactivateDialog : BackendPageBase
    {
        public PaymentSettingsActivateDeactivateDialog(IWebDriver driver) : base(driver){}

        public string ConfirmationMessage
        {
            get { return _driver.FindElementValue(By.XPath("//div[contains(@data-bind, 'text: message()')]")); }
        }

        public void EnterRemark(string remark)
        {
            var remarks = _driver.FindElementWait(By.XPath("//textarea[contains(@data-bind, 'value: remarks')]"));
            remarks.SendKeys(remark);
        }

        public void Deactivate()
        {
            var deactivate = _driver.FindElementWait(By.XPath("//div[@data-view='payments/settings/status-dialog']//button[text()='Deactivate']"));
            deactivate.Click();
        }

        public void Activate()
        {
            var deactivate = _driver.FindElementWait(By.XPath("//div[@data-view='payments/settings/status-dialog']//button[text()='Activate']"));
            deactivate.Click();
        }

        public PaymentSettingsPage Close()
        {
            var closeButton = _driver.FindElementWait(By.XPath("//div[@data-view='payments/settings/status-dialog']//button[text()='Close']"));
            closeButton.Click();
            var page = new PaymentSettingsPage(_driver);
            return page;
        }
    }
}