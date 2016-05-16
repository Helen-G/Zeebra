using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class SubmittedAssignCountryForm : BackendPageBase
    {
        public SubmittedAssignCountryForm(IWebDriver driver) : base(driver)
        {
        }

        public string ConfirmationMessage
        {
            get
            {
                return
                    _driver.FindElementValue(By.XPath("//div[contains(@data-view, 'brand/country-manager/assign')]/div"));
            }
        }
    }
}