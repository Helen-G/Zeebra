using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class OfflineWithrawRequestsPage : BackendPageBase
    {
        public OfflineWithrawRequestsPage(IWebDriver driver) : base(driver) { }

        public Grid Grid
        {
            get
            {
                return new Grid(_driver, "withdrawal-request-name-search", "withdrawal-request-search-btn");
            }
        }
        
        public static By UnverifyButton = By.Id("btn-withdraw-unverify");
        public static By VerifyButton = By.Id("btn-withdraw-verify");

        public VerifyOfflineWithdrawRequestForm OpenVerifyForm(string username)
        {
            Grid.SelectRecord(username);
            var verifyButton = _driver.FindElementWait(VerifyButton);
            verifyButton.Click();
            var page = new VerifyOfflineWithdrawRequestForm(_driver);
            return page;
        }

        public UnverifyOfflineWithdrawRequestForm OpenUnverifyForm(string username)
        {
            Grid.SelectRecord(username);
            var verifyButton = _driver.FindElementWait(UnverifyButton);
            verifyButton.Click();
            var page = new UnverifyOfflineWithdrawRequestForm(_driver);
            return page;
        }

    }


    public class SubmittedVerifyOfflineWithdrawRequestForm : BackendPageBase
    {
        public SubmittedVerifyOfflineWithdrawRequestForm(IWebDriver driver) : base(driver) { }

        public string ConfirmationMessage
        {
            get
            {
                return _driver.FindElementValue(By.XPath("//div[@class='alert alert-success']"));
            }
        }
    }
}