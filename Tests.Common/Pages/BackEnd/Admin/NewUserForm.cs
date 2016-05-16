using AFT.RegoV2.Tests.Common.Extensions;
using AFT.RegoV2.Tests.Common.Pages.BackEnd.Bonus;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class NewUserForm : BackendPageBase
    {
        public NewUserForm(IWebDriver driver) : base(driver)
        {       
        }

        public SubmittedNewUserForm Submit(AdminUserRegistrationData data)
        {

            var userName = _driver.FindElementWait(By.XPath("//input[contains(@data-bind, 'value: Model.username')]"));
            userName.SendKeys(data.UserName);
            
            var password = _driver.FindElementWait(By.XPath("//input[@data-bind='value: Model.password']"));
            password.SendKeys(data.Password);
            
            var retypePassword = _driver.FindElementWait(By.XPath("//input[@data-bind='value: Model.passwordConfirmation']"));
            retypePassword.SendKeys(data.Password);
            
            var firstName = _driver.FindElementWait(By.XPath("//input[contains(@data-bind, 'value: Model.firstName')]"));
            firstName.SendKeys(data.FirstName);
            
            var lastName = _driver.FindElementWait(By.XPath("//input[contains(@data-bind, 'value: Model.lastName')]"));
            lastName.SendKeys(data.LastName);
            
            var statusOption = string.Format("//span[text()='{0}']", data.Status);
            var status = _driver.FindElementWait(By.XPath(statusOption));
            status.Click();
            _driver.ScrollPage(0, 900);

            var licenseesWidget = new MultiSelectWidget(_driver, By.XPath("//div[contains(@data-bind, 'items: Model.assignedLicensees')]"));
            licenseesWidget.SelectFromMultiSelect(data.Licensee);

            var rolesList = _driver.FindElementWait(By.XPath("//select[contains(@data-bind, 'options: Model.roles')]"));
            var roleField = new SelectElement(rolesList);
            roleField.SelectByText(data.Role);

            var brandsWidget = new MultiSelectWidget(_driver, By.XPath("//div[contains(@data-bind, 'items: Model.allowedBrands')]"));
            brandsWidget.SelectFromMultiSelect(data.Brand);
            var currenciesWidget = new MultiSelectWidget(_driver, By.XPath("//div[contains(@data-bind, 'items: Model.currencies')]"));
            currenciesWidget.SelectFromMultiSelect(data.Currency);
            
            var description = _driver.FindElementWait(By.XPath("//textarea[contains(@data-bind, 'value: Model.description')]"));
            description.SendKeys(data.Description);
            
            var submitButton = _driver.FindElementWait(By.XPath("//button[text()='Save']"));
            submitButton.Click();
            var submittedForm = new SubmittedNewUserForm(_driver);
            
            return submittedForm;
        }

        public void SubmitDataExceedingMaxLength(AdminUserRegistrationData data)
        {

            var userName = _driver.FindElementWait(By.XPath("//input[contains(@data-bind, 'value: Model.username')]"));
            userName.SendKeys(data.UserName);

            var password = _driver.FindElementWait(By.XPath("//input[@data-bind='value: Model.password']"));
            password.SendKeys(data.Password);

            var retypePassword = _driver.FindElementWait(By.XPath("//input[@data-bind='value: Model.passwordConfirmation']"));
            retypePassword.SendKeys(data.Password);

            var firstName = _driver.FindElementWait(By.XPath("//input[contains(@data-bind, 'value: Model.firstName')]"));
            firstName.SendKeys(data.FirstName);

            _driver.ScrollPage(0, 600);
            var lastName = _driver.FindElementWait(By.XPath("//input[contains(@data-bind, 'value: Model.lastName')]"));
            lastName.SendKeys(data.LastName);

            var statusOption = string.Format("//span[text()='{0}']", data.Status);
            var status = _driver.FindElementWait(By.XPath(statusOption));
            status.Click();
            _driver.ScrollPage(0, 400);

            var licenseesWidget = new MultiSelectWidget(_driver, By.XPath("//div[contains(@data-bind, 'items: Model.assignedLicensees')]"));
            licenseesWidget.SelectFromMultiSelect(data.Licensee);

            var rolesList = _driver.FindElementWait(By.XPath("//select[contains(@data-bind, 'options: Model.roles')]"));
            var roleField = new SelectElement(rolesList);
            roleField.SelectByText(data.Role);

            var brandsWidget = new MultiSelectWidget(_driver, By.XPath("//div[contains(@data-bind, 'items: Model.allowedBrands')]"));
            brandsWidget.SelectFromMultiSelect(data.Brand);
            var currenciesWidget = new MultiSelectWidget(_driver, By.XPath("//div[contains(@data-bind, 'items: Model.currencies')]"));
            currenciesWidget.SelectFromMultiSelect(data.Currency);
            _driver.ScrollPage(0, 900);

            var description = _driver.FindElementWait(By.XPath("//textarea[contains(@data-bind, 'value: Model.description')]"));
            description.SendKeys(data.Description);

            var submitButton = _driver.FindElementWait(By.XPath("//button[text()='Save']"));
            submitButton.Click();
        }

        public string ValidationMessageForUsername
        {
            get
            {
                return _driver.FindElementValue(By.XPath("//input[contains(@data-bind, 'value: Model.username')]/following-sibling::span"));
            }
        }

        public string ValidationMessageForFirstName
        {
            get
            {
                return _driver.FindElementValue(By.XPath("//input[contains(@data-bind, 'value: Model.firstName')]/following-sibling::span"));
            }
        }

        public string ValidationMessageForLastName
        {
            get
            {
                return _driver.FindElementValue(By.XPath("//input[contains(@data-bind, 'value: Model.firstName')]/following-sibling::span"));
            }
        }
    }
}