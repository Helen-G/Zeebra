using System.Linq;
using AFT.RegoV2.Tests.Common.Extensions;
using AFT.RegoV2.Tests.Common.Pages.BackEnd.Bonus;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;
using OpenQA.Selenium.Support.UI;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class NewBrandIpRegulationForm : BackendPageBase
    {
        public NewBrandIpRegulationForm(IWebDriver driver) : base(driver){}

        public ViewBrandIpRegulationForm Submit(BrandIpRegulationData data)
        {
            var licenseesList = _driver.FindElementWait(By.XPath("//select[contains(@data-bind, 'options: Model.licensees')]"));
            var licenseeField = new SelectElement(licenseesList);
            licenseeField.SelectByText(data.Licensee);

            var brandsWidget = new MultiSelectWidget(_driver, By.XPath("//div[contains(@data-bind, 'items: Model.assignedBrands')]"));
            brandsWidget.SelectFromMultiSelect(data.Brand);

            if (!string.IsNullOrEmpty(data.IpAddress))
            {
                var ipAddress = _driver.FindElementWait(By.XPath("//input[contains(@data-bind, 'value: Model.ipAddress')]"));
                ipAddress.SendKeys(data.IpAddress);
            }

            if (data.AdvancedSettings)
            {
                var advancedSettings = _driver.FindElementWait(By.XPath("//input[contains(@data-bind, 'checked: advancedSettings')]"));
                advancedSettings.Click();

                var multipleIpAddresses = _driver.FindElementWait(By.XPath("//textarea[contains(@data-bind, 'value: Model.ipAddressBatch')]"));
                multipleIpAddresses.SendKeys(data.MultipleIpAddress);
            }

            var description = _driver.FindElementWait(By.XPath("//textarea[contains(@data-bind, 'value: Model.description')]"));
            description.SendKeys(data.Description);

            //var restrictionOption = string.Format("//span[text()='{0}']", data.Restriction);
            //var restriction = _driver.FindElementWait(By.XPath(restrictionOption));
            //restriction.Click();

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

#pragma warning disable 649
        [FindsBy(How = How.XPath, Using = "//button[contains(@data-bind, 'click: save')]")]
        private IWebElement _saveButton;
#pragma warning restore 649

    }

    public class ViewBrandIpRegulationForm : BackendPageBase
    {
        public ViewBrandIpRegulationForm(IWebDriver driver) : base(driver){}

        public object ConfirmationMessage
        {
            get { return _driver.FindElementValue(By.XPath("//div[contains(@data-bind, 'visible: message')]")); }
        }

        public object IpAddress
        {
            get { return _ipAddress.Text; }
        }

        public string IpAddressValidation
        {
            get
            {
                return _driver.FindElementValue(By.XPath("//span[contains(@data-bind, 'Model.ipAddress')]"));
            }
        }

        public void Close()
        {
            var close = _driver.FindElementWait(By.XPath("//button[text()='Close']"));
            close.Click();
        }

#pragma warning disable 649
        [FindsBy(How = How.XPath, Using = "//label[contains(., 'IP Address')]/following-sibling::div/p")]
        private IWebElement _ipAddress;
#pragma warning restore 649
    }
}