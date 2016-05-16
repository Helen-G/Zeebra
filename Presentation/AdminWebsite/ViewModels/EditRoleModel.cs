using System;
using System.Collections.Generic;

namespace AFT.RegoV2.AdminWebsite.ViewModels
{
    public class EditRoleModel
    {
        public Guid Id { get; set; }

        public string Code { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public Guid LicenseeId { get; set; }

        public IList<Guid> AssignedLicensees { get; set; }

        public IEnumerable<Guid> CheckedPermissions { get; set; }
    }
}