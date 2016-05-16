using System;
using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;
using OpenQA.Selenium.Support.UI;

namespace AFT.RegoV2.Tests.Common.Pages.FrontEnd
{
    public class PlayerProfilePage : FrontendPageBase
    {
        public PlayerProfilePage(IWebDriver driver) : base(driver) {}

        public void ClearFields()
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(45));
            wait.Until(d => _oldPassword.Displayed);
            _oldPassword.Clear();
            _newPassword.Clear();
            _confirmPassword.Clear();
        }

        protected override string GetPageUrl()
        {
            return "en-US/Home/PlayerProfile";
        }

        public MemberWebsiteLoginPage Logout()
        {
            _driver.WaitForJavaScript();
            _logoutUrl.Click();
            return new MemberWebsiteLoginPage(_driver);
        }

        public string GetUserName()
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(45));
            wait.Until(d =>
            {
                try
                {
                    Initialize();
                    return Username.Displayed;
                }
                catch (StaleElementReferenceException)
                {
                    return false;
                }
            });
            return Username.Text;
        }

        public void ChangePassword()
        {
            ChangePassword(GetUserName(), "new-password");
        }

        public void EnterOnlyNewPassword()
        {
            ChangePassword(null, "new-password");
        }

        public void EnterNewPassword(string password)
        {
            ChangePassword(GetUserName(), password);
        }

        public void EnterNewPasswordWithoutConfirmPassword()
        {
            ChangePassword(GetUserName(), "new-password", string.Empty);
        }

        public void EnterNewPasswordWithIncorrectConfirmPassword()
        {
            ChangePassword(GetUserName(), "new-password", "another-password");
        }

        private void ChangePassword(string oldPassword, string newPassword, string confirmPassword = null)
        {
            if (oldPassword != null)
            {
                _oldPassword.SendKeys(oldPassword);
            }
            if (newPassword != null)
            {
                _newPassword.SendKeys(newPassword);
                _confirmPassword.SendKeys(confirmPassword ?? newPassword);
            }
            _changePasswordBtn.Click();
            _driver.WaitForJavaScript();
        }

        public string GetPasswordChangedMsg()
        {
            return _passwordChangeSuccessMsg.Text;
        }

        public string GetPasswordChangedErrorMsg()
        {
            return _passwordChangeErrorMsg.Text;
        }

        public void RequestMobileVerificationCode()
        {
            _editContactInformationBtn.Click();
            _requestMobileVerificationCodeBtn.Click();
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(30));
            wait.Until(driver => _verifyMobileSucessLabel.Text == "Verification code has been sent.");
            _saveContactInformationBtn.Click();
        }

        public void VerifyMobileNumber(int mobileVerificationCode)
        {
            _editContactInformationBtn.Click();
            _mobileVerificationCodeField.SendKeys(mobileVerificationCode.ToString("D4"));
            _verifyMobileNumberBtn.Click();
            _saveContactInformationBtn.Click();
        }

        #region player profile
#pragma warning disable 649
        [FindsBy(How = How.XPath, Using = "//button[@data-bind='click: logout']")]
        private IWebElement _logoutUrl;

        [FindsBy(How = How.XPath, Using = "//span[@data-bind='text: account.username']")]
        public IWebElement Username { get; private set; }

        [FindsBy(How = How.XPath, Using = "//span[contains(@data-bind, 'text: personal.firstName')]")]
        public IWebElement FirstName { get; private set; }

        [FindsBy(How = How.XPath, Using = "//span[contains(@data-bind, 'text: personal.lastName')]")]
        public IWebElement LastName { get; private set; }

        [FindsBy(How = How.XPath, Using = "//span[contains(@data-bind, 'text: personal.dateOfBirth')]")]
        public IWebElement DateOfBirth { get; private set; }

        [FindsBy(How = How.XPath, Using = "//span[contains(@data-bind, 'text: contacts.phoneNumber')]")]
        public IWebElement PhoneNumber { get; private set; }

        [FindsBy(How = How.XPath, Using = "//span[contains(@data-bind, 'text: personal.email')]")]
        public IWebElement Email { get; private set; }

        [FindsBy(How = How.XPath, Using = "//span[contains(@data-bind, 'text: contacts.address')]")]
        public IWebElement Address { get; private set; }

        [FindsBy(How = How.XPath, Using = "//span[contains(@data-bind, 'text: contacts.postalCode')]")]
        public IWebElement ZipCode { get; private set; }

        [FindsBy(How = How.XPath, Using = "//span[contains(@data-bind, 'text: contacts.countryCode')]")]
        public IWebElement CountryCode { get; private set; }

        [FindsBy(How = How.XPath, Using = "//span[contains(@data-bind, 'text: personal.currencyCode')]")]
        public IWebElement CurrencyCode { get; private set; }

        [FindsBy(How = How.Id, Using = "oldPassword")]
        private IWebElement _oldPassword;

        [FindsBy(How = How.Id, Using = "newPassword")]
        private IWebElement _newPassword;

        [FindsBy(How = How.Id, Using = "confirmPassword")]
        private IWebElement _confirmPassword;

        [FindsBy(How = How.Id, Using = "changePasswordBtn")]
        private IWebElement _changePasswordBtn;

        [FindsBy(How = How.XPath, Using = "//label[@data-bind='visible: account.success']")]
        private IWebElement _passwordChangeSuccessMsg;

        [FindsBy(How = How.XPath, Using = "//label[contains(@data-bind, 'text: account.errorMessage')]")]
        private IWebElement _passwordChangeErrorMsg;

        [FindsBy(How = How.XPath, Using = "//button[contains(@data-bind, 'click: verification.requestCode')]")]
        private IWebElement _requestMobileVerificationCodeBtn;

        [FindsBy(How = How.XPath, Using = "//input[contains(@data-bind, 'value: verification.code')]")]
        private IWebElement _mobileVerificationCodeField;

        [FindsBy(How = How.XPath, Using = "//button[contains(@data-bind, 'click: verification.verifyPhoneNumber')]")]
        private IWebElement _verifyMobileNumberBtn;

        [FindsBy(How = How.XPath, Using = "//label[contains(@data-bind, 'text: verification.successMessage')]")]
        private IWebElement _verifyMobileSucessLabel;

        [FindsBy(How = How.XPath, Using = "//button[contains(@data-bind, 'click: contacts.edit')]")]
        private IWebElement _editContactInformationBtn;

        [FindsBy(How = How.XPath, Using = "//button[contains(@data-bind, 'click: contacts.save')]")]
        private IWebElement _saveContactInformationBtn;


#pragma warning restore 649
        #endregion
    }
}