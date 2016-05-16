using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class OfflineWithrawalApprovalPage : BackendPageBase
    {
        public OfflineWithrawalApprovalPage(IWebDriver driver) : base(driver) { }

        public Grid Grid
        {
            get
            {
                return new Grid(_driver, "withdrawal-approval-name-search", "withdrawal-approval-search-btn");
            }
        }

        public static By ApproveButton = By.Id("btn-withdrawal-approve");
        public static By RejectButton = By.Id("btn-withdrawal-reject");

        public ApproveOfflineWithdrawalForm OpenVerifyForm(string username)
        {
            Grid.SelectRecord(username);
            var approveButton = _driver.FindElementWait(ApproveButton);
            approveButton.Click();
            var page = new ApproveOfflineWithdrawalForm(_driver);
            return page;
        }
    }

    public class ApproveOfflineWithdrawalForm : BackendPageBase
    {
        public ApproveOfflineWithdrawalForm(IWebDriver driver) : base(driver) { }

        public SubmittedApproveOfflineWithdrawRequestForm Submit(string remarks)
        {
            var remarksField = _driver.FindElementWait(By.XPath("//textarea[contains(@id, 'withdrawal-approval-remarks')]"));
            remarksField.SendKeys(remarks);
            var approveButton = _driver.FindElementScroll(By.XPath("//button[text()='Approve']"));
            approveButton.Click();
            var form = new SubmittedApproveOfflineWithdrawRequestForm(_driver);
            return form;
        }
    }

    public class SubmittedApproveOfflineWithdrawRequestForm : BackendPageBase
    {
        public SubmittedApproveOfflineWithdrawRequestForm(IWebDriver driver) : base(driver) { }

        public string ConfirmationMessage
        {
            get
            {
                return _driver.FindElementValue(By.XPath("//div[@class='alert alert-success']"));
            }
        }
    }
}