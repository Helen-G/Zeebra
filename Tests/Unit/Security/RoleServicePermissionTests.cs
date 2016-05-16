using AFT.RegoV2.ApplicationServices.Security;
using AFT.RegoV2.Core.Security.ApplicationServices.Data.Roles;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Domain.BoundedContexts.Security.ApplicationServices;
using AFT.RegoV2.Tests.Common.Base;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Security
{
    class RoleServicePermissionTests : PermissionsTestsBase
    {
        private RoleService _roleService;

        public override void BeforeEach()
        {
            base.BeforeEach();

            _roleService = Container.Resolve<RoleService>();
        }

        [Test]
        public void Cannot_execute_RoleService_without_permissions()
        {
            /* Arrange */
            LogWithNewUser(Modules.PlayerManager, Permissions.View);

            /* Act */
            Assert.Throws<InsufficientPermissionsException>(() => _roleService.GetRoles());
            Assert.Throws<InsufficientPermissionsException>(() => _roleService.CreateRole(new AddRoleData()));
            Assert.Throws<InsufficientPermissionsException>(() => _roleService.UpdateRole(new EditRoleData()));
        }
    }
}
