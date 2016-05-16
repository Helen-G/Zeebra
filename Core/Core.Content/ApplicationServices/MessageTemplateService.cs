using System;
using AFT.RegoV2.Core.Common.Interfaces;
using RazorTemplates.Core;

namespace AFT.RegoV2.Core.Content.ApplicationServices
{
    public class MessageTemplateService : IMessageTemplateService
    {
        private readonly IContentRepository _repository;
        
        public MessageTemplateService(IContentRepository repository)
        {
            _repository = repository;
        }

        public string GetMessage<T>(Guid templateId, T model)
        {
            var messageTemplate = _repository.FindMessageTemplateById(templateId);
            if (messageTemplate == null)
                return null;

            var template = Template.Compile(messageTemplate.MessageContent);
            var message = template.Render(model);

            return message;
        }
    }
}