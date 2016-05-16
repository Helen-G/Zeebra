using System;

namespace AFT.RegoV2.Core.Security.Data
{
    public class IpRegulationBase
    {
        public Guid Id { get; set; }

        public string IpAddress { get; set; }

        public string Description { get; set; }

        public User CreatedBy { get; set; }

        public DateTimeOffset? CreatedDate { get; set; }

        public User UpdatedBy { get; set; }

        public DateTimeOffset? UpdatedDate { get; set; }
    }
}

