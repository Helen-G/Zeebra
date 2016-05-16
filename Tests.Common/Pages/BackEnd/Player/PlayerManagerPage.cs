using System;
using System.Linq;
using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;
using OpenQA.Selenium.Support.UI;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class PlayerManagerPage : BackendPageBase
    {
        public static readonly By NewButton = By.Id("btn-player-add");
        public static readonly By PlayerInfoButton = By.Id("btn-player-info");
        public static readonly By OfflineWithdrawRequestButton = By.Id("btn-withdraw-request");

        public PlayerManagerPage(IWebDriver driver) : base(driver) {}

        public Grid Grid
        {
            get
            {
                return new Grid(_driver, "player-username-search", "player-search-button");
            }
        }

        public string FilterGrid(string username)
        {
            var searchBox = _driver.FindElementWait(By.CssSelector("#player-username-search"));
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
            wait.Until(x => searchBox.Displayed); 
            searchBox.Clear();
            searchBox.SendKeys(username);
            var searchButton = _driver.FindElementWait(By.CssSelector("#player-search-button"));
            searchButton.Click();
            _driver.WaitForJavaScript();
            return string.Format("//td[text() =\"{0}\"]", username);
        }

        public void SelectPlayer(string username)
        {
            var userRecord = FilterGrid(username);
            var firstCell = _driver.FindElementWait(By.XPath(userRecord));
            firstCell.Click();
        }

        public OfflineDepositRequestForm OpenOfflineDepositRequestForm()
        {
            _depositRequestButton.Click();
            Initialize();
            var tab = new OfflineDepositRequestForm(_driver);
            tab.Initialize();
            return tab;
        }

        public bool CheckIfDepositRequestButtonEnabled()
        {
            var buttonStatus = _driver.FindElementWait(By.XPath("//button[text()='Search']")).Enabled;
            return buttonStatus;
        }

        public PlayerInfoPage OpenPlayerInfoPage()
        {
            var playersTab = _driver.FindElementWait(By.XPath("//ul[@data-view='layout/document-container/tabs']//span[text()='Players']"));
            playersTab.Click();
            _playerInfoButton.Click();
            var playerInforPage = new PlayerInfoPage(_driver);
            _driver.WaitForJavaScript();
            return playerInforPage;
        }

        public PlayerInfoPage OpenPlayerInfoPage(string playerName)
        {
            SelectPlayer(playerName);
            var playersTab = _driver.FindElementWait(By.XPath("//ul[@data-view='layout/document-container/tabs']//span[text()='Players']"));
            playersTab.Click();
            _playerInfoButton.Click();
            var playerInforPage = new PlayerInfoPage(_driver);
            _driver.WaitForJavaScript();
            return playerInforPage;
        }

#region player profile
#pragma warning disable 649
#pragma warning disable 169
        [FindsBy(How = How.Id, Using = "btn-deposit-request")]
        private IWebElement _depositRequestButton;

        [FindsBy(How = How.Id, Using = "btn-player-info")]
        private IWebElement _playerInfoButton;

        [FindsBy(How = How.Id, Using = "btn-player-add")]
        private IWebElement _newButton;

        [FindsBy(How = How.Id, Using = "btn-player-bank-accounts")]
        private IWebElement _playerBankAccountsButton;

        [FindsBy(How = How.Id, Using = "btn-withdraw-request")]
        private IWebElement _withdrawRequestButton;

        [FindsBy(How = How.Id, Using = "btn-fund-in")]
        private IWebElement _fundInMockButton;
        
#pragma warning restore 169
#pragma warning restore 649
#endregion

        public NewPlayerForm OpenNewPlayerForm()
        {
            var newButton = _driver.FindElementWait(By.Id("btn-player-add"));
            newButton.Click();
            var form = new NewPlayerForm(_driver);
            form.Initialize();
            return form;
        }

        public IWebElement FindDepositRequestButton()
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(30));
            IWebElement element = null;
            wait.Until(d =>
            {
                element = _driver.FindElements(By.Id("btn-deposit-request")).FirstOrDefault();
                return element != null;
            });
            return element;
        }

        public OfflineWithdrawRequestForm OpenOfflineWithdrawRequestForm(string username)
        {
            Grid.SelectRecord(username);
            _withdrawRequestButton.Click();
            var form = new OfflineWithdrawRequestForm(_driver);
            form.Initialize();
            return form;
        }

        public string Status
        {
            get
            {
                return _driver.FindElementValue(By.XPath("//td[contains(@aria-describedby, '_Status')]"));
            }
        }
    }
}