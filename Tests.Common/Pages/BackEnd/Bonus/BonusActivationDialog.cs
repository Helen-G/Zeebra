using AFT.RegoV2.Tests.Common.Annotations;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd.Bonus
{
    public sealed class BonusActivationDialog : BackendPageBase
    {
        public BonusActivationDialog(IWebDriver driver) : base(driver)
        {
            Initialize();
        }

        public BonusManagerPage Activate()
        {
            _descriptionArea.SendKeys("valid bonus");
            _activateBtn.Click();
            return new BonusManagerPage(_driver);
        }

        public BonusManagerPage Deactivate()
        {
            _descriptionArea.SendKeys("valid bonus");
            _deactivateBtn.Click();
            return new BonusManagerPage(_driver);
        }

#pragma warning disable 649
        [FindsBy(How = How.XPath, Using = "//div[@data-view='bonus/bonus-manager/activate-dialog']//textarea"), UsedImplicitly]
        private IWebElement _descriptionArea;

        [FindsBy(How = How.XPath, Using = "//div[@data-view='bonus/bonus-manager/activate-dialog']//button[contains(@data-bind, 'visible: isActive')]")]
        private IWebElement _activateBtn;

        [FindsBy(How = How.XPath, Using = "//div[@data-view='bonus/bonus-manager/activate-dialog']//button[contains(@data-bind, 'visible: !isActive')]")]
        private IWebElement _deactivateBtn;
#pragma warning restore 649
    }
}