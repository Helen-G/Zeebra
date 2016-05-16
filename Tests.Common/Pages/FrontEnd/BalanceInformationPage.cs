using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.FrontEnd
{
    public class BalanceDetailsPage : FrontendPageBase
    {
        public BalanceDetailsPage(IWebDriver driver) : base(driver) { }

        public string GetBonusBalance
        {
            get
            {
                return
                    _driver.FindElementValue(
                        By.XPath("//tr/td[text()='Bonus Balance:']/following-sibling::td/div/div/span"));
            }
        }


        public string GetMainBalance
        {
            get
            {
                return
                    _driver.FindElementValue(
                        By.XPath("//tr/td[text()='Main Balance:']/following-sibling::td/div/div/span"));
            }
        }


        public BalanceDetailsPage OpenFundTransferSection()
        {
            _driver.Url += "#fundIn";

            return this;
        }

        
    }
}
