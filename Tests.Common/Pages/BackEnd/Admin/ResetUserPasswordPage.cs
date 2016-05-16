using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class ResetUserPasswordPage : BackendPageBase
    {
        public ResetUserPasswordPage(IWebDriver driver) : base(driver) {}

        public void ResetUserPassword(string newPassword)
        {
            var password = _driver.FindElementWait(By.XPath("//input[@data-bind='value: Model.password']"));
            password.SendKeys(newPassword);
            var passwordConfirmation = _driver.FindElementWait(By.XPath("//input[@data-bind='value: Model.passwordConfirmation']"));
            passwordConfirmation.SendKeys(newPassword);
            var saveButton =
                _driver.FindElementWait(
                    By.XPath("//button[text()='Save']"));
            saveButton.Click();
        }
    }
}