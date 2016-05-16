using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class SubmittedBankForm : BackendPageBase
    {
        public SubmittedBankForm(IWebDriver driver) : base(driver) {}

        public string ConfirmationMessage
        {
            get
            {
                return _driver.FindElementValue(By.XPath("//div[@class='alert alert-success']"));
            }
        }

        public string BankNameValue
        {
            get { return _driver.FindElementValue(By.XPath("//p[contains(@data-bind, 'text: fields.bankName')]")); }
        }

        public string LicenseeValue
        {
            get { return _driver.FindElementValue(By.XPath("//div[contains(@data-bind, 'with: form.fields.licensee')]//p[contains(@data-bind, 'text: display')]")); }
        }

        public string BrandValue
        {
            get { return _driver.FindElementValue(By.XPath("//div[contains(@data-bind, 'with: form.fields.brand')]//p[contains(@data-bind, 'text: display')]")); }
        }

        public string BankIdValue
        {
            get
            {
                return _driver.FindElementValue(By.XPath("//p[contains(@data-bind, 'text: fields.bankId')]")); 
            }
        }
    }
}