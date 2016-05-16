using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AFT.RegoV2.Core.Services.Security;

namespace AFT.RegoV2.Core.Security.ApplicationServices.Data.IpRegulations
{
    public class IpRegulationDataBase
    {
        public string IpAddress { get; set; }

        public string Description { get; set; }
    }
}
