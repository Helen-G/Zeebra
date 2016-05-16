using System.Threading;
using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class SubmittedActivateBankAccountDialog : BackendPageBase
    {
        public SubmittedActivateBankAccountDialog(IWebDriver driver) : base(driver) {}

        public string ConfirmationMessage
        {
            get
            {
                return
                    _driver.FindElementValue(
                        By.XPath(
                            "//div[@data-view='payments/bank-accounts/status-dialog']/div/div[@class='alert alert-success']"));
            }
        }

        public BankAccountManagerPage Close()
        {
            var closeButton = _driver.FindElementWait(By.XPath("//div[@data-view='payments/bank-accounts/status-dialog']//button[text()='Close']"));
            closeButton.Click();
            Thread.Sleep(500);
            var page = new BankAccountManagerPage(_driver);
            return page;
        }
    }
}