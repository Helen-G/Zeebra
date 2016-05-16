using System;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Game.Data;

namespace AFT.RegoV2.Core.Game.Events
{
    public class BetLimitDeleted : DomainEventBase
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public Guid BrandId { get; set; }

        public BetLimitDeleted()
        {
            
        }

        public BetLimitDeleted(GameProviderBetLimit betLimit)
        {
            Name = betLimit.Name;
            Code = betLimit.Code;
            Description = betLimit.Description;
            BrandId = betLimit.BrandId;
        }
    }
}
