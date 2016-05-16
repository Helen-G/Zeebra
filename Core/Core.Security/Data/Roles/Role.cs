using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Core.Security.Data;
using AFT.RegoV2.Domain.BoundedContexts.Security.Data;
using AFT.RegoV2.Domain.Security.Interfaces;

namespace AFT.RegoV2.Core.Security.Data
{
    public class Role
    {
        public Role()
        {
            Licensees = new List<RoleLicenseeId>();
            Permissions = new List<RolePermission>();
        }

        public Guid Id { get; set; }

        public string Code { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public ICollection<RoleLicenseeId> Licensees { get; set; }

        public User CreatedBy { get; set; }

        public DateTimeOffset CreatedDate { get; set; }

        public User UpdatedBy { get; set; }

        public DateTimeOffset? UpdatedDate { get; set; }

        public bool IsSuperAdmin { get; set; }

        public ICollection<RolePermission> Permissions { get; set; }
    }
}
