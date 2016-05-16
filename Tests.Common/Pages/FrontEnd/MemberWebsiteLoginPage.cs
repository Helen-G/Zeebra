using System;
using System.Threading;
using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;
using OpenQA.Selenium.Support.UI;

namespace AFT.RegoV2.Tests.Common.Pages.FrontEnd
{
    public class MemberWebsiteLoginPage : FrontendPageBase
    {
        public MemberWebsiteLoginPage(IWebDriver driver) : base(driver) {}

        protected override string GetPageUrl()
        {
            return "en-US/Home/Login";
        }

        public PlayerProfilePage Login(string username, string password)
        {
            Initialize();
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(45));
            wait.Until(d => _usernameField.Displayed);
            _usernameField.Clear();
            _usernameField.SendKeysWithOnChangeEvent(username);
            
            _passwordField.Clear();
            _passwordField.SendKeysWithOnChangeEvent(password);
            ClickSignInButton();
            var page = new PlayerProfilePage(_driver);
            page.Initialize();
            return page;
        }

        public void TryToLogin(string username, string password)
        {
            _driver.WaitForJavaScript();
            Initialize();
            _usernameField.Clear();
            _passwordField.Clear();
            _usernameField.SendKeys(username);
            _passwordField.SendKeys(password);
            ClickSignInButton();
            Thread.Sleep(TimeSpan.FromSeconds(1)); // Wait for message box fade in animation
        }

        public void ClickSignInButton()
        {
            _signInButton.Click();
            _driver.WaitForJavaScript();
        }

        public string GetUsernameErrorMsg()
        {
            return _usernameErrorMsg.Text;
        }

        public string GetPasswordErrorMsg()
        {
            return _passwordErrorMsg.Text;
        }

        public string GetErrorMsg()
        {
            return _errorMsg.Text;
        }

        public string GetPageTitle()
        {
            return _pageTitle.Text;
        }

#pragma warning disable 649
        [FindsBy(How = How.CssSelector, Using = "input[placeholder='Username']")]
        private IWebElement _usernameField;

        [FindsBy(How = How.CssSelector, Using = "input[placeholder='Password']")]
        private IWebElement _passwordField;

        [FindsBy(How = How.XPath, Using = "//button[contains(., 'login')]")]
        private IWebElement _signInButton;

        [FindsBy(How = How.XPath, Using = "//a[@href='/en-US/Home/Register']")]
#pragma warning disable 169
        private IWebElement _registerButton;
#pragma warning restore 169

        [FindsBy(How = How.XPath, Using = "//div[@id='login-messages']//li")]
        private IWebElement _errorMsg;

        [FindsBy(How = How.XPath, Using = "//div[@id='login-messages']//li[1]")]
        private IWebElement _usernameErrorMsg;

        [FindsBy(How = How.XPath, Using = "//div[@id='login-messages']//li[2]")]
        private IWebElement _passwordErrorMsg;

        [FindsBy(How = How.CssSelector, Using = "h2")]
        private IWebElement _pageTitle;
#pragma warning restore 649

        public RegisterPage GoToRegisterPage()
        {
            var registerPage = new RegisterPage(_driver);
            registerPage.NavigateToMemberWebsite();
            return registerPage;
        }
    }
}