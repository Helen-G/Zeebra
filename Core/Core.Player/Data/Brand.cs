using System;
using System.Collections.Generic;

namespace AFT.RegoV2.Core.Player.Data
{
    public class Brand
    {
        public Guid     Id { get; set; }
        public string   Name { get; set; }
        public Guid?    DefaultVipLevelId { get; set; }

        public VipLevel DefaultVipLevel { get; set; }

        public ICollection<VipLevel> VipLevels { get; set; }
    }
}