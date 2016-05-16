using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class SubmittedDepositVerifyForm : BackendPageBase
    {
        public SubmittedDepositVerifyForm(IWebDriver driver) : base(driver) {}

        public string ConfirmationMessage
        {
            get
            {
                return _driver.FindElementValue(By.XPath("//div[@data-view='player-manager/offline-deposit/verify']/div"));
            }
        }
        
    }
}