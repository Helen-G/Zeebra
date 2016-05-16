using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class DashboardPage : BackendPageBase
    {
        public DashboardPage(IWebDriver driver) : base(driver){}

        public string Username
        {
            get { return _driver.FindElementValue(By.XPath("//span[@data-bind='text: security.userName']")); }
        }
    }
}