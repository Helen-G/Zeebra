using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class EditBackendIpRegulationForm : BackendPageBase
    {
        public EditBackendIpRegulationForm(IWebDriver driver)
            : base(driver)
        {
        }

        public const string FormXPath =
            "//div[contains(@class, 'tab-content') and not(contains(@style, 'display: none'))]" +
            "/div/div[@data-view='admin/ip-regulations/admin/admin-add-edit-ip-regulation']";


        public SubmittedBackendIpRegulationForm Submit(EditBackendIpRegulationData data)
        {
            _ipAddressField.SendKeys(data.IpAddress);

            _descriptionField.SendKeys(data.Description);
            _saveButton.Click();
            var submittedForm = new SubmittedBackendIpRegulationForm(_driver);
            submittedForm.Initialize();
            return submittedForm;
        }

        public void ClearFieldsOnForm()
        {
            const string editBackendIpRegulationFormXPath = "admin/ip-regulations/admin/admin-add-edit-ip-regulation";
            base.ClearFieldsOnForm(editBackendIpRegulationFormXPath);
        }

#pragma warning disable 649
        [FindsBy(How = How.XPath, Using = FormXPath + "//input[contains(@data-bind, 'value: Model.ipAddress')]")]
        private IWebElement _ipAddressField;

        [FindsBy(How = How.XPath, Using = "//textarea[contains(@data-bind, 'value: Model.description')]")]
        private IWebElement _descriptionField;

        [FindsBy(How = How.XPath, Using = FormXPath + "//button[contains(@data-bind, 'click: save')]")]
        private IWebElement _saveButton;
#pragma warning restore 649
    }

    public class EditBackendIpRegulationData
    {
        public string IpAddress { get; set; }
        public string Description { get; set; }
    }
}