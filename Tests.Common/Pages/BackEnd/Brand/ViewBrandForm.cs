using System;
using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class ViewBrandForm : BackendPageBase
    {
        public ViewBrandForm(IWebDriver driver) : base(driver) { }

        public string ConfirmationMessage
        {
            get
            {
                var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
                var message = _driver.FindElementWait(By.XPath("//div[@class[contains(., 'alert-success')]]"));
                wait.Until(x => message.Displayed);
                return message.Text;
            }
        }

        public string BrandName
        {
            get { return _driver.FindElementValue(By.XPath("//p[@data-bind='text: name']")); }
        }

        public string BrandCode
        {
            get { return _driver.FindElementValue(By.XPath("//p[@data-bind='text: code']")); }
        }

        public string BrandType
        {
            get { return _driver.FindElementValue(By.XPath("//p[@data-bind='text: type']")); }
        }

        public string PlayerPrefix
        {
            get { return _driver.FindElementValue(By.XPath("//p[@data-bind='text: playerPrefix']")); }
        }

        public string Status
        {
            get { return _driver.FindElementValue(By.XPath("//p[@data-bind='text: status']")); }
        }

        public string Licensee
        {
            get
            {
                return _driver.FindElementValue(By.XPath("//p[@data-bind='text: licensee']"));
            }
        }
    }
}