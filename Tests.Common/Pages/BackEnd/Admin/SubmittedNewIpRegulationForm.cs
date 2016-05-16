using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class ViewIpRegulationForm : BackendPageBase
    {
        public ViewIpRegulationForm(IWebDriver driver)
            : base(driver)
        {
        }

        public string XPath = "//div[@data-view='admin/ip-regulation-manager/add-edit-ip-regulation']";

        public string ConfirmationMessage
        {
            get { return _driver.FindElementValue(By.XPath("//div[contains(@data-bind, 'visible: message')]")); }
        }

        public string IpAddress
        {
            get { return _driver.FindElementValue(By.XPath("//p[contains(@data-bind, 'text: Model.ipAddress')]")); }
        }

        public string IpAddressValidation
        {
            get { return _driver.FindElementValue(By.XPath("//span[contains(@data-bind, 'validationMessage: Model.ipAddress')]")); }
        }

        public string MultipleIpAddresses
        {
            get { return _driver.FindElementValue(By.XPath("//p[contains(@data-bind, 'text: Model.ipAddressBatch')]")); }
        }

        public string Restriction
        {
            get { return _driver.FindElementValue(By.XPath("//p[contains(@data-bind, 'text: Model.isAccessible.ToStatus()')]")); }
        }

        public string AppliedTo
        {
            get
            {
                return _driver.FindElementValue(By.XPath("//p[contains(@data-bind, 'text: Model.appliedTo')]")); 
            }
        }

        public void Close()
        {
            var closeButton = _driver.FindElementWait(By.XPath("//button[text() = 'Close']"));
            closeButton.Click();
        }
    }
}
