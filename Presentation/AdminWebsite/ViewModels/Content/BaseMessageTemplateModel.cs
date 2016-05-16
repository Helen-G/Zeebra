using AFT.RegoV2.Core.Common.Data.Content;
using AFT.RegoV2.Core.Content.Data;

namespace AFT.RegoV2.AdminWebsite.ViewModels.Content
{
    public abstract class BaseMessageTemplateModel
    {
        public string LanguageCode { get; set; }
        public MessageType MessageType { get; set; }
        public MessageDeliveryMethod MessageDeliveryMethod { get; set; }
        public string TemplateName { get; set; }
        public string SenderName { get; set; }
        public string SenderEmail { get; set; }
        public string Subject { get; set; }
        public string SenderNumber { get; set; }
        public string MessageContent { get; set; }
    }
}