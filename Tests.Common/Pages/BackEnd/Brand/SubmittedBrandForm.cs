using System;
using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class SubmittedBrandForm : BackendPageBase
    {
        public SubmittedBrandForm(IWebDriver driver) : base(driver) {}

        public string ConfirmationMessage
        {
            get
            {
                var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
                var message = _driver.FindElementWait(By.XPath("//div[@class[contains(., 'alert alert-success')]]"));
                wait.Until(x => message.Displayed);
                return message.Text;
            }
        }

        public string LicenseeValue
        {
            get
            {
                return _driver.FindElementWait(By.XPath("//p[@data-bind='text: licensee']")).Text;
            }
        }

        public string BrandTypeValue
        {
            get
            {
                return _driver.FindElementWait(By.XPath("//p[@data-bind='text: type']")).Text;
                
            }
        }

        public string BrandNameValue
        {
            get
            {

                return _driver.FindElementWait(By.XPath("//p[@data-bind='text: name']")).Text;
                
            }
        }

        public string BrandCodeValue
        {
            get
            {
                return _driver.FindElementWait(By.XPath("//p[@data-bind='text: code']")).Text;
            }
        }
        

        public string PlayerPrefix
        {
            get
            {
                return _driver.FindElementWait(By.XPath("//p[@data-bind='text: playerPrefix']")).Text;
            }
        }
    }
}