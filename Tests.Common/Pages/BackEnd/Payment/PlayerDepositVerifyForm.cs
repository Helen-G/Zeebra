using System;
using System.Linq;
using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;
using OpenQA.Selenium.Support.UI;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class PlayerDepositVerifyPage : BackendPageBase
    {
        public PlayerDepositVerifyPage(IWebDriver driver) : base(driver)
        {
        }

        public string Title
        {
            get { return _driver.FindElementValue(By.XPath("//div[contains(@class, 'page-header') and not(contains(@style, 'display: none'))]/h5")); }
        }

        public static By VerifyButton = By.Id("deposit-verify-button");
        public static By UnverifyButton = By.Id("deposit-verify-unverify-button");

        public bool HasRecordByReferenceCode(string referenceCode)
        {
            var xpath = string.Format("//div[@id='deposit-verify-grid']//td[text()=\"{0}\"]", referenceCode);
            return _driver.FindElements(By.XPath(xpath)).Count == 1;
        }

        public void SelectConfirmedDeposit(string referenceCode)
        {
            var xpath = string.Format("//div[@id='deposit-verify-grid']//td[text()='{0}']", referenceCode);
            var record = _driver.FindElementWait(By.XPath(xpath));
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
            wait.Until(x => record.Displayed);
            record.Click();
        }

        public string SelectConfirmedDepositRequest(string username)
        {
            var userName = FilterGrid(username);
            var depositToConfirm = _driver.FindElementWait(By.XPath(userName));
            depositToConfirm.Click();
            return depositToConfirm.Text;
        }

        public string FilterGrid(string username)
        {
            var searchBox = _driver.FindElementWait(By.Id("deposit-verify-username-search"));
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
            wait.Until(x => searchBox.Displayed);
            searchBox.SendKeys(username);
            searchBox.SendKeys(Keys.Enter);
            _driver.WaitForJavaScript();
            return string.Format("//td[text() =\"{0}\"]", username);
        }


        public VerifyOfflineDepositForm OpenVerifyForm()
        {
            var verifyButton = _driver.FindElementWait(By.Id("deposit-verify-button"));
            verifyButton.Click();
            var tab = new VerifyOfflineDepositForm(_driver);
            return tab;
        }


        //public VerifyOfflineDepositForm OpenVerifyForm()
        //{
        //    _verifyButton.Click();
        //    _driver.WaitForJavaScript();
        //    var tab = new VerifyOfflineDepositForm(_driver);
        //    tab.Initialize();
        //    return tab;
        //}

        public bool HasSearchButton()
        {
            return _driver.FindElementWait(By.XPath("//button[text()='Search']")).Displayed;
        }

        public void SwitchToListTab()
        {
            _driver.FindElementWait(By.XPath("//div[contains(@class, 'nav-tabs-documents')]//ul[contains(@class, 'nav-tabs')]/li/a/span[text()='List']")).Click();
        }

        public UnverifyOfflineDepositForm OpenUnverifyForm()
        {
            var rejectButton = _driver.FindElementWait(By.Id("deposit-verify-unverify-button"));
            rejectButton.Click();
            var tab = new UnverifyOfflineDepositForm(_driver);
            tab.Initialize();
            return tab;
        }

        public bool CheckIfConfirmButtonDisplayed()
        {
            var unverifyButton = By.Id("deposit-verify-unverify-button");
            var result = _driver.FindElements(unverifyButton).Count(x => x.Displayed && x.Enabled) != 0;
            return result;
        }
//#pragma warning disable 649
//        [FindsBy(How = How.Id, Using = "deposit-verify-button")]
//        private IWebElement _verifyButton;
//#pragma warning restore 649
    }
}