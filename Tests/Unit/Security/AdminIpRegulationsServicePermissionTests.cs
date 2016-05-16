using System;
using AFT.RegoV2.ApplicationServices.Security;
using AFT.RegoV2.Core.Security.ApplicationServices;
using AFT.RegoV2.Core.Security.ApplicationServices.Data.IpRegulations;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Tests.Common.Base;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Security
{
    class AdminIpRegulationsServicePermissionTests : PermissionsTestsBase
    {
        private BackendIpRegulationService _backendService;

        public override void BeforeEach()
        {
            base.BeforeEach();

            _backendService = Container.Resolve<BackendIpRegulationService>();
        }

        [Test]
        public void Cannot_execute_BackendIpRegulationService_without_permissions()
        {
            /* Arrange */
            LogWithNewUser(Modules.PlayerManager, Permissions.View);

            /* Act */
            Assert.Throws<InsufficientPermissionsException>(() => _backendService.GetIpRegulations());
            Assert.Throws<InsufficientPermissionsException>(() => _backendService.CreateIpRegulation(new AddBackendIpRegulationData()));
            Assert.Throws<InsufficientPermissionsException>(() => _backendService.UpdateIpRegulation(new EditBackendIpRegulationData()));
            Assert.Throws<InsufficientPermissionsException>(() => _backendService.DeleteIpRegulation(new Guid()));
        }
    }
}
