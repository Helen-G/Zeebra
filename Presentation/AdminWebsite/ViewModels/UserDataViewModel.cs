using System;
using System.Collections.Generic;
using AFT.RegoV2.Core.Security.Data;
using AFT.RegoV2.Core.Services.Security;

namespace AFT.RegoV2.AdminWebsite.ViewModels
{
    public class SecurityViewModel
    {
        public string UserName { get; set; }

        public IEnumerable<Permission> Operations { get; set; }

        public IEnumerable<Guid> UserPermissions { get; set; }

        public IEnumerable<Guid> Licensees { get; set; }

        public bool IsSingleBrand { get; set; }

        public bool IsSuperAdmin { get; set; }

        public IDictionary<string, string> Permissions { get; set; }

        public IDictionary<string, string> Categories { get; set; }
    }
}