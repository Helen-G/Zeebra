using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class NewBackendIpRegulationForm : BackendPageBase
    {
        public NewBackendIpRegulationForm(IWebDriver driver) : base(driver)
        {
        }

        public const string FormXPath =
            "//div[contains(@class, 'tab-content') and not(contains(@style, 'display: none'))]" +
            "/div/div[@data-view='admin/ip-regulations/admin/admin-add-edit-ip-regulation']";


        public SubmittedBackendIpRegulationForm Submit(BackendIpRegulationData data)
        {
            _ipAddressField.SendKeys(data.IpAddress);
            if (data.AdvancedSettings)
            {
                _advancedSettingsCheckbox.Click();
                _multipleIpAddresses.SendKeys(data.MultipleAddresses);
            }
            _descriptionField.SendKeys(data.Description);
            _saveButton.Click();
            var submittedForm = new SubmittedBackendIpRegulationForm(_driver);
            submittedForm.Initialize();
            return submittedForm;
        }

#pragma warning disable 649
        [FindsBy(How = How.XPath, Using = FormXPath + "//input[contains(@data-bind, 'value: Model.ipAddress')]")]
        private IWebElement _ipAddressField;

        [FindsBy(How = How.XPath, Using = FormXPath + "//input[@data-bind='checked: advancedSettings']")]
        private IWebElement _advancedSettingsCheckbox;

        [FindsBy(How = How.XPath, Using = FormXPath + "//input[@data-bind='checked: advancedSettings']")]
        private IWebElement _multipleIpAddresses;

        [FindsBy(How = How.XPath, Using = "//textarea[contains(@data-bind, 'value: Model.description')]")]
        private IWebElement _descriptionField;

        [FindsBy(How = How.XPath, Using = FormXPath + "//button[contains(@data-bind, 'click: save')]")]
        private IWebElement _saveButton;
#pragma warning restore 649
    }

    public class BackendIpRegulationData
    {
        public string IpAddress { get; set; }
        public bool AdvancedSettings { get; set; }
        public string MultipleAddresses { get; set; }
        public string Description { get; set; }
    }
}