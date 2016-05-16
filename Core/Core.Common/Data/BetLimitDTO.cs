using System;

namespace AFT.RegoV2.Core.Common.Data
{
    public class BetLimitDTO
    {
        public Guid Id { get; set; }
        public Guid GameProviderId { get; set; }
        public string LimitId { get; set; }
        public int Min { get; set; }
        public int Max { get; set; }
        public string Name { get; set; }
        public string FullName { get; set; }
        public string Description { get; set; }
    }
}