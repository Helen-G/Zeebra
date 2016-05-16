using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class PlayerBankAccountVerifyPage : BackendPageBase
    {
        public PlayerBankAccountVerifyPage(IWebDriver driver) : base(driver) { }

        public Grid Grid
        {
            get
            {
                return new Grid(_driver, "player-bank-accounts-pending-name-search", "player-bank-accounts-pending-search-button");
            }
        }

        private const string BaseXpath = "//div[@data-view='payments/player-bank-accounts/pending-list']";
        private const string BaseDialogXpath = "//div[@data-view='payments/player-bank-accounts/status-dialog']";
        
        public static By VerifyButtonXpath = By.XPath(BaseXpath + "//button[contains(@data-bind, 'click: openVerifyDialog')]");
        public static By RemarksXpath = By.XPath(BaseDialogXpath + "//textarea[contains(@data-bind, 'value: remarks')]");                
        public static By ConfirmButtonXpath = By.XPath(BaseDialogXpath + "//button[contains(@data-bind, 'click: changeStatus')]");
        public static By CloseButtonXpath = By.XPath(BaseDialogXpath + "//button[contains(@data-bind, 'click: close')]");

        public void Verify(string bankAccountName, string remarks = null)
        {
            Grid.SelectRecord(bankAccountName);
            var verifyButton = _driver.FindElementWait(VerifyButtonXpath);
            verifyButton.Click();

            if (!string.IsNullOrEmpty(remarks))
            {
                var remarksField = _driver.FindElementWait(RemarksXpath);
                remarksField.SendKeys(remarks);
            }

            var confirmButton = _driver.FindElementWait(ConfirmButtonXpath);
            confirmButton.Click();

            var closeButton = _driver.FindElementWait(CloseButtonXpath);
            closeButton.Click();
        }
    }
}
