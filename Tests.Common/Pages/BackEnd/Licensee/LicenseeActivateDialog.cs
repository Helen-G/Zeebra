using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class LicenseeActivateDialog : BackendPageBase
    {
        public LicenseeActivateDialog(IWebDriver driver) : base(driver) { }

        public const string DialogXPath = "//div[@data-view='licensee-manager/status-dialog']";
        
        public void TryToActivate(string description)
        {
            var descriptionFiled =
                _driver.FindElementWait(By.XPath(DialogXPath +"//textarea"));
            descriptionFiled.SendKeys(description);
            var activateButton = _driver.FindElementWait(By.XPath(DialogXPath + "//button[text()='Activate']"));
            activateButton.Click();
        }

        public IEnumerable<IWebElement> GetErrorMessages()
        {
            var validationMessages = _driver.FindElementsWait(By.XPath("//ul[@data-bind='foreach: errorsArray']/li")).ToArray();
            return validationMessages;
        }

        public SubmittedActivateLicenseeDialog Activate(string description)
        {
            var descriptionFiled =
                _driver.FindElementWait(By.XPath(DialogXPath + "//textarea"));
            descriptionFiled.SendKeys(description);
            var activateButton = _driver.FindElementWait(By.XPath(DialogXPath + "//button[text()='Activate']"));
            activateButton.Click();
            var dialog = new SubmittedActivateLicenseeDialog(_driver);
            return dialog;
        }
    }

    public class SubmittedActivateLicenseeDialog : BackendPageBase
    {
        public SubmittedActivateLicenseeDialog(IWebDriver driver)
            : base(driver)
        {
        }

        public string ConfirmationMessage
        {
            get
            {
                return
                    _driver.FindElementValue(
                        By.XPath(
                            "//div[@data-view='licensee-manager/status-dialog']/div/div[@class='alert alert-success']"));
            }
        }

        public LicenseeManagerPage Close()
        {
            var closeButton = _driver.FindElementWait(By.XPath("//div[@data-view='licensee-manager/status-dialog']//button[text()='Close']"));
            closeButton.Click();
            Thread.Sleep(500);
            var page = new LicenseeManagerPage(_driver);
            return page;
        }
    }
}