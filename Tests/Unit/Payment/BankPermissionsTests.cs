using AFT.RegoV2.ApplicationServices.Security;
using AFT.RegoV2.Core.Payment.ApplicationServices;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Tests.Common.Base;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Payment
{
    internal class BankPermissionsTests : PermissionsTestsBase
    {
        private BankCommands _bankCommands;

        public override void BeforeEach()
        {
            base.BeforeEach();

            _bankCommands = Container.Resolve<BankCommands>();
        }

        [Test]
        public void Cannot_execute_BankCommands_without_permissions()
        {
            // Arrange
            LogWithNewUser(Modules.Banks, Permissions.View);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _bankCommands.Save(new SaveBankData()));
        }
    }
}