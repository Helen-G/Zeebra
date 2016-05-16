using System;
using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class AdminManagerPage : BackendPageBase
    {
        protected const string GridXPath = "//div[contains(@data-view, 'admin/admin-manager/list')]";

        public AdminManagerPage(IWebDriver driver) : base(driver)
        {
        }

        public Grid Grid
        {
            get
            {
                return new Grid(_driver, GridXPath);
            }
        }

        public string UserStatus
        {
            get
            {
                return _driver.FindElementValue(By.XPath("//td[@title='Active']"));
            }
        }

        public NewUserForm OpenNewUserForm()
        {
            var newUserButton = FindActionButton("new", GridXPath);
            newUserButton.Click();
            var form = new NewUserForm(_driver);
            return form;
        }

        public ViewUserForm OpenViewUserForm(string username)
        {
            Grid.SelectRecord(username);
            var viewUserButton = FindActionButton("view", GridXPath);
            viewUserButton.Click();
            var form = new ViewUserForm(_driver);
            return form;
        }

        public void ActivateUser(string buttonName)
        {
            var activateButton = _driver.FindElementWait(By.XPath(buttonName));
            activateButton.Click();
        }

        public void SelectAndActivateUser(string username)
        {
            Grid.SelectRecord(username);
            var activateButton = FindActionButton("activate", GridXPath);
            activateButton.Click();

            var modalRemarks = _driver.FindElementWait(By.XPath("//div[@class='modal-content']//textarea[contains(@data-bind, 'value: remarks')]"));
            modalRemarks.SendKeys("User activation");
            var modalActivateButton = _driver.FindElementWait(By.XPath("//div[contains(@data-view, 'admin/admin-manager/dialogs/user-status-dialog')]//button[text()='Activate']"));
            modalActivateButton.Click();
            
            var modalCloseButton = _driver.FindElementWait(By.XPath("//div[@class='modal-content']//button[text()='Close']"));
            var wait = new WebDriverWait(_driver,TimeSpan.FromSeconds(10));
            wait.Until(x => modalCloseButton.Displayed && modalCloseButton.Displayed);
            modalCloseButton.Click();
        }

        public void DeactivateUser(string buttonName)
        {
            var deactivateUserButton = _driver.FindElementWait(By.XPath(buttonName));
            deactivateUserButton.Click();
        }

        public EditUserForm OpenEditUserForm(string username)
        {
            Grid.SelectRecord(username);
            var editUserButton = FindActionButton("edit", GridXPath);
            editUserButton.Click();
            var editForm = new EditUserForm(_driver);
            return editForm;
        }

        public void SelectAndDeactivateUser(string username)
        {
            Grid.SelectRecord(username);
            var deactivateButton = FindActionButton("deactivate", GridXPath);
            deactivateButton.Click();

            var modalRemarks = _driver.FindElementWait(By.XPath("//div[@class='modal-content']//textarea[contains(@data-bind, 'value: remarks')]"));
            modalRemarks.SendKeys("User deactivation");
            var modalDeactivateButton = _driver.FindElementWait(By.XPath("//div[contains(@data-view, 'admin/admin-manager/dialogs/user-status-dialog')]//button[text()='Deactivate']"));
            modalDeactivateButton.Click();
            var modalCloseButton = _driver.FindElementWait(By.XPath("//div[@class='modal-content']//button[text()='Close']"));
            modalCloseButton.Click();
        }

        public ResetUserPasswordPage OpenResetPasswordTab(string userName)
        {
            Grid.SelectRecord(userName);
            var resetButton = FindActionButton("resetPassword", GridXPath);
            resetButton.Click();
            var page = new ResetUserPasswordPage(_driver);
            return page;
        }
    }
}