using System;
using AFT.RegoV2.Core.Services.Security;

namespace AFT.RegoV2.Core.Security.Data
{
    public class RoleLicenseeId
    {
        public Guid Id { get; set; }
        public Guid RoleId { get; set; }

        public Role Role { get; set; }
    }
}
