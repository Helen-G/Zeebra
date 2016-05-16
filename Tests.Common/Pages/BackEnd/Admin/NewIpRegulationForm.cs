using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class NewIpRegulationForm : BackendPageBase
    {
        public NewIpRegulationForm(IWebDriver driver)
            : base(driver)
        {
        }

        public ViewIpRegulationForm Submit(IpRegulationData data)
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
            
            if (data.AdvancedSettings)
            {
                var advancedSettings = _driver.FindElementWait(By.XPath("//input[contains(@data-bind, 'checked: advancedSettings')]"));
                advancedSettings.Click();

                var multipleIpAddresses = _driver.FindElementWait(By.XPath("//textarea[contains(@data-bind, 'value: Model.ipAddressBatch')]"));
                multipleIpAddresses.SendKeys(data.MultipleIpAddress);
            }

            var description = _driver.FindElementWait(By.XPath("//textarea[contains(@data-bind, 'value: Model.description')]"));
            description.SendKeys(data.Description);

            var restrictionOption = string.Format("//span[text()='{0}']", data.Restriction);
            var restriction = _driver.FindElementWait(By.XPath(restrictionOption));
            restriction.Click();

            _driver.ScrollPage(0, 600);

            if (data.ApplyToBrand)
            {
                var applyToBrand = _driver.FindElementWait(By.XPath("//input[contains(@data-bind, 'checked: Model.isAppliedToBrand')]"));
                applyToBrand.Click();
            }

            if (data.ApplyToBackend)
            {
                var applyToBackend = _driver.FindElementWait(By.XPath("//input[contains(@data-bind, 'checked: Model.isAppliedToBackend')]"));
                applyToBackend.Click();
            }

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

            _driver.ScrollPage(0, 600);

            var submitButton = _driver.FindElementWait(By.XPath("//button[contains(@data-bind, 'click: save')]"));
            submitButton.Click();
            var submittedForm = new ViewIpRegulationForm(_driver);

            return submittedForm;
        }
    }
}
