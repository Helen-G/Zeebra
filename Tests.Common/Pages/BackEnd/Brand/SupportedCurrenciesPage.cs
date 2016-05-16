using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class SupportedCurrenciesPage : BackendPageBase
    {
        public SupportedCurrenciesPage(IWebDriver driver) : base(driver) {}

        public string Title
        {
            get { return _driver.FindElementWait(By.XPath("//h5[text()='Supported Currencies']")).Text; } 
        }

        public AssignCurrencyForm OpenAssignCurrencyForm()
        {
            var assignCurrencyButton = _driver.FindElementWait(By.Id("btn-currency-assign"));
            assignCurrencyButton.Click();
            var form = new AssignCurrencyForm(_driver);
            return form;
        }
    }
}