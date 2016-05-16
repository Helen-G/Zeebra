using System;
using System.Threading;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Extensions;
using AFT.RegoV2.Tests.Common.Helpers;
using AFT.RegoV2.Tests.Common.Pages.FrontEnd;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Selenium
{
    internal class PlayerLoginTests : SeleniumBaseForMemberWebsite
    {
        private MemberWebsiteLoginPage _loginPage;
        private RegistrationDataForMemberWebsite _data;
        private static string _correctUsername;
        private static string _correctPassword;
        private static string _incorrectUsername;
        private static string _incorrectPassword;

        public override void BeforeAll()
        {
            base.BeforeAll();
            _data = _container.Resolve<PlayerTestHelper>().CreatePlayerForMemberWebsite();
            _correctUsername = _data.Username;
            _correctPassword = _data.Password;
            _incorrectUsername = _correctUsername + 1;
            _incorrectPassword = _correctPassword + 1;
        }

        public override void BeforeEach()
        {
            base.BeforeEach();
            _driver.Manage().Cookies.DeleteAllCookies();
            _loginPage = new MemberWebsiteLoginPage(_driver);
            _loginPage.NavigateToMemberWebsite();
            _driver.WaitForJavaScript();
        }

        [Test]
        public void Can_login_on_member_website()
        {
            var currentPage = _loginPage.Login(_correctUsername, _correctPassword);
            Assert.That(currentPage.GetUserName(), Is.StringContaining(_correctUsername));
        }

        [Test]
        public void Cannot_login_with_empty_data()
        {
            _loginPage.ClickSignInButton();
            Thread.Sleep(TimeSpan.FromSeconds(1)); // Wait for message box fade in animation

            Assert.That(_loginPage.GetUsernameErrorMsg(), Is.StringContaining("UsernameShouldNotBeEmpty"));
            Assert.That(_loginPage.GetPasswordErrorMsg(), Is.StringContaining("PasswordShouldNotBeEmpty"));
        }

        [Test]
        public void Cannot_login_with_invalid_credentials()
        {
            _loginPage.TryToLogin(_incorrectUsername, _incorrectPassword);
            var errorMsg = _loginPage.GetErrorMsg();

            Assert.That(errorMsg, Is.StringContaining("UsernamePasswordCombinationIsNotValid"));
        }

        [Test]
        public void Cannot_login_with_invalid_username()
        {
            _loginPage.TryToLogin(_incorrectUsername, _correctPassword);
            var errorMsg = _loginPage.GetErrorMsg();

            Assert.That(errorMsg, Is.StringContaining("UsernamePasswordCombinationIsNotValid"));
        }

        [Test]
        public void Cannot_login_with_invalid_password()
        {
            _loginPage.TryToLogin(_correctUsername, _incorrectPassword);
            var errorMsg = _loginPage.GetErrorMsg();

            Assert.That(errorMsg, Is.StringContaining("UsernamePasswordCombinationIsNotValid"));
        }
    }
}
