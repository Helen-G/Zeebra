using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class SubmittedRoleForm : BackendPageBase
    {
        public SubmittedRoleForm(IWebDriver driver) : base(driver) { }

        public string RoleCode
        {
            get { return _driver.FindElementValue(By.XPath("//p[contains(@data-bind, 'text: Model.code')]")); }
        }

        public string RoleName
        {
            get { return _driver.FindElementValue(By.XPath("//p[contains(@data-bind, 'text: Model.name')]")); }
        }

        public string Licensee
        {
            get { return _driver.FindElementValue(By.XPath("//p[contains(@data-bind, 'text: Model.displayLicensees')]")); }
        }

        public string ConfirmationMessage
        {
            get { return _driver.FindElementValue(By.XPath("//div[@id='add-role-home']/div")); }
        }

        public string Module
        {
            get { return _module.Text; }
        }

        public string Permission
        {
            get { return _permission.Text; }
        }

#pragma warning disable 649

        [FindsBy(How = How.XPath, Using = "//div[contains(@class, 'ui-jqgrid-bdiv') and contains(@style, 'height: auto; width: 300px;')]//tr[2]/td[2]")]
        private IWebElement _module;

        [FindsBy(How = How.XPath, Using = "//div[contains(@class, 'ui-jqgrid-bdiv') and contains(@style, 'height: auto; width: 300px;')]//tr[2]/td[3]")]
        private IWebElement _permission;

#pragma warning restore 649
    }
}