using AFT.RegoV2.Core.Bonus.Data.ViewModels;
using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd.Bonus.BonusTemplateWizard
{
    public class InfoPage : TemplateWizardPageBase
    {
        public InfoPage(IWebDriver driver) : base(driver)
        {
            _driver.FindElementWait(By.XPath(BaseXPath + "//div[@class='template-wizard']/ul/li[1][@class='active']"));
        }

        public AvailabilityPage Next()
        {
            _nextBtn.Click();

            return new AvailabilityPage(_driver);
        }

        public InfoPage SelectLicenseeAndBrand(string licensee, string brand)
        {
            SelectLicenseeBrand(By.XPath(BaseXPath + "//label[text()='Licensee']"),
                By.XPath("//select[contains(@data-bind, 'options: availableLicensees')]"), licensee,
                By.XPath("//select[contains(@data-bind, 'options: availableBrands')]"), brand);

            return this;
        }

        public InfoPage SelectBonusType(BonusType type)
        {
            var typeField = _driver.FindElementWait(By.XPath(BaseXPath + "//select[contains(@data-bind, 'options: availableTypes')]"));
            new SelectElement(typeField).SelectByIndex((int)type);

            return this;
        }

        public InfoPage MakeWithdrawable()
        {
            var checkBox = _driver.FindElementWait(By.XPath(BaseXPath + "//input[@data-bind='checked: IsWithdrawable.ForEditing' and @value='true']/following-sibling::span"));
            checkBox.Click();

            return this;
        }

        public InfoPage SetTemplateName(string bonusTemplateName)
        {
            var bonusTemplateNameField = _driver.FindElementWait(By.XPath(BaseXPath + "//input[@data-bind='value: Name']"));
            bonusTemplateNameField.SendKeys(Keys.Control + "a");
            bonusTemplateNameField.SendKeys(bonusTemplateName);

            return this;
        }

        public InfoPage SelectIssuanceMode(IssuanceMode mode)
        {
            var typeField = _driver.FindElementWait(By.XPath(BaseXPath + "//select[contains(@data-bind, 'options: availableModes')]"));
            new SelectElement(typeField).SelectByIndex((int)mode);

            return this;
        }

        public RulesPage NavigateToRules()
        {
            return Next().Next();
        }
    }
}