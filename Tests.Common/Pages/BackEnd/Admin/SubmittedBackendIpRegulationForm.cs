using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class SubmittedBackendIpRegulationForm : BackendPageBase
    {
        public SubmittedBackendIpRegulationForm(IWebDriver driver) : base(driver){ }

        public string ConfirmationMessage
        {
            get
            {
                return _driver.FindElementValue(By.XPath("//div[contains(@data-bind, 'visible: message')]"));
            }
        }

        public string IpAddressValidation
        {
            get
            {
                return _driver.FindElementValue(By.XPath("//span[contains(@data-bind, 'Model.ipAddress')]"));
            }
        }

        public void Close()
        {
            var close = _driver.FindElementWait(By.XPath("//button[text()='Close']"));
            close.Click();
        }
    }
}