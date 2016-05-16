using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd.Fraud
{
    public class AutoVerificationConfigurationFailure : BackendPageBase
    {
        internal static readonly By ErrorAlertBy = By.XPath("//div[contains(@class,'alert-danger')]");


        public IWebElement ErrorAlert
        {
            get { return _driver.FindElementWait(ErrorAlertBy); }
        }


        public AutoVerificationConfigurationFailure(IWebDriver driver)
            : base(driver)
        { }
    }
}