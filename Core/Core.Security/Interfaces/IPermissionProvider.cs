using System.Collections.Generic;
using AFT.RegoV2.Core.Security.Data;
using AFT.RegoV2.Core.Services.Security;

namespace AFT.RegoV2.Domain.Security.Interfaces
{
    public interface IPermissionProvider
    {
        IEnumerable<Permission> GetPermissions();
        Permission GetRootPermission();
        Permission GetPermission(string permission, string module = null);
        RoleOperation CreateRolePermission(Permission permission);
        RoleOperation GetRolePermission(string name, string parent = null);
        IEnumerable<RoleOperation> CreateRolePermissions(IEnumerable<Permission> operations);
        IEnumerable<Permission> GetPermissionsFromRole(IEnumerable<RoleOperation> rolePermissions);
        bool RegisterPermissionUnlessExists(string name, string category);
        IEnumerable<Permission> BuildPermissionsList(IEnumerable<Permission> rootPermissions);
    }
}
