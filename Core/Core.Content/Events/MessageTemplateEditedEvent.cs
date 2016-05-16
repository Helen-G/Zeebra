using System;
using AFT.RegoV2.Core.Common.Data.Content;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Content.Data;

namespace AFT.RegoV2.Core.Content.Events
{
    public class MessageTemplateEditedEvent : DomainEventBase
    {
        public Guid Id { get; set; }
        public Guid BrandId { get; set; }
        public MessageType MessageType { get; set; }
        public string Name { get; set; }
        public string LanguageCode { get; set; }
        public string Content { get; set; }
        public DateTimeOffset Updated { get; set; }
        public string UpdatedBy { get; set; }

        public MessageTemplateEditedEvent() { }

        public MessageTemplateEditedEvent(MessageTemplate messageTemplate)
        {
            Id = messageTemplate.Id;
            BrandId = messageTemplate.BrandId;
            MessageType = messageTemplate.MessageType;
            Name = messageTemplate.TemplateName;
            LanguageCode = messageTemplate.LanguageCode;
            Content = messageTemplate.MessageContent;
            Updated = messageTemplate.Created;
            UpdatedBy = messageTemplate.CreatedBy;
        }
    }
}