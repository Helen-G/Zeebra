using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd.Bonus.BonusTemplateWizard
{
    public class AvailabilityPage : TemplateWizardPageBase
    {
        public AvailabilityPage(IWebDriver driver) : base(driver)
        {
            _driver.FindElementWait(By.XPath(BaseXPath + "//div[@class='template-wizard']/ul/li[2][@class='active']"));
        }

        public RulesPage Next()
        {
            _nextBtn.Click();

            return new RulesPage(_driver);
        }

        public AvailabilityPage LimitMaxNumberOfRedemptions(int limit)
        {
            var bonusTemplateNameField = _driver.FindElementWait(By.XPath(BaseXPath + "//input[contains(@data-bind, 'value: vRedemptionsLimit')]"));
            bonusTemplateNameField.SendKeys(limit.ToString());

            return this;
        }

        public AvailabilityPage LimitNumberOfRedempltionsPerPlayer(int limit)
        {
            var bonusAmountField = _driver.FindElementWait(By.XPath(BaseXPath + "//input[contains(@data-bind, 'value: vPlayerRedemptionsLimit')]"));
            bonusAmountField.SendKeys(limit.ToString());

            return this;
        }

        public AvailabilityPage SelectParentBonus(string parentBonusName)
        {
            var parentBonusField = _driver.FindElementWait(By.XPath(BaseXPath + "//select[contains(@data-bind, 'value: ParentBonusId')]"));
            new SelectElement(parentBonusField).SelectByText(parentBonusName);

            return this;
        }

        public AvailabilityPage ExcludeBonus(string excludeBonusName)
        {
            var widget = new MultiSelectWidget(_driver, By.XPath(BaseXPath + "//div[contains(@data-bind, 'items: ExcludeBonuses')]"));
            widget.SelectFromMultiSelect(excludeBonusName);

            return this;
        }

        public AvailabilityPage ExcludeRiskLevel(string excludeRiskLevelName)
        {
            var widget = new MultiSelectWidget(_driver, By.XPath(BaseXPath + "//div[contains(@data-bind, 'items: ExcludeRiskLevels')]"));
            widget.SelectFromMultiSelect(excludeRiskLevelName);

            return this;
        }

        public AvailabilityPage DeselectVipLevel(string vipLevel)
        {
            var widget = new MultiSelectWidget(_driver, By.XPath(BaseXPath + "//div[contains(@data-bind, 'items: vVipLevels')]"));
            widget.DeselectFromMultiSelect(vipLevel);

            return this;
        }
    }
}