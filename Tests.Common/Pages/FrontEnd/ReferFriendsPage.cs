using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.FrontEnd
{
    public class ReferFriendsPage : FrontendPageBase
    {
        public ReferFriendsPage(IWebDriver driver) : base(driver) {}

        public void ReferFriend()
        {
            var phoneNumberField = _driver.FindElementWait(By.XPath("//input[@data-bind='value: number']"));
            var phoneNumber = TestDataGenerator.GetRandomPhoneNumber().Replace("-", "");
            phoneNumberField.SendKeys(phoneNumber);

            var submitButton = _driver.FindElementWait(By.XPath("//button[@type='submit']"));
            submitButton.Click();
        }

        public string Message
        {
            get { return _driver.FindElementWait(By.XPath("//label[@data-bind='text: $data']")).Text; }
        }
    }
}
