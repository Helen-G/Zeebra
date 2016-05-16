using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class UnverifyOfflineWithdrawRequestForm : BackendPageBase
    {
        public UnverifyOfflineWithdrawRequestForm(IWebDriver driver) : base(driver) {}

        public SubmittedVerifyOfflineWithdrawRequestForm Submit(string remarks)
        {
            var remarksField = _driver.FindElementScroll(By.XPath("//textarea[contains(@id, 'withdrawal-verification-remarks')]"));
            remarksField.SendKeys(remarks);
            var submitButton = _driver.FindElementScroll(By.XPath("//button[text()='Unverify']"));
            submitButton.Click();
            var form = new SubmittedVerifyOfflineWithdrawRequestForm(_driver);
            form.Initialize();
            return form;
        }
    }
}