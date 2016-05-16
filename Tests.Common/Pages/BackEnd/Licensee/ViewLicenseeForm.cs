using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class ViewLicenseeForm : BackendPageBase
    {
        public ViewLicenseeForm(IWebDriver driver) : base(driver)
        {
        }

        private const string BaseXPath = "//div[@data-view='licensee-manager/view-licensee']";

        public string ConfirmationMessage
        {
            get { return _driver.FindElementValue(By.XPath("//div[@class='alert alert-success']")); }
        }

        public string Licensee
        {
            get { return _driver.FindElementValue(By.XPath(BaseXPath + "//p[@data-bind='text: name']")); }
        }

        public string CompanyName
        {
            get
            {
                return _driver.FindElementValue(By.XPath(BaseXPath + "//p[@data-bind='text: companyName']"));
            }
        }

        public string ContractStartDate
        {
            get
            {
                return _driver.FindElementValue(By.XPath("//p[@data-bind='text: contractStartDate']"));

            }
        }

        public string ContractEndDate
        {
            get
            {
                return _driver.FindElementValue(By.XPath("//p[@data-bind='text: contractEndDate']"));

            }
        }

        public string Email
        {
            get
            {
                return _driver.FindElementValue(By.XPath(BaseXPath + "//p[@data-bind='text: email']"));
            }
        }

        public string ContractHistoryContractStartDate
        {
            get
            {
                return _driver.FindElementValue(By.XPath(BaseXPath + "//td[@aria-describedby='view-licensee-0-list_startDate']"));
            }
        }

        public string NumberOfAllowedBrands
        {
            get
            {
                return _driver.FindElementValue(By.XPath(BaseXPath + "//p[@data-bind='text: allowedBrands']"));
            }
        }

        public string NumberOfAllowedWebsites
        {
            get
            {
                return _driver.FindElementValue(By.XPath(BaseXPath + "//p[@data-bind='text: allowedWebsites']"));
            }
        }

        public string AssignedProduct
        {
            get
            {
                return _driver.FindElementValue(By.XPath(BaseXPath + "//label[text()='Assigned Products']/following-sibling::div/select/option[1]"));
            }
        }

        public string AssignedCurrencies
        {
            get
            {
                return _driver.FindElementValue(By.XPath(BaseXPath + "//label[text()='Assigned Currencies']/following-sibling::div/select/option[1]"));
            }
        }

        public string AssignedCountries
        {
            get
            {
                return _driver.FindElementValue(By.XPath(BaseXPath + "//label[text()='Assigned Countries']/following-sibling::div/select/option[1]"));
            }
        }

        public string AssignedLanguages
        {
            get
            {
                return _driver.FindElementValue(By.XPath(BaseXPath + "//label[text()='Assigned Languages']/following-sibling::div/select/option[1]"));
            }
        }


        public object Status
        {
            get
            {
                return _driver.FindElementValue(By.XPath(BaseXPath + "//td[@aria-describedby='view-licensee-0-list_isCurrentContract']"));
            }
        }

        public string GetStatus(string date)
        {
            var xpath =
                string.Format(
                    "//table[contains(@id, 'view-licensee')]//tr/td[contains(@title, '{0}')]/following-sibling::td[2]", date);
            var value = _driver.FindElementValue(By.XPath(xpath));
            return value;
        }
    }
}