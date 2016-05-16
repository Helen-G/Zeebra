using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.FrontEnd
{
    public class GameListPage : FrontendPageBase
    {
        public GameListPage(IWebDriver driver) : base(driver) {}

        public GamePage StartGame(string game)
        {
            var xpath = string.Format("//li[h5[text()='{0}']]/a", game);
            var gameLink = _driver.FindElementWait(By.XPath(xpath));
            gameLink.Click();
            var page = new GamePage(_driver);
            return page;
        }
    }

    public class GamePage : FrontendPageBase
    {
        public GamePage(IWebDriver driver) : base(driver) {}


        public string Balance
        {
            get { return _driver.FindElementValue(By.XPath("//p[contains(., 'Balance')]")); }
        }

        public string PlayerName
        {
            get { return _driver.FindElementValue(By.XPath("//p[contains(., 'Name:')]")); }
        }

        public string BetLimitCode
        {
            get { return _driver.FindElementValue(By.XPath("//span[@id='bet-limit-code']")); }
        }

        public string Tag
        {
            get { return _driver.FindElementValue(By.XPath("//p[contains(., 'Tag:')]")); }
        }

        public string BetAmount
        {
            get { return _driver.FindElementValue(By.XPath("//td[@class='bet-amount']")); }
        }

        public string RoundId
        {
            get { return _driver.FindElementValue(By.XPath("//td[@class='bet-id']")); }
        }

        public string BetStatus
        {
            get { return _driver.FindElementValue(By.XPath("//td[@class='bet-status']")); }
        }

        public void PlaceInitialBet(decimal amount, string description)
        {
            var betAmountField = _driver.FindElementWait(By.Id("Amount"));
            betAmountField.Clear();
            betAmountField.SendKeys(amount.ToString());

            var placeBetButton = _driver.FindElementWait(By.XPath("//input[contains(@id, 'place-bet')]"));
            placeBetButton.Click();
        }

        public void PlaceSubBet(decimal amount, string description)
        {
            var betAmountField = _driver.FindElementWait(By.XPath("//input[@class='add-tx-amount']"));
            betAmountField.Clear();
            betAmountField.SendKeys(amount.ToString());

            var placeSubbetButton = _driver.FindElementWait(By.XPath("//input[contains(@value, 'Place')]"));
            placeSubbetButton.Click();
        }

        public string GetTransactionDetail(int txNumber, string detail)
        {
            return _driver.FindElementValue(By.XPath(
                string.Format("//td[@id='tx-{0}-{1}']", detail, txNumber))); 
        }

        public void WinBet(decimal amount)
        {
            var amountField = _driver.FindElementWait(By.XPath("//input[@class='add-tx-amount']"));
            amountField.Clear();
            amountField.SendKeys(amount.ToString());

           var winBetButton = _driver.FindElementWait(By.XPath("//input[contains(@value, 'Win')]"));
           winBetButton.Click();
        }

        public void LoseBet(decimal amount)
        {
            var amountField = _driver.FindElementWait(By.XPath("//input[@class='add-tx-amount']"));
            amountField.Clear();
            amountField.SendKeys(amount.ToString());

            var loseBetButton = _driver.FindElementWait(By.XPath("//input[contains(@value, 'Lose')]"));
            loseBetButton.Click();
        }

        public void AdjustTransaction(int txNumber, decimal amount)
        {
            var amountField = _driver.FindElementWait(By.XPath(
                string.Format("//td[@id='tx-cmd-{0}']//input[@class='change-tx-amount']", txNumber)));
            amountField.Clear();
            amountField.SendKeys(amount.ToString());

            var adjustBetButton = _driver.FindElementWait(By.XPath(string.Format("//input[@id='tx-cmd-adjust-{0}']", txNumber)));
            adjustBetButton.Click();
        }


        public void CancelTransaction(int txNumber, decimal amount)
        {
            var amountField = _driver.FindElementWait(By.XPath(
                string.Format("//td[@id='tx-cmd-{0}']//input[@class='change-tx-amount']", txNumber)));
            amountField.Clear();
            amountField.SendKeys(amount.ToString());

            var cancelBetButton = _driver.FindElementWait(By.XPath(string.Format("//input[@id='tx-cmd-cancel-{0}']", txNumber)));
            cancelBetButton.Click();
        }

    }
}