using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AFT.RegoV2.Core.Security.Data
{
    public class Permission
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Module { get; set; }
    }
}
