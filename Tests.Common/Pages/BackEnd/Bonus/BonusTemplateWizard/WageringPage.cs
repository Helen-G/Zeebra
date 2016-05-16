using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd.Bonus.BonusTemplateWizard
{
    public class WageringPage: TemplateWizardPageBase
    {
        public WageringPage(IWebDriver driver) : base(driver)
        {
            _driver.FindElementWait(By.XPath(BaseXPath + "//div[@class='template-wizard']/ul/li[4][@class='active']"));
        }

        [FindsBy(How = How.XPath, Using = BaseXPath + "//input[@data-bind='checked: HasWagering.ForEditing' and @value='true']/following-sibling::span")]
        private IWebElement _isPostWagerBtn { get; set; }
        [FindsBy(How = How.XPath, Using = BaseXPath + "//input[contains(@data-bind, 'value: vMultiplier')]")]
        private IWebElement _multiplierField { get; set; }
        [FindsBy(How = How.XPath, Using = BaseXPath + "//input[contains(@data-bind, 'value: vThreshold')]")]
        private IWebElement _thresholdField { get; set; }

        public NotificationPage Next()
        {
            _nextBtn.Click();

            return new NotificationPage(_driver);
        }

        public WageringPage MakeAfterWager(decimal wageringCondition, decimal wageringThreshold)
        {
            _isPostWagerBtn.Click();

            _multiplierField.Clear();
            _multiplierField.SendKeys(wageringCondition.ToString());

            _thresholdField.Clear();
            _thresholdField.SendKeys(wageringThreshold.ToString());

            return this;
        }

        public SummaryPage NavigateToSummary()
        {
            return Next().Next();
        }
    }
}