using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class EditBrandForm : BackendPageBase
    {
        public EditBrandForm(IWebDriver driver) : base(driver) {}

        public SubmittedEditBrandForm EditOnlyRequiredData(string brandType, string brandName, string brandCode, string remarks = "remarks")
        {
            var brandNameField = _driver.FindElementWait(By.XPath("//label[contains(., 'Brand Name')]/following-sibling::div/input"));
            brandNameField.Clear();
            brandNameField.SendKeys(brandName);
            
            var brandCodeField = _driver.FindElementWait(By.XPath("//label[contains(., 'Brand Code')]/following-sibling::div/input"));
            brandCodeField.Clear();
            brandCodeField.SendKeys(brandCode);
            
            var brandTypesField =
                _driver.FindElementWait(By.XPath("//label[contains(., 'Brand Type')]/following-sibling::div/select"));
            var brandTypesList = new SelectElement(brandTypesField);
            brandTypesList.SelectByText(brandType);

            var brandRemarksField = _driver.FindElementWait(By.XPath("//label[contains(., 'Remark')]/following-sibling::div/textarea"));
            brandRemarksField.Clear();
            brandRemarksField.SendKeys(remarks);

            var saveButton = _driver.FindElementWait(By.XPath("//button[text()='Save']"));
            saveButton.Click();
            var form = new SubmittedEditBrandForm(_driver);
            return form;
        }
    }

    public class SubmittedEditBrandForm : BackendPageBase
    {
        public SubmittedEditBrandForm(IWebDriver driver) : base(driver)
        {
        }

        public string ConfirmationMessage
        {
            get { return _driver.FindElementValue(By.XPath("//div[@class[contains(., 'alert-success')]]")); }
        }

        public string BrandNameValue
        {
            get { return _driver.FindElementWait(By.XPath("//p[@data-bind='text: name']")).Text; }
        }

        public string BrandCodeValue
        {
            get { return _driver.FindElementWait(By.XPath("//p[@data-bind='text: code']")).Text; }
        }

        public string BrandTypeValue
        {
            get { return _driver.FindElementWait(By.XPath("//p[@data-bind='text: type']")).Text; }
        }

        public string PlayerPrefix
        {
            get { return _driver.FindElementWait(By.XPath("//p[@data-bind='text: playerPrefix']")).Text; }
        }

        public string LicenseeValue
        {
            get { return _driver.FindElementWait(By.XPath("//p[@data-bind='text: licensee']")).Text; }
        }

        public string RemarksValue
        {
            get { return _driver.FindElementWait(By.XPath("//p[@data-bind='text: remarks']")).Text; }
        }
    }
}