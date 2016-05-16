using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class ViewUserForm : BackendPageBase
    {
        public ViewUserForm(IWebDriver driver) : base(driver)
        {
        }

        private const string BaseXPath = "//form[@data-view='admin/admin-manager/view-user']";

        public string UserName
        {
            get
            {
                return _driver.FindElementValue(By.XPath(BaseXPath +"//p[@data-bind='text: Model.username']"));
            }
        }

        public string UserRole
        {
            get
            {
                return _driver.FindElementValue(By.XPath(BaseXPath + "//p[@data-bind='text: Model.role']"));
            }
        }

        public string FirstName
        {
            get
            {
                return _driver.FindElementValue(By.XPath(BaseXPath + "//p[@data-bind='text: Model.firstName']"));
            }
        }

        public string LastName
        {
            get
            {
                return _driver.FindElementValue(By.XPath(BaseXPath + "//p[@data-bind='text: Model.lastName']"));
            }
        }

        public string UserCurrencies
        {
            get
            {
                return _driver.FindElementValue(By.XPath(BaseXPath + "//p[@data-bind='text: Model.displayCurrencies']"));
            }
        }
    }
}