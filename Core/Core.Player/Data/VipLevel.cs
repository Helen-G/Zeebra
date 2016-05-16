using System;
using AFT.RegoV2.Core.Common.Events.Brand;

namespace AFT.RegoV2.Core.Player.Data
{
    public class VipLevel
    {
        public Guid     Id { get; set; }
        public Guid     BrandId { get; set; }

        public string   Code { get; set; }
        public string   Name { get; set; }
        public int      Rank { get; set; }
        public string   Description { get; set; }
        public string   ColorCode { get; set; }
        public VipLevelStatus   Status { get; set; }
        public Brand    Brand { get; set; }
    }
}
