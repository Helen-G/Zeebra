using System;
using System.Collections.Generic;
using AFT.RegoV2.Core.Common.Events.Brand;

namespace AFT.RegoV2.Core.Brand.Data
{
    public class VipLevel
    {
        public VipLevel()
        {
            VipLevelLimits = new List<VipLevelGameProviderBetLimit>();
        }

        public Guid             Id { get; set; }
        public Guid             BrandId { get; set; }
        public string           Code { get; set; }
        public string           Name { get; set; }
        public string           Description { get; set; }
        public int              Rank { get; set; }
        public string           ColorCode { get; set; }
        public string           CreatedBy { get; set; }
        public DateTimeOffset   DateCreated { get; set; }
        public string           UpdatedBy { get; set; }
        public string           UpdatedRemark { get; set; }
        public DateTimeOffset?  DateUpdated { get; set; }
        public VipLevelStatus   Status { get; set; }
        public Brand                            Brand { get; set; }
        public ICollection<VipLevelGameProviderBetLimit>    VipLevelLimits { get; set; } 
    }

    public class VipLevelId
    {
        private readonly Guid _id;

        public VipLevelId(Guid id)
        {
            _id = id;
        }

        public static implicit operator Guid(VipLevelId id)
        {
            return id._id;
        }

        public static implicit operator VipLevelId(Guid id)
        {
            return new VipLevelId(id);
        }
    }
}