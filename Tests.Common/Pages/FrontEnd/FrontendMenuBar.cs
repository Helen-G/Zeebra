using System.Linq;
using AFT.RegoV2.Tests.Common.Extensions;
using AFT.RegoV2.Tests.Common.Pages.FrontEnd;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages
{
    public class FrontendMenuBar
    {

        protected readonly IWebDriver _driver;

        public FrontendMenuBar(IWebDriver driver)
        {
            _driver = driver;
        }

        public GameListPage ClickPlayGamesMenu()
        {
            var menuItem = _driver.FindElementWait(By.XPath("//a[@href='/en-US/Home/GameList']"));
            menuItem.Click();
            return new GameListPage(_driver);
        }
        
        private ClaimBonusPage ClickClaimBonusMenu()
        {
            var menuItem = _driver.FindElementWait(By.XPath("//a[@href='/en-US/Home/ClaimBonusReward']"));
            menuItem.Click();
            return new ClaimBonusPage(_driver);
        }

        public ReferFriendsPage ClickReferFriendsMenu()
        {
            var menuItem = _driver.FindElementWait(By.XPath("//a[@href='/en-US/Home/ReferAFriend']"));
            menuItem.Click();
            return new ReferFriendsPage(_driver);
        }

        public BalanceDetailsPage ClickBalanceInformationMenu()
        {
            var menuItem = _driver.FindElementWait(By.XPath("//a[@href='/en-US/Home/BalanceInformation']"));
            menuItem.Click();
            return new BalanceDetailsPage(_driver);
        }
        
        public TransferFundPage ClickTransferFundSubMenu()
        {
            var menuItem = By.XPath("//a[@href='#fundIn']");
            if (_driver.FindElements(menuItem).Count(x => x.Displayed && x.Enabled) == 0)
            {
                ClickBalanceInformationMenu();
            }
            var subMenu = _driver.FindElementWait(menuItem);
            subMenu.Click();
            var page = new TransferFundPage(_driver);
            page.Initialize();
            return page;
        }

        public TransferFundPage ClickBalanceDetailsSubMenu()
        {
            var menuItem = By.XPath("//a[@href='#balance']");
            if (_driver.FindElements(menuItem).Count(x => x.Displayed && x.Enabled) == 0)
            {
                ClickBalanceInformationMenu();
            }
            var subMenu = _driver.FindElementWait(menuItem);
            subMenu.Click();
            return new TransferFundPage(_driver);
        }

        public ClaimBonusPage ClickClaimBonusSubMenu()
        {
            var menuItem = By.XPath("//a[@href='#claimBonus']");
            if (_driver.FindElements(menuItem).Count(x => x.Displayed && x.Enabled) == 0)
            {
                ClickClaimBonusMenu();
            }
            var subMenu = _driver.FindElementWait(menuItem);
            subMenu.Click();
            return new ClaimBonusPage(_driver);
        }

        public OfflineDepositRequestPage ClickOfflineDepositSubmenu()
        {
            var menuItem = By.XPath("//a[@href='#offlineDeposit']");
            if (_driver.FindElements(menuItem).Count(x => x.Displayed && x.Enabled) == 0)
            {
                ClickBalanceInformationMenu();
            }
            var subMenu = _driver.FindElementWait(menuItem);
            subMenu.Click();
            return new OfflineDepositRequestPage(_driver);
        }
    }
}