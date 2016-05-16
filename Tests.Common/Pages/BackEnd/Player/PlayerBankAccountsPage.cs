using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class PlayerBankAccountsPage : BackendPageBase
    {
        public PlayerBankAccountsPage(IWebDriver driver) : base(driver) { }

        public PlayerBankAccountForm OpenNewPlayerBankAccountForm()
        {
            var newButton = _driver.FindElementWait(By.XPath("//*[starts-with(@id, 'player-bank-accounts-') and contains(@id, '-list-jqgrid-title-bar')]/div/button[1]"));
            newButton.Click();
            var form = new PlayerBankAccountForm(_driver);
            form.Initialize();
            return form;
        }
    }
}
