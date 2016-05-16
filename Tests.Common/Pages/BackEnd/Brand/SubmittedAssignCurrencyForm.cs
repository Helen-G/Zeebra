using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class SubmittedAssignCurrencyForm : BackendPageBase
    {
        public SubmittedAssignCurrencyForm(IWebDriver driver) : base(driver) {}

        public const string FormXPath =
            "//div[contains(@class, 'tab-content') and not(contains(@style, 'display: none'))]" +
            "/div/div[@data-view='currency-manager/assign']";
        
        public string ConfirmationMessage
        {
            get
            {
                return
                    _driver.FindElementValue(By.XPath("//div[@data-view='brand/currency-manager/assign']/div"));
            }
        }

        public string Currency
        {
            get
            {
                return
                    _driver.FindElementValue(By.XPath("//div[@data-bind='with: form.fields.defaultCurrency']/p"));
            }
        }
    }
}