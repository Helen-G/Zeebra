using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd.Bonus
{
    public class DeleteBonusTemplateDialog : BackendPageBase
    {
        public DeleteBonusTemplateDialog(IWebDriver driver) : base(driver){}

        public BonusTemplateManagerPage Confirm()
        {
            var yesButton = _driver.FindElementWait(By.XPath("//button[text()='Yes']"));
            yesButton.Click();
            var closeButton = _driver.FindElementWait(By.XPath("//button[text()='Close']"));
            closeButton.Click();
            return new BonusTemplateManagerPage(_driver);
        }
    }
}