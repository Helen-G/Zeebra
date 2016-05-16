using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AFT.RegoV2.Core.Security.Data;

namespace AFT.RegoV2.Core.Security.Helpers
{
    public static class RoleExtensions
    {
        public static void SetLicensees(this Role role, IEnumerable<Guid> licensees)
        {
            if (licensees == null) return;
            role.Licensees.Clear();

            foreach (var licensee in licensees)
            {
                var licenseeId = new RoleLicenseeId();
                licenseeId.Id = licensee;
                licenseeId.RoleId = role.Id;

                role.Licensees.Add(licenseeId);
            }
        }
    }
}
