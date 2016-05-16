using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.FrontEnd
{
    public class ClaimBonusPage : FrontendPageBase
    {
        public ClaimBonusPage(IWebDriver driver) : base(driver) { }

        public static By ClaimButton = By.XPath("//button[contains(@data-bind, 'click: $root.claimRedemption')]");

        public string Message
        {
            get { return _driver.FindElementWait(By.XPath("//label[@data-bind='text: $data']")).Text; }
        }

        public void ClaimBonus()
        {
            var climButton = _driver.FindElementWait(By.XPath("//button[contains(@data-bind, 'click: $root.claimRedemption')]"));
            climButton.Click();
        }
    }
}