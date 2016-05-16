using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class ViewRoleForm : BackendPageBase
    {
        public ViewRoleForm(IWebDriver driver) : base(driver) {}

        private const string FormXPath =
            "//div[contains(@class, 'nav-tabs-documents')]//div[@data-active-view='true']//div[contains(@class, 'tab-content') and not(contains(@style, 'display: none;'))]";
        
        public string RoleCode
        {
            get { return _driver.FindElementValue(By.XPath("//p[contains(@data-bind,'text: Model.code')]")); }
        }

        public string RoleName
        {
            get { return _roleName.Text; }
        }

        public string Permissions
        {
            get { return _permission.Text; }
        }

        public IWebElement Brands
        {
            get { return _brands; }
        }

        public string Module
        {
            get { return _module.Text; }
        }

        public string Permission
        {
            get { return _permission.Text; }
        }

        public string Message
        {
            get { return _driver.FindElementValue(By.XPath("//div[contains(@class, 'alert alert-success left')]")); }
        }

        public string Licensee
        {
            get { return _licensee.Text; }
        }

#pragma warning disable 649
        //[FindsBy(How = How.XPath, Using = "//p[contains(@data-bind,'text: Model.code')]")]
        //private IWebElement _roleCode;

        [FindsBy(How = How.XPath, Using ="//p[contains(@data-bind, 'text: Model.name')]")]
        private IWebElement _roleName;

        [FindsBy(How = How.XPath, Using = "//p[contains(@data-bind, 'text: Model.displayLicensees')]")]
        private IWebElement _licensee;

        [FindsBy(How = How.XPath, Using = "//select[contains(@data-bind, 'options: assignedItems')]")]
        private IWebElement _brands;

        [FindsBy(How = How.XPath, Using = "//div[contains(@class, 'ui-jqgrid-bdiv') and contains(@style, 'height: auto; width: 300px;')]//tr[2]/td[2]")]
        private IWebElement _module;

        [FindsBy(How = How.XPath, Using = "//div[contains(@class, 'ui-jqgrid-bdiv') and contains(@style, 'height: auto; width: 300px;')]//tr[2]/td[3]")]
        private IWebElement _permission;
#pragma warning restore 649
    }
}