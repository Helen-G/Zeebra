using System;
using System.Threading;
using AFT.RegoV2.Core.Common.Utils;
using Newtonsoft.Json;

namespace AFT.RegoV2.Core.Common.Interfaces
{
    public abstract class DomainEventBase : IDomainEvent
    {
        protected DomainEventBase()
        {
            EventId = Identifier.NewSequentialGuid();
            EventCreated = Identifier.NewDateTimeOffset();
            EventCreatedBy = Thread.CurrentPrincipal.Identity.Name;
        }

        [JsonProperty]
        public Guid EventId { get; private set; }
        [JsonProperty]
        public DateTimeOffset EventCreated { get; private set; }
        [JsonProperty]
        public string EventCreatedBy { get; set; }
    }
}