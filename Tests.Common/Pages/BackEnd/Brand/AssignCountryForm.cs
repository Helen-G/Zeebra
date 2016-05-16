using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class AssignCountryForm : BackendPageBase
    {
        public AssignCountryForm(IWebDriver driver) : base(driver)
        {
        }

        private const string FormXPath =
            "//div[contains(@class, 'tab-content') and not(contains(@style, 'display: none'))]" +
            "/div/div[@data-view='brand/country-manager/assign']";


        public SubmittedAssignCountryForm Submit(string licensee, string brand, string country)
        {
            SelectLicenseeBrand(By.XPath("//label[contains(@for, 'brand-country-licensee')]"),
                By.XPath("//select[contains(@id, 'brand-country-licensee')]"), licensee,
                By.XPath("//select[contains(@id, 'brand-country-brand')]"), brand);
            var countriesList =
                _driver.FindElementWait(By.XPath("//select[contains(@data-bind, 'options: availableItems')]"));
            var countriesField = new SelectElement(countriesList);
            countriesField.SelectByText(country);
            var assignButton = _driver.FindElementWait(By.XPath(FormXPath + "//button[contains(@data-bind, 'click: assign')]"));
            assignButton.Click();
            var saveButton = _driver.FindElementWait(By.XPath(FormXPath + "//button[text()='Save']"));
            saveButton.Click();
            var submittedForm = new SubmittedAssignCountryForm(_driver);
            return submittedForm;
        }
    }
}