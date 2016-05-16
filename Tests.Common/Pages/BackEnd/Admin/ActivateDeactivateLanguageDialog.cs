using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class ActivateDeactivateLanguageDialog : BackendPageBase
    {
        public string ResponseMessage { get; private set; }

        public ActivateDeactivateLanguageDialog(IWebDriver driver)
            : base(driver)
        {
        }

        public void Activate()
        {
            var remarks = _driver.FindElementWait(By.XPath("//textarea[contains(@data-bind, 'value: remarks')]"));
            remarks.SendKeys("remark");
            var activate = _driver.FindElementWait(By.XPath("//button[text()='Activate']"));
            activate.Click();
            ResponseMessage = _driver.FindElementValue(By.XPath("//div[contains(@data-bind, 'text: resultMessage')]"));
        }

        public void Deactivate()
        {
            var remarks = _driver.FindElementWait(By.XPath("//textarea[contains(@data-bind, 'value: remarks')]"));
            remarks.SendKeys("remark");
            var deactivate = _driver.FindElementWait(By.XPath("//button[text()='Deactivate']"));
            deactivate.Click();
            ResponseMessage = _driver.FindElementValue(By.XPath("//div[contains(@data-bind, 'text: resultMessage')]"));
        }

        public void CloseDialog()
        {
            var closeButton = _driver.FindElementWait(By.XPath("//button[text()='Close']"));
            closeButton.Click();
        }
    }
}