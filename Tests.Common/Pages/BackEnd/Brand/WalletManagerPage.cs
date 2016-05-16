using AFT.RegoV2.Tests.Common.Extensions;
using AFT.RegoV2.Tests.Common.Pages;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Tests.Selenium.Pages.BackEnd.Bonus
{
    public class WalletManagerPage : BackendPageBase
    {
        protected const string GridXPath = "//div[contains(@data-view, 'wallet/manager/list')]";

        public WalletManagerPage(IWebDriver driver)
            : base(driver)
        {

        }

        public AddEditWalletForm OpenNewWalletForm()
        {
            var newButton = FindActionButton("new", GridXPath);
            newButton.Click();
            return new AddEditWalletForm(_driver);
        }
    }
}