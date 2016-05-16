using AFT.RegoV2.MemberApi.Interface.Account;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Integration
{
    internal class LoginTests : PlayerServiceTestsBase
    {
        private RegisterRequest _registrationData;

        public override void BeforeEach()
        {
            base.BeforeEach();

            _registrationData = RegisterPlayerAndLogin();
        }

        [Test]
        public void Can_login_Player()
        {
            ServiceProxy.Login(new LoginRequest { Username = _registrationData.Username, Password = _registrationData.Password });
        }
    }
}