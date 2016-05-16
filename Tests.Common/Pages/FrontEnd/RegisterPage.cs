using System.Collections.Generic;
using System.Linq;
using System.Web.Configuration;
using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;
using OpenQA.Selenium.Support.UI;

namespace AFT.RegoV2.Tests.Common.Pages.FrontEnd
{
    public class RegisterPage : FrontendPageBase
    {
        public RegisterPage(IWebDriver driver) : base(driver) { }

        protected override string GetPageUrl()
        {
            return "en-US/Home/Register";
        }

        public virtual void NavigateToMemberWebsite(string referralId)
        {
            var url = WebConfigurationManager.AppSettings["MemberWebsiteUrl"] + string.Format("{0}?referralId={1}", GetPageUrl(), referralId);
            _driver.Navigate().GoToUrl(url);
            Initialize();
        }

        public bool Register(RegistrationDataForMemberWebsite data)
        {
            EnterRegistrationData(data);
            ClickRegisterButton();
            _driver.FindElementWait(By.XPath("//div[@id='register2-wrapper']|//div[@id='register-messages']"));
            return _driver.FindElements(By.Id("register2-wrapper")).Any();
        }

        public PlayerProfilePage GoToPlayerProfile()
        {
            _playerProfileUrl.Click();
            var playerProfilePage = new PlayerProfilePage(_driver);
            playerProfilePage.Initialize();
            return playerProfilePage;
        }

        public void RegisterWithInvalidData(RegistrationDataForMemberWebsite data)
        {
            _username.SendKeys(data.Username);
            _firstName.SendKeys(data.FirstName);
            _lastName.SendKeys(data.LastName);
            _email.SendKeys(data.Email);
            _phoneNumber.SendKeys(data.PhoneNumber);
            _password.SendKeys(data.Password);
            _passwordConfirm.SendKeys(data.Password);

            var dayOfBirth = new SelectElement(_dayOfBirth);
            _driver.WaitForJavaScript();
            dayOfBirth.SelectByValue("0");
            var monthOfBirth = new SelectElement(_monthOfBirth);
            monthOfBirth.SelectByValue("0");
            var yearOfBirth = new SelectElement(_yearOfBirth);
            yearOfBirth.SelectByValue("0");
            _address.SendKeys(data.Address);
            _postalCode.SendKeys(data.PostalCode);
            var country = new SelectElement(_country);
            _driver.WaitForJavaScript();

            country.SelectByText("--Please Select--");

            var currency = new SelectElement(_currency);
            _driver.WaitForJavaScript();
            currency.SelectByText("--Please Select--");

            var gender = new SelectElement(_gender);
            gender.SelectByText("--Please Select--");

            var title = new SelectElement(_title);
            title.SelectByText("--Please Select--");

            _addressLine2.SendKeys(data.AddressLine2);
            _addressLine3.SendKeys(data.AddressLine3);
            _addressLine4.SendKeys(data.AddressLine4);
            _city.SendKeys(data.City);


            var contactPreference = new SelectElement(_contactPreference);
            contactPreference.SelectByText("--Please Select--");

            ClickRegisterButton();
        }

        private void EnterRegistrationData(RegistrationDataForMemberWebsite data)
        {
            _username.SendKeys(data.Username);
            _password.SendKeys(data.Password);
            _passwordConfirm.SendKeys(data.Password);

            var title = new SelectElement(_title);
            title.SelectByValue(data.Title);

            _firstName.SendKeys(data.FirstName);
            _lastName.SendKeys(data.LastName);

            var gender = new SelectElement(_gender);
            gender.SelectByValue(data.Gender);

            _email.SendKeys(data.Email);
            _phoneNumber.SendKeys(data.PhoneNumber);

            _driver.WaitForJavaScript();

            new SelectElement(_dayOfBirth).SelectByText(data.Day.ToString());
            new SelectElement(_monthOfBirth).SelectByText(data.Month.ToString());
            new SelectElement(_yearOfBirth).SelectByText(data.Year.ToString());

            var country = new SelectElement(_country);
            country.SelectByValue(data.Country);

            var currency = new SelectElement(_currency);
            currency.SelectByValue(data.Currency);

            _address.SendKeys(data.Address);
            _addressLine2.SendKeys(data.AddressLine2);
            _addressLine3.SendKeys(data.AddressLine3);
            _addressLine4.SendKeys(data.AddressLine4);
            _city.SendKeys(data.City);
            _postalCode.SendKeys(data.PostalCode);

            _driver.WaitForJavaScript();

            var contactPreference = new SelectElement(_contactPreference);
            contactPreference.SelectByText(data.ContactPreference);

            var questions = new SelectElement(_securityQuestion);
            questions.SelectByValue(data.SecurityQuestion);

            _securityAnswer.SendKeys(data.SecurityAnswer);
        }

        public void ClickRegisterButton()
        {
            _registerButton.Click();
            _driver.WaitForJavaScript();
        }

        public IEnumerable<string> GetErrorMessages()
        {
            return ErrorMessages.FindElements(By.TagName("li")).AsEnumerable().Select(li => li.Text);
        }
        public PlayerProfilePage GoToProfilePage()
        {
            var profileLink = _driver.FindElementWait(By.XPath("//a[@href='/en-US/Home/PlayerProfile']"));
            profileLink.Click();

            return new PlayerProfilePage(_driver);
        }

