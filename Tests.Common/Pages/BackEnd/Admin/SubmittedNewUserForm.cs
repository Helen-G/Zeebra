using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class SubmittedNewUserForm : BackendPageBase
    {
        public SubmittedNewUserForm(IWebDriver driver) : base(driver)
        {
        }

        public string XPath = "//div[@data-view='admin/admin-manager/add-user']";
        
        public string Username
        {
            get
            {
                return
                    _driver.FindElementValue(
                        By.XPath("//p[contains(@data-bind, 'text: Model.username')]"));
            }
        }

        public string FirstName
        {
            get
            {
                return
                    _driver.FindElementValue(By.XPath("//p[contains(@data-bind, 'text: Model.firstName')]"));
            }
        }

        public string LastName
        {
            get
            {
                return
                    _driver.FindElementValue(By.XPath("//p[contains(@data-bind, 'text: Model.lastName')]"));
            }
        }

        public string ConfirmationMessage
        {
            get { return _driver.FindElementValue(By.XPath("//div[contains(@data-bind, 'visible: message')]")); }
        }

        public string UserRole
        {
            get { return _driver.FindElementValue(By.XPath("//p[contains(@data-bind, 'Model.displayRole')]")); }
        }

        public string Status
        {
            get
            {
                return _driver.FindElementValue(By.XPath("//p[contains(@data-bind, 'Model.isActive.ToStatus()')]"));
            }
        }

        public string Brand
        {
            get
            {
                return _driver.FindElementValue(By.XPath("/div[contains(@class, 'tab-content') and not(contains(@style, 'display: none'))]" +
                                                         "/div/form[@data-view='admin/admin-manager/view-user']//label[contains(., 'Assigned Brands')]" +
                                                         "/following-sibling::select/option"));
            }
        }

        public string EditedBrand
        {
            get
            {
                return _driver.FindElementValue(By.XPath("//p[contains(@data-bind, 'text: Model.displayBrands')]"));
            }
        }

        public string Currency
        {
            get
            {
                return _driver.FindElementValue(By.XPath("//p[contains(@data-bind, 'text: Model.displayCurrencies')]"));
            }
        }

        public string Description
        {
            get
            {
                return _driver.FindElementValue(By.XPath("//p[contains(@data-bind, 'text: Model.description')]"));
            }
        }

        public AdminManagerPage SwitchToList()
        {
           var usersTab = _driver.FindElementWait(By.XPath("//ul[@data-view='layout/document-container/tabs']//span[text()='Users']"));
           usersTab.Click();
           var page = new AdminManagerPage(_driver);
           return page;
        }
    }
}