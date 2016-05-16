using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd.Bonus.BonusTemplateWizard
{
    public class TemplateWizardPageBase: BackendPageBase
    {
        protected const string BaseXPath = "//div[@data-view='bonus/template-manager/wizard']";
#pragma warning disable 649
        [FindsBy(How = How.XPath, Using = BaseXPath + "//button[@data-i18n='bonus.templateManager.next']")]
        protected IWebElement _nextBtn { get; set; }
#pragma warning restore 649

        protected TemplateWizardPageBase(IWebDriver driver) : base(driver)
        {
            Initialize();
        }
    }
}