using System;
using System.Data.Entity;
using System.Linq;
using AFT.RegoV2.Core.Common.Data.Content;
using AFT.RegoV2.Core.Content;
using AFT.RegoV2.Core.Content.ApplicationServices;
using AFT.RegoV2.Core.Content.Data;
using AFT.RegoV2.Core.Content.Validators;
using AutoMapper;

namespace AFT.RegoV2.Tests.Common.Helpers
{
    public class ContentTestHelper
    {
        private readonly IContentRepository _contentRepository;
        private readonly IMessageTemplatesCommands _messageTemplatesCommands;
        private readonly IMessageTemplatesQueries _messageTemplatesQueries;

        static ContentTestHelper()
        {
            Mapper.CreateMap<AddMessageTemplateData, EditMessageTemplateData>();
        }

        public ContentTestHelper(
            IContentRepository contentRepository, 
            IMessageTemplatesCommands messageTemplatesCommands, 
            IMessageTemplatesQueries messageTemplatesQueries)
        {
            _contentRepository = contentRepository;
            _messageTemplatesCommands = messageTemplatesCommands;
            _messageTemplatesQueries = messageTemplatesQueries;
        }

        public MessageTemplate CreateMessageTemplate(
            bool activate = true,
            Guid? brandId = null,
            string cultureCode = null,
            MessageType? messageType = null,
            MessageDeliveryMethod? messageDeliveryMethod = null,
            string content = null)
        {
            var addMessageTemplateData = CreateAddMessageTemplateData(
                brandId,
                cultureCode,
                messageType,
                messageDeliveryMethod,
                content);

            var id = _messageTemplatesCommands.Add(addMessageTemplateData);

            if (activate)
                _messageTemplatesCommands.Activate(new ActivateMessageTemplateData { Id = id });

            return _contentRepository.MessageTemplates
                .Include(x => x.Brand)
                .Include(x => x.Language)
                .Single(x => x.Id == id);
        }

        public AddMessageTemplateData CreateAddMessageTemplateData(
            Guid? brandId = null,
            string cultureCode = null,
            MessageType? messageType = null,
            MessageDeliveryMethod? messageDeliveryMethod = null,
            string content = null)
        {
            var brand = brandId.HasValue
                ? _contentRepository.Brands.Include(x => x.Languages).Single(x => x.Id == brandId.Value)
                : _contentRepository.Brands.Include(x => x.Languages).First();

            messageDeliveryMethod = messageDeliveryMethod ?? TestDataGenerator.GetRandomMessageDeliveryMethod();

            var addMessageTemplateData = new AddMessageTemplateData
            {
                BrandId = brand.Id,
                LanguageCode = cultureCode ?? brand.Languages.First().Code,
                MessageType = messageType ?? TestDataGenerator.GetRandomMessageType(),
                MessageDeliveryMethod = messageDeliveryMethod.Value,
                TemplateName = TestDataGenerator.GetRandomString(),
                SenderName = messageDeliveryMethod == MessageDeliveryMethod.Email ? brand.Name : null,
                SenderEmail = messageDeliveryMethod == MessageDeliveryMethod.Email ? TestDataGenerator.GetRandomEmail() : null,
                Subject = messageDeliveryMethod == MessageDeliveryMethod.Email ? TestDataGenerator.GetRandomString() : null,
                SenderNumber = messageDeliveryMethod == MessageDeliveryMethod.Sms ? TestDataGenerator.GetRandomPhoneNumber() : null,
                MessageContent = content ?? "Hello, @Model.Username."
            };

            return addMessageTemplateData;
        }

        public EditMessageTemplateData CreateEditMessageTemplateData(
            Guid? messageTemplateId = null,
            Guid? brandId = null,
            string cultureCode = null,
            MessageType? messageType = null,
            MessageDeliveryMethod? messageDeliveryMethod = null,
            string content = null)
        {
            var addMessageTemplateData = CreateAddMessageTemplateData(
                brandId,
                cultureCode,
                messageType,
                messageDeliveryMethod,
                content);

            var editMessageTemplateData = new EditMessageTemplateData
            {
                Id = messageTemplateId ?? _contentRepository.MessageTemplates.First().Id
            };

            Mapper.Map(addMessageTemplateData, editMessageTemplateData);

            return editMessageTemplateData;
        }
    }
}