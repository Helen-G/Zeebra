using System;
using AFT.RegoV2.ApplicationServices.Security;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Brand.Validators;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Tests.Common.Base;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Brand
{
    class LicenseePermissionsTests : PermissionsTestsBase
    {
        private LicenseeCommands _licenseeCommands;
        private LicenseeQueries _licenseeQueries;

        public override void BeforeEach()
        {
            base.BeforeEach();

            _licenseeCommands = Container.Resolve<LicenseeCommands>();
            _licenseeQueries = Container.Resolve<LicenseeQueries>();
        }

        [Test]
        public void Cannot_execute_LicenseeCommands_without_permissions()
        {
            /* Arrange */
            LogWithNewUser(Modules.LicenseeManager, Permissions.View);

            /* Act */
            Assert.Throws<InsufficientPermissionsException>(() => _licenseeCommands.Add(new AddLicenseeData()));
            Assert.Throws<InsufficientPermissionsException>(() => _licenseeCommands.Edit(new EditLicenseeData()));
            Assert.Throws<InsufficientPermissionsException>(() => _licenseeCommands.RenewContract(new Guid(), "Some string", "Some another string"));
            Assert.Throws<InsufficientPermissionsException>(() => _licenseeCommands.Activate(new Guid(), "Some remark"));
            Assert.Throws<InsufficientPermissionsException>(() => _licenseeCommands.Deactivate(new Guid(), "Some remark"));
        }

        [Test]
        public void Cannot_execute_LicenseeQueries_without_permissions()
        {
            /* Arrange */
            LogWithNewUser(Modules.LicenseeManager, Permissions.View);

            /* Act */
            Assert.Throws<InsufficientPermissionsException>(() => _licenseeQueries.GetRenewContractData(new Guid()));
        }
    }
}
