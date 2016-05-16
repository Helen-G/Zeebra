using System;
using System.Collections.Generic;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Common.Events.Brand
{
    public class VipLevelRegistered : DomainEventBase
    {
        public Guid             Id { get; set; }
        public Guid             BrandId { get; set; }
        public string           Code { get; set; }
        public string           Name { get; set; }
        public int              Rank { get; set; }
        public string           Description { get; set; }
        public string           ColorCode { get; set; }
        public VipLevelStatus   Status { get; set; }
        public string           CreatedBy { get; set; }
        public DateTimeOffset   DateCreated { get; set; }

        public ICollection<VipLevelLimitData> VipLevelLimits { get; set; }
    }

    public class VipLevelLimitData
    {
        public Guid     Id { get; set; }
        public Guid     VipLevelId { get; set; }
        public Guid     GameProviderId { get; set; }
        public Guid     BetLimitId { get; set; }
        public string   CurrencyCode { get; set; }
    }

    public enum VipLevelStatus
    {
        Active,
        Inactive
    }
}