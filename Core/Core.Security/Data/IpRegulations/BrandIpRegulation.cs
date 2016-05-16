using System;

namespace AFT.RegoV2.Core.Security.Data
{
    public class BrandIpRegulationId
    {
        private readonly Guid _id;

        public BrandIpRegulationId(Guid id)
        {
            _id = id;
        }

        public static implicit operator Guid(BrandIpRegulationId id)
        {
            return id._id;
        }

        public static implicit operator BrandIpRegulationId(Guid id)
        {
            return new BrandIpRegulationId(id);
        }
    }

    public class BrandIpRegulation : IpRegulationBase
    {
        public Guid LicenseeId { get; set; }

        public Guid BrandId { get; set; }

        public string RedirectionUrl { get; set; }

        public string BlockingType { get; set; }
    }
}

