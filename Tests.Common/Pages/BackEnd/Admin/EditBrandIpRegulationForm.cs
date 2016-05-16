using System.Linq;
using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;
using OpenQA.Selenium.Support.UI;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class EditBrandIpRegulationForm : BackendPageBase
    {
        public EditBrandIpRegulationForm(IWebDriver driver) : base(driver) { }

        public ViewBrandIpRegulationForm Submit(BrandIpRegulationData data)
        {
            var licenseesList = _driver.FindElementWait(By.XPath("//select[contains(@data-bind, 'options: Model.licensees')]"));
            var licenseeField = new SelectElement(licenseesList);
            licenseeField.SelectByText(data.Licensee);

            var brandsList = _driver.FindElementWait(By.XPath("//select[contains(@data-bind, 'options: Model.brands')]"));
            var brandField = new SelectElement(brandsList);
            brandField.SelectByText(data.Brand);

            if (!string.IsNullOrEmpty(data.IpAddress))
            {
                var ipAddress = _driver.FindElementWait(By.XPath("//input[contains(@data-bind, 'value: Model.ipAddress')]"));
                ipAddress.SendKeys(data.IpAddress);
            }

            var description = _driver.FindElementWait(By.XPath("//textarea[contains(@data-bind, 'value: Model.description')]"));
            description.SendKeys(data.Description);

            if (data.BlockingType != null)
            {
                var blockingTypeList = _driver.FindElementWait(By.XPath("//select[contains(@data-bind, 'options: Model.blockingTypes')]"));
                var blockingTypeField = new SelectElement(blockingTypeList);
                blockingTypeField.SelectByText(data.BlockingType);
            }

            if (data.RedirectUrl != null)
            {
                var redirection = _driver.FindElementWait(By.XPath("//input[contains(@data-bind, 'value: Model.redirectionUrl')]"));
                redirection.Clear();
                redirection.SendKeys(data.RedirectUrl);
            }
            _saveButton.Click();
            var submittedForm = new ViewBrandIpRegulationForm(_driver);
            submittedForm.Initialize();
            return submittedForm;
        }

        public void ClearFieldsOnForm()
        {
            const string editBrandIpRegulationFormXPath = "admin/ip-regulations/brand/brand-add-edit-ip-regulation";
            base.ClearFieldsOnForm(editBrandIpRegulationFormXPath);
        }

#pragma warning disable 649
        [FindsBy(How = How.XPath, Using = "//button[contains(@data-bind, 'click: save')]")]
        private IWebElement _saveButton;
#pragma warning restore 649

    }
}