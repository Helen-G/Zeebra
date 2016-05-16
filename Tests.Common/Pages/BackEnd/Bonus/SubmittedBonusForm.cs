using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd.Bonus
{
    public class SubmittedBonusForm : BackendPageBase
    {
        public SubmittedBonusForm(IWebDriver driver) : base(driver) {}

        public string ConfirmationMessageAfterBonusSaving
        {
            get
            {
                return _driver.FindElementValue(By.XPath("//form[@data-view='bonus/bonus-manager/view-bonus']/div[1]"));
            }
        }

        public string ConfirmationMessageAfterBonusEditing
        {
            get
            {
                return _driver.FindElementValue(By.XPath("//form[@data-view='bonus/bonus-manager/view-bonus']/div[2]"));
            }
        }

        public string BonusName
        {
            get
            {
                return _driver.FindElementValue(By.XPath("//p[@data-bind='text: Name']"));
            }
        }

        public string BonusCode
        {
            get
            {
                return _driver.FindElementValue(By.XPath("//p[@data-bind='text: vCode']"));
            }
        }

        public string BonusTemplate
        {
            get
            {
                return _driver.FindElementValue(By.XPath("//p[@data-bind='text: vTemplateName']"));
            }
        }

        public BonusManagerPage SwitchToBonusList()
        {
            var brandsTab = _driver.FindElementWait(By.XPath("//span[text()='Bonuses']"));
            brandsTab.Click();
            return new BonusManagerPage(_driver);
        }
    }
}