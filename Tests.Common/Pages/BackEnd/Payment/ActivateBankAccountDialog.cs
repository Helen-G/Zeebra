using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class ActivateBankAccountDialog : BackendPageBase
    {
        public ActivateBankAccountDialog(IWebDriver driver) : base(driver)
        {
        }

        public SubmittedActivateBankAccountDialog ActivateBankAccount(string remark)
        {
            var remarkField = _driver.FindElementWait(By.XPath("//textarea[@data-bind='value: remarks']"));
            remarkField.SendKeys(remark);
            var activateButton = _driver.FindElementWait(By.XPath("//button[text()='Activate']"));
            activateButton.Click();
            var submittedDialog = new SubmittedActivateBankAccountDialog(_driver);
            return submittedDialog;
        }

        }
}