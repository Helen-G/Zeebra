using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class VerifyOfflineWithdrawRequestForm : BackendPageBase
    {
        public VerifyOfflineWithdrawRequestForm(IWebDriver driver) : base(driver) { }

        public SubmittedVerifyOfflineWithdrawRequestForm Submit(string remarks)
        {
            var remarksField = _driver.FindElementWait(By.XPath("//textarea[contains(@id, 'withdrawal-verification-remarks')]"));
            remarksField.SendKeys(remarks);
            var submitButton = _driver.FindElementWait(By.XPath("//button[text()='Submit']"));
            submitButton.Click();
            _driver.ScrollPage(0,0);
            var form = new SubmittedVerifyOfflineWithdrawRequestForm(_driver);
            form.Initialize();
            return form;
        }
    }
}