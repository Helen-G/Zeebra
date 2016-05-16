using System.Linq;
using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class NewBankForm : BackendPageBase
    {
        public NewBankForm(IWebDriver driver) : base(driver) { }

        public const string FormXPath = "//li[contains(@class, 'active') and not(contains(@class, 'inactive'))]";

        public string Title
        {
            get
            {
                return _driver.FindElementWait(By.XPath(FormXPath + "//span[text()='New Bank']")).Text;
            }
        }

        private void SelectCountry(string countryName)
        {
            var countryBy = By.XPath("//select[contains(@id, 'bank-country')]");
            if (_driver.FindElements(countryBy).Count(x => x.Displayed && x.Enabled) > 0)
            {
                var countryList = _driver.FindElementWait(countryBy);
                var countryField = new SelectElement(countryList);
                countryField.SelectByText(countryName);
            }
        }

        public SubmittedBankForm Submit(string brand, string bankID, string bankName, string countryName, string remarks)
        {
            SelectLicenseeBrand(By.XPath("//label[contains(@for, 'bank-licensee')]"), 
                By.XPath("//select[contains(@id, 'bank-licensee')]"), "Flycow", By.XPath("//select[contains(@id, 'bank-brand')]"), brand);

            var bankIDField = _driver.FindElementWait(By.XPath("//input[contains(@id, 'bank-id')]"));
            bankIDField.SendKeys(bankID);
            var bankNameField = _driver.FindElementWait(By.XPath("//input[contains(@id, 'bank-name')]"));
            bankNameField.SendKeys(bankName);
            SelectCountry(countryName);
            var remarksField = _driver.FindElementWait(By.XPath("//textarea[contains(@id, 'bank-remark')]"));
            remarksField.SendKeys("new bank");
            ClickSaveButton();
            var form = new SubmittedBankForm(_driver);
            return form;
        }

        public SubmittedBankForm SubmitWithLicensee(string licensee, string brand, string bankID, string bankName, string countryName, string remarks)
        {
            SelectLicenseeBrand(By.XPath("//label[contains(@for, 'bank-licensee')]"), 
                By.XPath("//select[contains(@id, 'bank-licensee')]"), licensee, By.XPath("//select[contains(@id, 'bank-brand')]"), brand);

            var bankIDField = _driver.FindElementWait(By.XPath("//input[contains(@id, 'bank-id')]"));
            bankIDField.SendKeys(bankID);
            var bankNameField = _driver.FindElementWait(By.XPath("//input[contains(@id, 'bank-name')]"));
            bankNameField.SendKeys(bankName);
            SelectCountry(countryName);
            var RemarksField = _driver.FindElementWait(By.XPath("//textarea[contains(@id, 'bank-remark')]"));
            RemarksField.SendKeys("new bank");
            ClickSaveButton();
            var form = new SubmittedBankForm(_driver);
            return form;
        }

        public void ClickSaveButton()
        {
            var saveButton = _driver.FindElementWait(By.XPath("//button[text()='Save']"));
            saveButton.Click();
        }
    }
}