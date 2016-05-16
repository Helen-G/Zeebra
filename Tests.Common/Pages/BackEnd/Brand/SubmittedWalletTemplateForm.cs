using System;
using System.Collections;
using System.Collections.Generic;
using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class SubmittedWalletTemplateForm : BackendPageBase
    {
        public SubmittedWalletTemplateForm(IWebDriver driver) : base(driver)
        {
        }

        public string ConfirmationMessage
        {
            get
            {
                var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
                var message = _driver.FindElementWait(By.XPath("//div[contains(@class, 'alert alert-success')]"));
                wait.Until(x => message.Displayed);
                return message.Text;
            }
        }

        public string ProductWalletName
        {
            get { return _driver.FindElementValue(By.XPath("//div[@data-bind='foreach: productWallets']//p")); }
        }

        public IList<IWebElement> GetAssignedProducts()
        {
            var listXPath = _driver.FindElement(By.XPath("//div[@data-bind='foreach: productWallets']//select[contains(@data-bind, 'enable: $root.submitted()')]"));
            var assignedProductsList = new SelectElement(listXPath);
            return assignedProductsList.Options;
        }
    }
}