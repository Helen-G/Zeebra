using System;
using AFT.RegoV2.Tests.Common.Extensions;
using AFT.RegoV2.Tests.Common.Pages.BackEnd.Bonus;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;
using OpenQA.Selenium.Support.UI;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class EditRoleForm : BackendPageBase
    {
        public EditRoleForm(IWebDriver driver) : base(driver){}

        public void EditRequiredFields(string code, string name, string licensee, string brand)
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(45));
            wait.Until(x => _driver.FindElementWait(By.XPath("//span[text()='Role Code']")));

            if (code != null)
            {
                _roleCodeField.Clear();
                _roleCodeField.SendKeys(code);
            }

            if (name != null)
            {
                _roleNameField.Clear();
                _roleNameField.SendKeys(name);
            }

            if (licensee != null)
            {
                var licenseesWidget = new MultiSelectWidget(_driver, By.XPath("//div[contains(@data-bind, 'items: Model.assignedLicensees')]"));
                licenseesWidget.DeselectFromMultiSelect(licensee);
                licenseesWidget.SelectFromMultiSelect(licensee);
            }
        }

        public void UpdatePermissions(string[] permissions)
        {
            _driver.ScrollPage(0, 900);

            ExpandPermissions();

            foreach (var permission in permissions)
            {
                var elementXPath = string.Format("//tr[td/@title='{0}']//input", permission);
                _driver.FindElementClick(elementXPath);
            }
        }

        public void ExpandPermissions()
        {
            const string expandButtonXPath = "//button[contains(@data-bind, 'click: expandGrid')]";
            _driver.FindElementClick(expandButtonXPath);
        }
        
        
        public SubmittedRoleForm Submit()
        {
            _submitButton.Click();
            var form = new SubmittedRoleForm(_driver);

            form.Initialize();
            return form;
        }


#pragma warning disable 649
        [FindsBy(How = How.XPath, Using = "//input[contains(@data-bind, 'value: Model.code')]")]
        private IWebElement _roleCodeField;

        [FindsBy(How = How.XPath, Using = "//input[contains(@data-bind, 'value: Model.name')]")]
        private IWebElement _roleNameField;

        [FindsBy(How = How.XPath, Using = "//div[@id='add-role-home']//button[text()='Save']")]
        private IWebElement _submitButton; 
#pragma warning restore 649

    }
}