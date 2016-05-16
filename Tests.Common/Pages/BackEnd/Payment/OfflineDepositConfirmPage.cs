using System;
using System.Linq;
using System.Threading;
using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;
using OpenQA.Selenium.Support.UI;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class OfflineDepositConfirmPage : BackendPageBase
    {
        public OfflineDepositConfirmPage(IWebDriver driver) : base(driver)
        {
        }

        //public Grid Grid
        //{
        //    get
        //    {
        //        return new Grid(_driver, "offline-deposit-confirm-username-search", "offline-deposit-confirm-username-search-button");
        //    }
        //}

        public ViewOfflineDepositConfirmForm OpenViewOfflineDepositConfirmForm(string username, string referenceCode)
        {
            SelectOfflineDepositRequest(username, referenceCode);
            _viewButton.Click();
            var form = new ViewOfflineDepositConfirmForm(_driver);
            form.Initialize();
            return form;
        }
        
        
        public string SelectOfflineDepositRequest(string username, string referenceCode)
        {
            var refCode = FilterGrid(username, referenceCode);
            var depositToConfirm = _driver.FindElementWait(By.XPath(refCode));
            depositToConfirm.Click();
            return depositToConfirm.Text;
        }

        public string SelectOfflineDepositRequest(string username)
        {
            var userName = FilterGrid(username);
            var depositToConfirm = _driver.FindElementWait(By.XPath(userName));
            depositToConfirm.Click();
            return depositToConfirm.Text;
        }

        public string FilterGrid(string username)
        {
            var searchBox = _driver.FindElementWait(By.Id("offline-deposit-confirm-username-search"));
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
            wait.Until(x => searchBox.Displayed);
            searchBox.SendKeys(username);
            searchBox.SendKeys(Keys.Enter);
            _driver.WaitForJavaScript();
            return string.Format("//td[text() =\"{0}\"]", username);
        }

        public string FilterGrid(string username, string referenceCode)
        {
            var searchBox = _driver.FindElementWait(By.Id("offline-deposit-confirm-username-search"));
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
            wait.Until(x => searchBox.Displayed);
            searchBox.SendKeys(username);
            searchBox.SendKeys(Keys.Enter);
            _driver.WaitForJavaScript();
            return string.Format("//td[text() =\"{0}\"]", referenceCode);
        }

        public bool HasRecordWithReferenceCode(string referenceCode)
        {
            var xpath = string.Format("//div[@id='offline-deposit-confirm-grid']//td[text()=\"{0}\"]", referenceCode);
            return _driver.FindElements(By.XPath(xpath)).Count == 1;
        }

        public ConfirmOfflineDepositForm ClickConfirmButton()
        {
            _confirmButton.Click();
            _driver.WaitForJavaScript();
            var tab = new ConfirmOfflineDepositForm(_driver);
            tab.Initialize();
            return tab;
        }

        public bool CheckIfConfirmButtonDisplayed()
        {
            var confirmButton = By.Id("offline-deposit-confirm-button");
            var result = _driver.FindElements(confirmButton).Count(x => x.Displayed && x.Enabled) != 0;
            return result;
        }

#pragma warning disable 649
        [FindsBy(How = How.Id, Using = "offline-deposit-confirm-button")]
        private IWebElement _confirmButton;

        [FindsBy(How = How.Id, Using = "offline-deposit-confirm-view-button")]
        private IWebElement _viewButton;
#pragma warning restore 649
    }
}
