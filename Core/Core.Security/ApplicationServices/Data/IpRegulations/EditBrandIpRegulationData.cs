using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AFT.RegoV2.Core.Security.ApplicationServices.Data.IpRegulations
{
    public class EditBrandIpRegulationData : IpRegulationDataBase
    {
        public Guid Id { get; set; }

        public Guid LicenseeId { get; set; }

        public string RedirectionUrl { get; set; }

        public string BlockingType { get; set; }

        public Guid BrandId { get; set; }
    }
}
