using System.Linq;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.MemberApi.Interface;
using AFT.RegoV2.MemberApi.Interface.Player;
using AFT.RegoV2.MemberApi.Interface.Proxy;
using AFT.RegoV2.Shared;
using AFT.RegoV2.Tests.Common;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Player
{
    internal class ChangePasswordTests : PlayerServiceTestsBase
    {
        private RegisterRequest _registrationData;

        public override void BeforeEach()
        {
            base.BeforeEach();

            _registrationData = RegisterPlayer().Result;
        }


        [Test]
        public async void Can_change_password()
        {
            var initialPasswordValue = FakePlayerRepository.Players.Single().PasswordEncrypted;
            await PlayerWebservice.ChangePasswordAsync(new ChangePasswordRequest
            {
                Username = _registrationData.Username,
                OldPassword = _registrationData.Password,
                NewPassword = "newPa$$word"
            });

            var changedPasswordValue = FakePlayerRepository.Players.Single().PasswordEncrypted;
            //Assert.False(result.IsErrorResponse());
            Assert.AreNotEqual(initialPasswordValue, changedPasswordValue);
        }

        [Test]
        public void Empty_username_validation_works()
        {
            var e = Assert.Throws<MemberApiProxyException>(
                        async () => await PlayerWebservice.ChangePasswordAsync(new ChangePasswordRequest { Username = string.Empty, OldPassword = _registrationData.Password, NewPassword = _registrationData.Password })).Exception;

            Assert.AreEqual(PlayerAccountResponseCode.UsernameShouldNotBeEmpty.ToString(), e.ErrorCode);
        }

        [Test]
        public void Empty_password_validation_works()
        {
            var e = Assert.Throws<MemberApiProxyException>(
                        async () => await PlayerWebservice.ChangePasswordAsync(new ChangePasswordRequest { Username = _registrationData.Username, OldPassword = _registrationData.Password, NewPassword = string.Empty })).Exception;

            Assert.AreEqual(PlayerAccountResponseCode.PasswordShouldNotBeEmpty.ToString(), e.ErrorCode);
        }

        [Test]
        [TestCase("short")]
        [TestCase("too_long_to_be_real_password")]
        public void Password_length_validation_works(string password)
        {
            var e = Assert.Throws<MemberApiProxyException>(
                    async () => await PlayerWebservice.ChangePasswordAsync(new ChangePasswordRequest { Username = _registrationData.Username, OldPassword = _registrationData.Password, NewPassword = password })).Exception;

            Assert.IsNotEmpty(e.ErrorMessage);
        }

        [Test]
        public void Player_is_absent_validation_works()
        {
            var e = Assert.Throws<MemberApiProxyException>(
                    async () => await PlayerWebservice.ChangePasswordAsync(new ChangePasswordRequest { Username = "random_username", OldPassword = _registrationData.Password, NewPassword = "random_pass" })).Exception;

            Assert.IsNotEmpty(e.ErrorMessage);
            Assert.AreEqual(PlayerAccountResponseCode.PlayerDoesNotExist.ToString(), e.ErrorCode);
        }

        [Test]
        public void Old_password_validation_works()
        {
            var newPassword = TestDataGenerator.GetRandomString();
            string oldPasswordToEnter;
            do
            {
                oldPasswordToEnter = TestDataGenerator.GetRandomString();
            }
            while (oldPasswordToEnter == _registrationData.Password);

            var e = Assert.Throws<MemberApiProxyException>(
                        async () => await PlayerWebservice.ChangePasswordAsync(new ChangePasswordRequest { Username = _registrationData.Username, OldPassword = oldPasswordToEnter, NewPassword = newPassword })).Exception;

            Assert.IsNotEmpty(e.ErrorMessage);
            Assert.AreEqual(PlayerAccountResponseCode.UsernamePasswordCombinationIsNotValid.ToString(), e.ErrorCode);
        }
    }
}
