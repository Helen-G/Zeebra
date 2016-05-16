using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AFT.RegoV2.Core.Security.ApplicationServices.Data.Roles
{
    public class RoleDataBase
    {
        public string Code { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public Guid LicenseeId { get; set; }

        public IList<Guid> AssignedLicensees { get; set; }

        public IList<Guid> CheckedPermissions { get; set; }
    }
}
