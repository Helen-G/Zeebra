using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd.Bonus.BonusTemplateWizard
{
    public class SummaryPage : TemplateWizardPageBase
    {
        public SummaryPage(IWebDriver driver) : base(driver)
        {
            _driver.FindElementWait(By.XPath(BaseXPath + "//div[@class='template-wizard']/ul/li[6][contains(@class, 'active')]"));
        }

#pragma warning disable 649
        [FindsBy(How = How.XPath, Using = BaseXPath + "//button[contains(@data-bind, 'click: close')]")]
        private IWebElement _closeTabBtn { get; set; }
        [FindsBy(How = How.XPath, Using = BaseXPath + "//p[@data-bind='text: Info.Name']")]
        private IWebElement _nameLabel { get; set; }
#pragma warning restore 649

        public string Name
        {
            get { return _nameLabel.Text; }
        }

        public string Brand
        {
            get { return _driver.FindElementValue(By.XPath(BaseXPath + "//p[@data-bind='text: BrandName']")); }
        }

        public BonusTemplateManagerPage CloseTab()
        {
            _closeTabBtn.Click();

            return new BonusTemplateManagerPage(_driver);
        }
    }
}