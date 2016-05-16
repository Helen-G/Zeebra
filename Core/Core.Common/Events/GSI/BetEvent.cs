using System;
using AFT.RegoV2.Core.Common.Interfaces;
using Newtonsoft.Json;

namespace AFT.RegoV2.Core.Common.Events.Games
{
    public abstract class BetEvent : DomainEventBase
    {
        public Guid Id { get; set; }

        [JsonProperty]
        public Guid PlayerId { get; set; }
        [JsonProperty]
        public Guid BrandId { get; set; }
        [JsonProperty]
        public Guid GameId { get; set; }
        [JsonProperty]
        public Guid BetId { get; set; }
        [JsonProperty]
        public Guid GameActionId { get; set; }
        [JsonProperty]
        public DateTimeOffset CreatedOn { get; set; }
        public decimal Amount { get; set; }
        [JsonProperty]
        public decimal AdjustedAmount { get; set; }
        [JsonProperty]
        public decimal WonAmount { get; set; }
    }
}