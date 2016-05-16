using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Extensions;
using AFT.RegoV2.Tests.Common.Pages.BackEnd;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Selenium
{
    internal class AdminLoginTests : SeleniumBaseForAdminWebsite
    {
        private AdminWebsiteLoginPage _loginPage;
        private static string _correctUsername;
        private static string _correctPassword;
        private static string _incorrectUsername;
        private static string _incorrectPassword;

        public override void BeforeAll()
        {
            base.BeforeAll();
            _correctUsername = "SuperAdmin";
            _correctPassword = "SuperAdmin";
            _incorrectUsername = "SuperAdmin1";
            _incorrectPassword = "SuperAdmin2";
        }

        public override void BeforeEach()
        {
            base.BeforeEach();
            _loginPage = new AdminWebsiteLoginPage(_driver);
            _loginPage.NavigateToAdminWebsite();
        }

        [Test]
        public void Cannot_login_with_empty_data()
        {
            _loginPage.ClearFields();
            _loginPage.ClickLoginButton();
            var usernameErrorMsg = _loginPage.GetUsernameErrorMsg();
            var passwordErrorMsg = _loginPage.GetPasswordErrorMsg();

            Assert.That(usernameErrorMsg, Is.StringContaining("Username is required"));
            Assert.That(passwordErrorMsg, Is.StringContaining("Password is required"));
        }

        [Test]
        public void Cannot_login_with_invalid_credentials()
        {
            _loginPage.LoginWithIncompleteData(_incorrectUsername, _incorrectPassword);
            var errorMsg = _loginPage.GetUsernameErrorMsg();

            Assert.That(errorMsg, Is.StringContaining("Incorrect Username or Password"));
            Assert.That(_driver.Uri().ToString(), Is.StringContaining(_loginPage.Url.ToString()));
        }

        [Test]
        public void Cannot_login_with_invalid_username()
        {
            _loginPage.LoginWithIncompleteData(_incorrectUsername, _correctPassword);
            var errorMsg = _loginPage.GetUsernameErrorMsg();

            Assert.That(errorMsg, Is.StringContaining("Incorrect Username or Password"));
        }

        [Test]
        public void Cannot_login_with_invalid_password()
        {
            _loginPage.LoginWithIncompleteData(_correctUsername, _incorrectPassword);
            var errorMsg = _loginPage.GetUsernameErrorMsg();

            Assert.That(errorMsg, Is.StringContaining("Incorrect Username or Password"));
        }
    }
}
