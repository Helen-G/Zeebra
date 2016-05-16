using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Tests.Common.Helpers;
using AFT.RegoV2.MemberApi.Interface;
using AFT.RegoV2.MemberApi.Interface.Player;
using AFT.RegoV2.MemberApi.Interface.Proxy;
using AFT.RegoV2.Shared;
using FluentAssertions;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Player
{
    internal class LoginTests : PlayerServiceTestsBase
    {
        private RegisterRequest _registrationData;

        public override void BeforeEach()
        {
            base.BeforeEach();

            _registrationData = RegisterPlayer(false).Result;
        }

        [Test]
        public void Can_login_Player()
        {
            var result = PlayerWebservice.Login(CreateLoginRequest(_registrationData.Username, _registrationData.Password));

            //Assert.False(result.IsErrorResponse(), result.ResponseStatus.Message);
            //Assert.That(result.SessionId, Is.Not.EqualTo(Guid.Empty), "SessionId should not be empty");
        }

        [Test]
        public async void Unable_to_login_if_username_does_not_exist()
        {
            var e = await AsyncTestHelper.ThrowsAsync<MemberApiProxyException>(
                () => PlayerWebservice.Login(CreateLoginRequest("notExistingUsername", _registrationData.Password )));

            Assert.That(e.Exception.ErrorMessage, Is.Not.Empty);
            Assert.That(e.Exception.ErrorCode, Is.EqualTo(PlayerAccountResponseCode.UsernamePasswordCombinationIsNotValid.ToString()));
        }

        [Test]
        public async void Unable_to_login_if_password_is_incorrect()
        {
            var e = await AsyncTestHelper.ThrowsAsync<MemberApiProxyException>(
                () => PlayerWebservice.Login(CreateLoginRequest (_registrationData.Username, "some invalid password" )));

            Assert.That(e.Exception.ErrorMessage, Is.Not.Empty);
            Assert.That(e.Exception.ErrorCode, Is.EqualTo(PlayerAccountResponseCode.UsernamePasswordCombinationIsNotValid.ToString()));
        }

        [Test]
        public async void Unable_to_login_if_username_or_password_is_empty()
        {
            var e = await AsyncTestHelper.ThrowsAsync<MemberApiProxyException>(
                () => PlayerWebservice.Login(CreateLoginRequest(string.Empty, _registrationData.Password)));

            Assert.That(e.Exception.ErrorMessage, Is.Not.Empty);
            Assert.That(e.Exception.ErrorCode, Is.EqualTo(PlayerAccountResponseCode.UsernameShouldNotBeEmpty.ToString()));

            e = await AsyncTestHelper.ThrowsAsync<MemberApiProxyException>(
                () => PlayerWebservice.Login(CreateLoginRequest(_registrationData.Username, string.Empty)));

            Assert.That(e.Exception.ErrorMessage, Is.Not.Empty);
            Assert.That(e.Exception.ErrorCode, Is.EqualTo(PlayerAccountResponseCode.PasswordShouldNotBeEmpty.ToString()));
        }

        [Test]
        public async void Unable_to_login_after_max_failed_login_attempts()
        {
            var maxFailedLoginAttempts = Convert.ToInt32(ConfigurationManager.AppSettings["MaxFailedLoginAttempts"]);
            var newPlayer = await RegisterPlayer(false);

            var wrongLoginRequest = CreateLoginRequest(newPlayer.Username, "wrongPassword");

            for (var i = 0; i < maxFailedLoginAttempts; i++)
            {
                var passwordError = await AsyncTestHelper.ThrowsAsync<MemberApiProxyException>(
                    () => PlayerWebservice.Login(wrongLoginRequest));

                Assert.That(passwordError.Exception.ErrorMessage, Is.Not.Empty);
                Assert.That(passwordError.Exception.ErrorCode, Is.EqualTo(PlayerAccountResponseCode.UsernamePasswordCombinationIsNotValid.ToString()));
            }

            var lockedError = await AsyncTestHelper.ThrowsAsync<MemberApiProxyException>(
                () => PlayerWebservice.Login(CreateLoginRequest(newPlayer.Username, newPlayer.Password)));

            Assert.That(lockedError.Exception.ErrorMessage, Is.Not.Empty);
            Assert.That(lockedError.Exception.ErrorCode, Is.EqualTo(PlayerAccountResponseCode.AccountLocked.ToString()));
        }

        private LoginRequest CreateLoginRequest(string username, string password)
        {
            return new LoginRequest
            {
                Username = username,
                Password = password,
                BrandId = new Guid("00000000-0000-0000-0000-000000000138"),
                IPAddress = "::1",
                RequestHeaders = new Dictionary<string, string>()
            };
        }

        //[Test, Ignore("Not implemented yet")]
        //public void Able_to_login_after_session_timeout()
        //{
        //    // TODO: abstract the way service knows about time (create DateTimeProvider) and test that after timeout player is able to login again 
        //}
    }
}