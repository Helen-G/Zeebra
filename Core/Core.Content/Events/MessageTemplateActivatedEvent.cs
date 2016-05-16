using System;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Content.Events
{
    public class MessageTemplateActivatedEvent : DomainEventBase
    {
        public Guid Id { get; set; }

        public MessageTemplateActivatedEvent(){}

        public MessageTemplateActivatedEvent(Guid id)
        {
            Id = id;
        }
    }
}