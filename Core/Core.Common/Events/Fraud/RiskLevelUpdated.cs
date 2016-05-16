using System;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Common.Events.Fraud
{
    public class RiskLevelUpdated : DomainEventBase
    {
        public Guid Id { get; set; }
        public Guid BrandId { get; set; }
        public int Level { get; set; }
        public string Name { get; set; }
        public Status Status { get; set; }
        public string Description { get; set; }

        public RiskLevelUpdated()
        { }

        public RiskLevelUpdated(Guid id, Guid brandId, int level, string name, Status status, string description)
        {
            Id = id;
            BrandId = brandId;
            Level = level;
            Name = name;
            Status = status;
            Description = description;
        }
    }
}
