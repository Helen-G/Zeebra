using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;
using OpenQA.Selenium.Support.UI;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class NewPlayerForm : BackendPageBase
    {
        public NewPlayerForm(IWebDriver driver) : base(driver) {}

        private const string FormXPath = "//div[@data-view='player-manager/add']";

        public string Title
        {
            get { return _driver.FindElementValue(By.XPath("//span[text()='New Player']")); }
        }

        public SubmittedPlayerForm Register(PlayerRegistrationDataForAdminWebsite data)
        {
            SelectLicenseeBrand(
                By.XPath("//label[contains(@data-bind, 'attr: { for: licenseeFieldId() }')]"),
                By.XPath("//select[contains(@id, 'licensee')]"),
                data.Licensee,
                By.XPath("//select[contains(@id, 'brand')]"),
                data.Brand);

            _loginName.SendKeys(data.LoginName);
            _password.SendKeys(data.Password);
            _confirmPassword.SendKeys(data.ConfirmPassword);

            var gender = new SelectElement(_gender);
            gender.SelectByText(data.Gender);

            var title = new SelectElement(_title);
            title.SelectByText(data.Title);

            _firstName.SendKeys(data.FirstName);
            _lastName.SendKeys(data.LastName);
            _emailAddress.SendKeys(data.Email);
            _mobileNumber.SendKeys(data.MobileNumber);

            if (_country.Displayed)
            {
                var country = new SelectElement(_country);
                country.SelectByText(data.Country);
            }
            if (_currency.Displayed)
            {
                var currency = new SelectElement(_currency);
                currency.SelectByText(data.Currency); 
            }

            if (_culture.Displayed)
            {
                var culture = new SelectElement(_culture);
                culture.SelectByText(data.Culture); 
            }
            _affiliateCode.SendKeys(data.AffiliateCode);

            var accountStatus = new SelectElement(_accountStatus);
            accountStatus.SelectByText(data.AccountStatus);

            _addressLine1.SendKeys(data.Address);
            _addressLine2.SendKeys(data.AddressLine2);
            _addressLine3.SendKeys(data.AddressLine3);
            _addressLine4.SendKeys(data.AddressLine4);
            _city.SendKeys(data.City);
            _zipCode.SendKeys(data.ZipCode);

            var contactPreference = new SelectElement(_contactPreference);
            contactPreference.SelectByText(data.ContactPreference);

            if (data.AccountAlertEmail)
            {
                _accountAlertEmail.Click();
            }

            if (data.AccountAlertSms)
            {
                _accountAlertSms.Click();
            }

            _saveButton.Click();
            _driver.ScrollPage(1200, 0);

            var submittedForm = new SubmittedPlayerForm(_driver);
            return submittedForm;
        }

#pragma warning disable 649
        #region registration data
        [FindsBy(How = How.XPath, Using = FormXPath + "//input[contains(@id, 'username')]")]
        private IWebElement _loginName;

        [FindsBy(How = How.XPath, Using = FormXPath + "//input[contains(@id, 'first-name')]")]
        private IWebElement _firstName;

        [FindsBy(How = How.XPath, Using = FormXPath + "//input[contains(@id, 'last-name')]")]
        private IWebElement _lastName;

        [FindsBy(How = How.XPath, Using = FormXPath + "//input[contains(@id, 'email')]")]
        private IWebElement _emailAddress;

        //[FindsBy(How = How.XPath, Using = FormXPath + "//input[contains(@id, 'add-player-date-of-birth')]")]
        //private IWebElement _dateOfBirth;

        [FindsBy(How = How.XPath, Using = FormXPath + "//input[contains(@id, 'phone-number')]")]
        private IWebElement _mobileNumber;

        [FindsBy(How = How.XPath, Using = FormXPath + "//input[contains(@id, 'add-player-password')]")]
        private IWebElement _password;

        [FindsBy(How = How.XPath, Using = FormXPath + "//input[contains(@id, 'password-confirm')]")]
        private IWebElement _confirmPassword;

        [FindsBy(How = How.XPath, Using = FormXPath + "//input[contains(@id, 'mailing-address-line1')]")]
        private IWebElement _addressLine1;

        [FindsBy(How = How.XPath, Using = FormXPath + "//input[contains(@id, 'postal-code')]")]
        private IWebElement _zipCode;

        [FindsBy(How = How.XPath, Using = FormXPath + "//select[contains(@id, 'currency')]")]
        private IWebElement _currency;

        [FindsBy(How = How.XPath, Using = FormXPath + "//select[contains(@id, 'gender')]")]
        private IWebElement _gender;
        
        [FindsBy(How = How.XPath, Using = FormXPath + "//select[contains(@id, 'title')]")]
        private IWebElement _title;

        [FindsBy(How = How.XPath, Using = FormXPath + "//input[contains(@id, 'city')]")]
        private IWebElement _city;

        [FindsBy(How = How.XPath, Using = FormXPath + "//input[contains(@id, 'mailing-address-line2')]")]
        private IWebElement _addressLine2;

        [FindsBy(How = How.XPath, Using = FormXPath + "//input[contains(@id, 'mailing-address-line3')]")]
        private IWebElement _addressLine3;

        [FindsBy(How = How.XPath, Using = FormXPath + "//input[contains(@id, 'mailing-address-line4')]")]
        private IWebElement _addressLine4;

        [FindsBy(How = How.XPath, Using = FormXPath + "//button[text()='Save']")]
        private IWebElement _saveButton;

        [FindsBy(How = How.XPath, Using = FormXPath + "//select[contains(@id, 'country')]")]
        private IWebElement _country;

        [FindsBy(How = How.XPath, Using = FormXPath + "//select[contains(@id, 'culture')]")]
        private IWebElement _culture;

        [FindsBy(How = How.XPath, Using = FormXPath + "//input[contains(@id, 'affiliate')]")]
        private IWebElement _affiliateCode;

        [FindsBy(How = How.XPath, Using = FormXPath + "//select[contains(@id, 'contact-preference')]")]
        private IWebElement _contactPreference;

        [FindsBy(How = How.XPath, Using = FormXPath + "//input[contains(@id, 'account-alert-email')]")] 
        private IWebElement _accountAlertEmail;

        [FindsBy(How = How.XPath, Using = FormXPath + "//input[contains(@id, 'account-alert-sms')]")]
        private IWebElement _accountAlertSms;

        [FindsBy(How = How.XPath, Using = FormXPath + "//select[contains(@id, 'account-status')]")]
        private IWebElement _accountStatus;
        #endregion
#pragma warning restore 649
    }
}