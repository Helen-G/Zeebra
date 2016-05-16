using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class OfflineWithrawalAcceptancePage : BackendPageBase
    {
        public OfflineWithrawalAcceptancePage(IWebDriver driver) : base(driver) { }

        public Grid Grid
        {
            get
            {
                return new Grid(_driver, "withdrawal-acceptance-name-search", "withdrawal-acceptance-search-btn");
            }
        }
        
        public static By AcceptButton = By.Id("btn-withdrawal-accept");
        public static By RevertButton = By.Id("btn-withdrawal-revert");

        public AcceptOfflineWithdrawalForm OpenAcceptForm(string username)
        {
            Grid.SelectRecord(username);
            var acceptButton = _driver.FindElementWait(AcceptButton);
            acceptButton.Click();
            var page = new AcceptOfflineWithdrawalForm(_driver);
            return page;
        }
    }

    public class AcceptOfflineWithdrawalForm : BackendPageBase
    {
        public AcceptOfflineWithdrawalForm(IWebDriver driver) : base(driver) { }

        public SubmittedAcceptOfflineWithdrawRequestForm Submit(string remarks)
        {
            var remarksField = _driver.FindElementWait(By.XPath("//textarea[contains(@id, 'withdrawal-acceptance-remarks')]"));
            remarksField.SendKeys(remarks);
            var acceptButton = _driver.FindElementScroll(By.XPath("//button[text()='Accept']"));
            acceptButton.Click();
            var form = new SubmittedAcceptOfflineWithdrawRequestForm(_driver);
            return form;
        }

        public string GetExempted()
        {
            return _driver.FindElementValue(By.XPath("//p[contains(@data-bind, 'text: exempted')]"));
        }
    }

    public class SubmittedAcceptOfflineWithdrawRequestForm : BackendPageBase
    {
        public SubmittedAcceptOfflineWithdrawRequestForm(IWebDriver driver) : base(driver) { }

        public string ConfirmationMessage
        {
            get
            {
                return _driver.FindElementValue(By.XPath("//div[@class='alert alert-success']"));
            }
        }
    }
}