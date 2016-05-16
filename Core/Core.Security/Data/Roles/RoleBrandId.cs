using System;
using AFT.RegoV2.Core.Security.Data;

namespace AFT.RegoV2.Domain.BoundedContexts.Security.Data
{
    public class RoleBrandId
    {
        public Guid RoleId { get; set; }
        public Guid BrandId { get; set; }

        public Role Role { get; set; }
    }
}
