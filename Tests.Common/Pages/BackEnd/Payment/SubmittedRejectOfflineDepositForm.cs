using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class SubmittedRejectOfflineDepositForm : BackendPageBase
    {
        public SubmittedRejectOfflineDepositForm(IWebDriver driver) : base(driver)
        {
        }

        public string Status
        {
            get { return _driver.FindElementValue(By.XPath("//label[text()='Status']/following-sibling::div/p[text()='Rejected']")); } 
            
        }
    }
}