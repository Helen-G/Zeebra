using System;
using AFT.RegoV2.Core.Common.Data.Content;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Content.Data;

namespace AFT.RegoV2.Core.Content.Events
{
    public class MessageTemplateAddedEvent : DomainEventBase
    {
        public Guid Id { get; set; }
        public Guid BrandId { get; set; }
        public MessageType MessageType { get; set; }
        public string Name { get; set; }
        public string LanguageCode { get; set; }
        public string Content { get; set; }
        public DateTimeOffset Created { get; set; }
        public string CreatedBy { get; set; }

        public MessageTemplateAddedEvent(){}

        public MessageTemplateAddedEvent(MessageTemplate messageTemplate)
        {
            Id = messageTemplate.Id;
            BrandId = messageTemplate.BrandId;
            MessageType = messageTemplate.MessageType;
            Name = messageTemplate.TemplateName;
            LanguageCode = messageTemplate.LanguageCode;
            Content = messageTemplate.MessageContent;
            Created = messageTemplate.Created;
            CreatedBy = messageTemplate.CreatedBy;
        }
    }
}