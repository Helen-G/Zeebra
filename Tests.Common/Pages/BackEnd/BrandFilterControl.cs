using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class BrandFilterControl
    {
        private readonly IWebDriver _driver;

        public BrandFilterControl(IWebDriver driver)
        {
            _driver = driver;
        }

        public void SelectAll()
        {
            OpenControl();
            
            if (SelectAllButton.Enabled)
                SelectAllButton.Click();

            CloseControl();
        }

        public void ClearAll()
        {
            OpenControl();

            ClearAllButton.Click();

            CloseControl();
        }

        private void OpenControl()
        {
            if (!SelectAllButton.Displayed)
                ControlTab.Click();
        }

        private void CloseControl()
        {
            if (SelectAllButton.Displayed)
                ControlTab.Click();
        }

        private IWebElement ControlTab
        {
            get { return _driver.FindElementWait(By.Id("navbar-licensee-brand-indicator")); }
        }

        private IWebElement SelectAllButton
        {
            get { return _driver.FindAnyElementWait(By.Id("brand-filter-select-all")); }
        }

        private IWebElement ClearAllButton
        {
            get { return _driver.FindAnyElementWait(By.Id("brand-filter-clear-all")); }
        }
    }
}