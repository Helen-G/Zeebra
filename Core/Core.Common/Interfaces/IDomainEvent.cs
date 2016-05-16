using System;

namespace AFT.RegoV2.Core.Common.Interfaces
{
    public interface IDomainEvent : IMessage
    {
        Guid            EventId { get; }
        DateTimeOffset  EventCreated { get; }
        string          EventCreatedBy { get; }
    }
}