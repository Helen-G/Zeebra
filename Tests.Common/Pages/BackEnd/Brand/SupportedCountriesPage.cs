using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class SupportedCountriesPage : BackendPageBase
    {
        public SupportedCountriesPage(IWebDriver driver) : base(driver)
        {
        }

        public AssignCountryForm OpenAssignCountriesForm()
        {
            var assignCountryButton = _driver.FindElementWait(By.Id("btn-assign-country"));
            assignCountryButton.Click();
            var form = new AssignCountryForm(_driver);
            return form;
        }
    }
}