        public void ClearFields()
        {
            _username.Clear();
            _password.Clear();
            _passwordConfirm.Clear();

            var title = new SelectElement(_title);
            title.SelectByText("--Please Select--");

            _firstName.Clear();
            _lastName.Clear();

            var gender = new SelectElement(_gender);
            gender.SelectByText("--Please Select--");

            _email.Clear();
            _phoneNumber.Clear();

            _driver.WaitForJavaScript();

            new SelectElement(_dayOfBirth).SelectByText("--Please Select--");
            new SelectElement(_monthOfBirth).SelectByText("--Please Select--");
            new SelectElement(_yearOfBirth).SelectByText("--Please Select--");

            var country = new SelectElement(_country);
            country.SelectByText("--Please Select--");

            var currency = new SelectElement(_currency);
            currency.SelectByText("--Please Select--");

            _address.Clear();
            _addressLine2.Clear();
            _addressLine3.Clear();
            _addressLine4.Clear();
            _city.Clear();
            _postalCode.Clear();

            _driver.WaitForJavaScript();

            var contactPreference = new SelectElement(_contactPreference);
            contactPreference.SelectByText("--Please Select--");

            var questions = new SelectElement(_securityQuestion);
            questions.SelectByText("--Please Select--");

            _securityAnswer.Clear();
        }

#pragma warning disable 649
        #region registration data
        [FindsBy(How = How.XPath, Using = "//input[contains(@data-bind, 'value: Username')]")]
        private IWebElement _username;

        [FindsBy(How = How.Id, Using = "first-name")]
        private IWebElement _firstName;

        [FindsBy(How = How.Id, Using = "last-name")]
        private IWebElement _lastName;

        [FindsBy(How = How.Id, Using = "email")]
        private IWebElement _email;

        [FindsBy(How = How.Id, Using = "phone-number")]
        private IWebElement _phoneNumber;

        [FindsBy(How = How.XPath, Using = "//input[contains(@data-bind, 'value: Password')]")]
        private IWebElement _password;

        [FindsBy(How = How.XPath, Using = "//input[contains(@data-bind, 'value: PasswordConfirm')]")]
        private IWebElement _passwordConfirm;

        [FindsBy(How = How.Id, Using = "day-of-birth")]
        private IWebElement _dayOfBirth;

        [FindsBy(How = How.Id, Using = "month-of-birth")]
        private IWebElement _monthOfBirth;

        [FindsBy(How = How.Id, Using = "year-of-birth")]
        private IWebElement _yearOfBirth;

        [FindsBy(How = How.Id, Using = "mailingAddressLine1")]
        private IWebElement _address;

        [FindsBy(How = How.Id, Using = "mailingAddressPostalCode")]
        private IWebElement _postalCode;

        [FindsBy(How = How.Id, Using = "country")]
        private IWebElement _country;

        [FindsBy(How = How.Id, Using = "currency")]
        private IWebElement _currency;

        [FindsBy(How = How.Id, Using = "gender")]
        private IWebElement _gender;

        [FindsBy(How = How.Id, Using = "title")]
        private IWebElement _title;

        [FindsBy(How = How.Id, Using = "mailingAddressLine2")]
        private IWebElement _addressLine2;

        [FindsBy(How = How.Id, Using = "mailingAddressLine3")]
        private IWebElement _addressLine3;

        [FindsBy(How = How.Id, Using = "mailingAddressLine4")]
        private IWebElement _addressLine4;

        [FindsBy(How = How.Id, Using = "mailingAddressCity")]
        private IWebElement _city;

        [FindsBy(How = How.Id, Using = "contactPreference")]
        private IWebElement _contactPreference;

        [FindsBy(How = How.Id, Using = "register-btn")]
        private IWebElement _registerButton;

        [FindsBy(How = How.Id, Using = "securityQuestionId")]
        private IWebElement _securityQuestion;

        [FindsBy(How = How.Id, Using = "securityAnswer")]
        private IWebElement _securityAnswer;

        [FindsBy(How = How.XPath, Using = "//a[@href='/en-US/Home/PlayerProfile']")]
        private IWebElement _playerProfileUrl;

        #endregion

        #region validation messages

        [FindsBy(How = How.XPath, Using = "//div[@id='register-messages']//ul")]
        public IWebElement ErrorMessages { get; private set; }

        #endregion
#pragma warning restore 649
    }

    public class RegistrationDataForMemberWebsite
    {
        public string Username;
        public string Password;
        public string Title;
        public string FirstName;
        public string LastName;
        public string Gender;
        public string Email;
        public string PhoneNumber;
        public int Day;
        public int Month;
        public int Year;
        public string Country;
        public string Currency;
        public string Address;
        public string AddressLine2;
        public string AddressLine3;
        public string AddressLine4;
        public string City;
        public string PostalCode;
        public string ContactPreference;
        public string SecurityQuestion;
        public string SecurityAnswer;

        public string FullName { get { return FirstName + " " + LastName; } }
    }
}
