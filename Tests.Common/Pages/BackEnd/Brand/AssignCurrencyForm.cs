using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class AssignCurrencyForm : BackendPageBase
    {
        public AssignCurrencyForm(IWebDriver driver) : base(driver)
        {
        }

        public const string FormXPath =
            "//div[contains(@class, 'tab-content') and not(contains(@style, 'display: none'))]" +
            "/div/div[@data-view='brand/currency-manager/assign']";

        public SubmittedAssignCurrencyForm Submit(string licensee, string brandName, string currency)
        {
            SelectLicenseeBrand(By.XPath(FormXPath + "//label[contains(@for, 'brand-currency-licensee')]"),
                By.XPath(FormXPath + "//select[contains(@id, 'brand-currency-licensee')]"), licensee,
                By.XPath(FormXPath + "//select[contains(@id, 'brand-currency-brand')]"), brandName);

            var currencyList = _driver.FindElementWait(By.XPath(FormXPath + "//select[contains(@data-bind, 'options: availableItems')]"));
            var currencyValue = new SelectElement(currencyList);
            currencyValue.SelectByText(currency);
            _driver.ScrollPage(0, 300);
            var assignButton = _driver.FindElementWait(By.XPath(FormXPath + "//button[contains(@data-bind, 'click: assign')]"));
            assignButton.Click();
            var saveButton = _driver.FindElementWait(By.XPath(FormXPath + "//button[text()='Save']"));
            saveButton.Click();
            var submittedForm = new SubmittedAssignCurrencyForm(_driver);
            return submittedForm;
        }
    }
}