using AFT.RegoV2.MemberApi.Interface.Account;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Integration
{
    internal class ChangePasswordTests : PlayerServiceTestsBase
    {
        private RegisterRequest _registrationData;

        public override void BeforeEach()
        {
            base.BeforeEach();

            _registrationData = RegisterPlayerAndLogin();
        }

        [Test]
        public void Can_change_password()
        {
            ServiceProxy.Login(new LoginRequest { Username = _registrationData.Username, Password = _registrationData.Password });

            var response = ServiceProxy.ChangePassword(new ChangePasswordRequest
            {
                Username = _registrationData.Username,
                OldPassword = _registrationData.Password,
                NewPassword = "newPa$$word"
            });

            
        }
    }
}
