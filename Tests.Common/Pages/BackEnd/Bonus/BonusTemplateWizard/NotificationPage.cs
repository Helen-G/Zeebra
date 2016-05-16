using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd.Bonus.BonusTemplateWizard
{
    public class NotificationPage : TemplateWizardPageBase
    {
        public NotificationPage(IWebDriver driver) : base(driver)
        {
            _driver.FindElementWait(By.XPath(BaseXPath + "//div[@class='template-wizard']/ul/li[5][@class='active']"));
        }

#pragma warning disable 649
        [FindsBy(How = How.XPath, Using = BaseXPath + "//select[contains(@data-bind, 'options: emailTemplates'")]
        private IWebElement _emailSelect { get; set; }
#pragma warning restore 649

        public SummaryPage Next()
        {
            _nextBtn.Click();

            return new SummaryPage(_driver);
        }
    }
}