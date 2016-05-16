using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class SubmittedPlayerForm : BackendPageBase
    {
        public SubmittedPlayerForm(IWebDriver driver) : base(driver)
        {
        }

        public string ConfirmationMessage
        {
            get { return _driver.FindElementValue(By.XPath("//div[@class='alert alert-success']")); }
        }

        public PlayerManagerPage SwitchToPlayersTab()
        {
            var playerManagerTab = _driver.FindElementWait(By.XPath("//ul[@data-view='layout/document-container/tabs']//span[text()='Players']"));
            playerManagerTab.Click();
            return new PlayerManagerPage(_driver);
        }

        public string PaymentLevel
        {
            get
            {
                return
                    _driver.FindElementValue(
                        By.XPath(
                            "//select[contains(@id, 'payment-level')]/following-sibling::p"));
            }
        }
    }
}