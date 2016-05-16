using System;
using System.Linq;
using System.Threading;
using AFT.RegoV2.Tests.Common.Extensions;
using AFT.RegoV2.Tests.Common.Pages.BackEnd.Bonus;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class EditUserForm : BackendPageBase
    {
        public EditUserForm(IWebDriver driver) : base(driver) { }

        public SubmittedNewUserForm SubmitEditedData(AdminUserRegistrationData editAdminUserData)
        {
            var usernameField = _driver.FindElementWait(By.XPath("//input[contains(@data-bind, 'value: Model.username')]"));
            usernameField.SendKeys(editAdminUserData.UserName);

            var firstNameField = _driver.FindElementWait(By.XPath("//input[contains(@data-bind, 'value: Model.firstName')]"));
            firstNameField.SendKeys(editAdminUserData.FirstName);

            _driver.ScrollPage(0, 600);
            var lastNameField = _driver.FindElementWait(By.XPath("//input[contains(@data-bind, 'value: Model.lastName')]"));
            lastNameField.SendKeys(editAdminUserData.LastName);

            var statusOption = string.Format("//span[text()='{0}']", editAdminUserData.Status);
            var statusField = _driver.FindElementWait(By.XPath(statusOption));
            statusField.Click();
            _driver.ScrollPage(0, 400);

            var licenseesWidget = new MultiSelectWidget(_driver, By.XPath("//div[contains(@data-bind, 'items: Model.assignedLicensees')]"));
            licenseesWidget.SelectFromMultiSelect(editAdminUserData.Licensee);

            var brandsWidget = new MultiSelectWidget(_driver, By.XPath("//div[contains(@data-bind, 'items: Model.allowedBrands')]"));
            brandsWidget.SelectFromMultiSelect(editAdminUserData.Brand);
            var currenciesWidget = new MultiSelectWidget(_driver, By.XPath("//div[contains(@data-bind, 'items: Model.currencies')]"));
            currenciesWidget.SelectFromMultiSelect(editAdminUserData.Currency);

            _driver.ScrollPage(0, 800);

            var descriptionField = _driver.FindElementWait(By.XPath("//textarea[contains(@data-bind, 'value: Model.description')]"));
            descriptionField.SendKeys(editAdminUserData.Description);

            var submitButton = _driver.FindElementWait(By.XPath("//button[text()='Save']"));
            submitButton.Click();
            var submittedForm = new SubmittedNewUserForm(_driver);
            return submittedForm;
        }

        public void ClearFieldsOnForm()
        {
            const string editUserFormXPath = "admin/admin-manager/edit-user";
            base.ClearFieldsOnForm(editUserFormXPath);
        }

        public SubmittedNewUserForm ChangeRole(string roleName)
        {
            var rolesList = _driver.FindElementWait(By.XPath("//select[contains(@data-bind, 'options: Model.roles')]"));
            var roleField = new SelectElement(rolesList);

            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
            wait.Until(x => roleField.Options.Any(o => o.Text == roleName));
            roleField.SelectByText(roleName);

            var submitButton = _driver.FindElementWait(By.XPath("//button[text()='Save']"));
            submitButton.Click();
            var submittedForm = new SubmittedNewUserForm(_driver);
            return submittedForm;
        }
    }
}