using System;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Extensions;
using AFT.RegoV2.Tests.Common.Pages.FrontEnd;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Selenium
{
    internal class PlayerProfileOnMemberWebsiteTests : SeleniumBaseForMemberWebsite
    {
        private RegisterPage _registerPage;
        private MemberWebsiteLoginPage _loginPage;
        private PlayerProfilePage _playerProfilePage;
        private static string _correctUsername;
        private static string _correctPassword;
        private RegistrationDataForMemberWebsite _data;

        public override void BeforeEach()
        {
            base.BeforeEach();
            _driver.Logout();
            _data = TestDataGenerator.CreateValidPlayerDataForMemberWebsite();
            _correctUsername = _data.Username;
            _correctPassword = _data.Password;
            _registerPage = new RegisterPage(_driver);
            _registerPage.NavigateToMemberWebsite();
            _registerPage.Register(_data);
            
            _loginPage = new MemberWebsiteLoginPage(_driver);
            _loginPage.NavigateToMemberWebsite();
            _driver.WaitForJavaScript();
            _playerProfilePage = _loginPage.Login(_correctUsername, _correctPassword);
            _driver.WaitForJavaScript();
            _playerProfilePage.ClearFields();
        }

        [Test]
        public void Can_view_player_info()
        {
            Assert.AreEqual(_data.Username, _playerProfilePage.Username.Text);
            Assert.AreEqual(_data.FirstName, _playerProfilePage.FirstName.Text);
            Assert.AreEqual(_data.LastName, _playerProfilePage.LastName.Text);

            var dateString = _data.Year.ToString("####") + "/" + _data.Month.ToString("##") + "/" + _data.Day.ToString("##");
            
            Assert.AreEqual(dateString, _playerProfilePage.DateOfBirth.Text);
            Assert.AreEqual(_data.PhoneNumber, _playerProfilePage.PhoneNumber.Text);
            Assert.AreEqual(_data.Email, _playerProfilePage.Email.Text);
            Assert.AreEqual(_data.Address, _playerProfilePage.Address.Text);
            Assert.AreEqual(_data.PostalCode, _playerProfilePage.ZipCode.Text);
            Assert.AreEqual(_data.Currency, _playerProfilePage.CurrencyCode.Text);
            Assert.AreEqual(_data.Country, _playerProfilePage.CountryCode.Text);
        }
        
        [Test]
        public void Can_change_password()
        {
            _playerProfilePage.ChangePassword();
            Assert.AreEqual(_playerProfilePage.GetPasswordChangedMsg(), "Password has been changed successfully.");

            _driver.Manage().Cookies.DeleteAllCookies();
            _loginPage = new MemberWebsiteLoginPage(_driver);
            _loginPage.NavigateToMemberWebsite();
            _driver.WaitForJavaScript();
            var currentPage = _playerProfilePage = _loginPage.Login(_correctUsername, "new-password");
            Assert.That(currentPage.GetUserName(), Is.StringContaining(_correctUsername));
        }

        [Test]
        public void Cannot_change_password_to_short_one()
        {
            var password = TestDataGenerator.GetRandomStringWithSpecialSymbols(5, ".-'");
            _playerProfilePage.EnterNewPassword(password);
            Assert.That(_playerProfilePage.GetPasswordChangedErrorMsg(), Is.StringContaining("PasswordIsNotWithinItsAllowedRange"));
        }

        [Test]
        public void Cannot_change_password_to_long_one()
        {
            var password = TestDataGenerator.GetRandomStringWithSpecialSymbols(13, ".-'");
            _playerProfilePage.EnterNewPassword(password);
            Assert.That(_playerProfilePage.GetPasswordChangedErrorMsg(), Is.StringContaining("PasswordIsNotWithinItsAllowedRange"));
        }

        [Test]
        public void Cannot_change_password_without_old_one()
        {
            _playerProfilePage.EnterOnlyNewPassword();
            Assert.AreEqual("PasswordShouldNotBeEmpty", _playerProfilePage.GetPasswordChangedErrorMsg());
        }

        [Test]
        public void Cannot_change_password_without_confirming_passward()
        {
            _playerProfilePage.EnterNewPasswordWithoutConfirmPassword();
            Assert.AreEqual("Passwords do not match.", _playerProfilePage.GetPasswordChangedErrorMsg());
        }

        [Test]
        public void Cannot_change_password_with_incorrect_confirm_password()
        {
            _playerProfilePage.EnterNewPasswordWithIncorrectConfirmPassword();
            Assert.AreEqual("Passwords do not match.", _playerProfilePage.GetPasswordChangedErrorMsg());
        }

        [Test]
        public void Can_request_mobile_verification_code()
        {
            try
            {
                _playerProfilePage.RequestMobileVerificationCode();
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }
    }
}
