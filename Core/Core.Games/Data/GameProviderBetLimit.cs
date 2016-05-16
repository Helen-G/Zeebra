using System;
using System.Collections.Generic;

namespace AFT.RegoV2.Core.Game.Data
{
    public class GameProviderBetLimit
    {
        public Guid Id { get; set; }

        public Guid BrandId { get; set; } 
        public Guid GameProviderId { get; set; } 

        public string Code { get; set; } 
        public string Description { get; set; } 
        public string Name { get; set; } 

        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }

        public DateTimeOffset   DateCreated { get; set; }
        public DateTimeOffset?  DateUpdated { get; set; }

        public GameProvider               GameProvider { get; set; }
        public ICollection<VipLevel>    VipLevels { get; set; }
    }
}