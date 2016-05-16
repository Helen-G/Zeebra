using System;
using AFT.RegoV2.Core.Services.Security;

namespace AFT.RegoV2.Core.Security.Data
{
    public class RolePermission
    {
        public Guid RoleId { get; set; }

        public Guid PermissionId { get; set; }

        public Role Role { get; set; }
    }
}
