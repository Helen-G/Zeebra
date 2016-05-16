using System;
using System.Data.Entity;
using System.Linq;
using AFT.RegoV2.ApplicationServices.Security;
using AFT.RegoV2.Core.Common.Commands;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Content;
using AFT.RegoV2.Core.Common.Data.Content.MessageTemplateModels;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Content.Data;
using AFT.RegoV2.Core.Content.Events;
using AFT.RegoV2.Core.Content.Exceptions;
using AFT.RegoV2.Core.Content.Validators;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Domain.Security.Interfaces;
using AFT.RegoV2.Shared;
using AutoMapper;
using RazorEngine;
using RazorEngine.Templating;

namespace AFT.RegoV2.Core.Content.ApplicationServices
{
    public class MessageTemplatesCommands : MarshalByRefObject, IMessageTemplatesCommands
    {
        private readonly IContentRepository _contentRepository;
        private readonly IMessageTemplatesQueries _messageTemplatesQueries;
        private readonly ISecurityProvider _securityProvider;
        private readonly IEventBus _eventBus;
        private readonly IServiceBus _serviceBus;

        static MessageTemplatesCommands()
        {
            Mapper.CreateMap<AddMessageTemplateData, MessageTemplate>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Status.Inactive));

            Mapper.CreateMap<EditMessageTemplateData, MessageTemplate>();

            Mapper.CreateMap<Player, IMessageTemplateModel>()
                .ForMember(dest => dest.BrandName, opt => opt.MapFrom(src => src.Brand.Name))
                .ForMember(dest => dest.ToEmail, opt => opt.MapFrom(src => src.Email));

            Mapper.CreateMap<MessageTemplate, MessageTemplateUiData>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(data => data.TemplateName))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(data => data.Id))
                .ForMember(dest => dest.Content, opt => opt.MapFrom(data => data.MessageContent))
                .ForMember(dest => dest.TypeId, opt => opt.MapFrom(data => data.MessageDeliveryMethod));

            Mapper.CreateMap<MessageTemplateUiData, MessageTemplate>()
                .ForMember(dest => dest.TemplateName, opt => opt.MapFrom(data => data.Name))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(data => data.Id == null ? Guid.Empty : Guid.Parse(data.Id)))
                .ForMember(dest => dest.MessageContent, opt => opt.MapFrom(data => data.Content))
                .ForMember(dest => dest.MessageDeliveryMethod, opt => opt.MapFrom(data => (MessageDeliveryMethod)data.TypeId));
        }

        public MessageTemplatesCommands(
            IContentRepository contentRepository,
            IMessageTemplatesQueries messageTemplatesQueries,
            ISecurityProvider securityProvider,
            IEventBus eventBus,
            IServiceBus serviceBus)
        {
            _contentRepository = contentRepository;
            _messageTemplatesQueries = messageTemplatesQueries;
            _securityProvider = securityProvider;
            _eventBus = eventBus;
            _serviceBus = serviceBus;
        }

        //TODO: REMOVE
        public Guid Create(MessageTemplateUiData data)
        {
            var template = Mapper.Map<MessageTemplate>(data);
            if (template.Id == Guid.Empty)
                template.Id = Guid.NewGuid();

            _contentRepository.MessageTemplates.Add(template);
            _contentRepository.SaveChanges();

            return template.Id;
        }

        //TODO: REMOVE
        public Guid Update(MessageTemplateUiData data)
        {
            var messageTemplateEntity = _contentRepository.MessageTemplates.Find(Guid.Parse(data.Id));

            Mapper.DynamicMap(data, messageTemplateEntity);
            _contentRepository.SaveChanges();

            return messageTemplateEntity.Id;
        }

        //TODO: REMOVE
        public void Delete(Guid id)
        {
            var template = _contentRepository.MessageTemplates.Find(id);
            if (template != null)
            {
                _contentRepository.MessageTemplates.Remove(template);
                _contentRepository.SaveChanges();
            }
        }

        [Permission(Permissions.Add, Module = Modules.MessageTemplateManager)]
        public Guid Add(AddMessageTemplateData data)
        {
            var validationResult = _messageTemplatesQueries.ValidateCanAdd(data);

            if (!validationResult.IsValid)
                throw new RegoValidationException(validationResult);

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var messageTemplate = Mapper.Map<MessageTemplate>(data);

                messageTemplate.Created = DateTimeOffset.UtcNow;
                messageTemplate.CreatedBy = _securityProvider.User.UserName;

                _contentRepository.MessageTemplates.Add(messageTemplate);
                _contentRepository.SaveChanges();

                _eventBus.Publish(new MessageTemplateAddedEvent(messageTemplate));

                scope.Complete();

                return messageTemplate.Id;
            }
        }

        [Permission(Permissions.Edit, Module = Modules.MessageTemplateManager)]
        public void Edit(EditMessageTemplateData data)
        {
            var validationResult = _messageTemplatesQueries.ValidateCanEdit(data);

            if (!validationResult.IsValid)
                throw new RegoValidationException(validationResult);

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var messageTemplate = _contentRepository.MessageTemplates.Single(x => x.Id == data.Id);

                messageTemplate = Mapper.Map(data, messageTemplate);
                messageTemplate.Updated = DateTimeOffset.UtcNow;
                messageTemplate.UpdatedBy = _securityProvider.User.UserName;

                _contentRepository.SaveChanges();

                _eventBus.Publish(new MessageTemplateEditedEvent(messageTemplate));

                scope.Complete();
            }
        }

        [Permission(Permissions.Activate, Module = Modules.MessageTemplateManager)]
        public void Activate(ActivateMessageTemplateData data)
        {
            var validationResult = _messageTemplatesQueries.ValidateCanActivate(data);

            if(!validationResult.IsValid)
                throw new RegoValidationException(validationResult);

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var messageTemplate = _contentRepository.MessageTemplates.Single(x => x.Id == data.Id);
                messageTemplate.Status = Status.Active;
                messageTemplate.Activated = DateTimeOffset.UtcNow;
                messageTemplate.ActivatedBy = _securityProvider.User.UserName;

                var oldMessageTemplate = _contentRepository.MessageTemplates.SingleOrDefault(x =>
                    x.Id != messageTemplate.Id &&
                    x.BrandId == messageTemplate.BrandId &&
                    x.LanguageCode == messageTemplate.LanguageCode &&
                    x.MessageType == messageTemplate.MessageType &&
                    x.MessageDeliveryMethod == messageTemplate.MessageDeliveryMethod);

                if (oldMessageTemplate != null)
                    oldMessageTemplate.Status = Status.Inactive;

                _contentRepository.SaveChanges();

                _eventBus.Publish(new MessageTemplateActivatedEvent(messageTemplate.Id));

                scope.Complete();
            }
        }

        public void SendEmailIfTemplateExists(Guid playerId, MessageType messageType, IMessageTemplateModel model)
        {
            var player = _contentRepository.Players
                .Include(x => x.Brand)
                .Include(x => x.Language)
                .Single(x => x.Id == playerId);

            var template = _contentRepository.MessageTemplates.SingleOrDefault(x =>
                x.BrandId == player.Brand.Id &&
                x.LanguageCode == player.Language.Code &&
                x.MessageType == messageType &&
                x.Status == Status.Active);

            if (template == null)
                return;

            var expectedModelType = _messageTemplatesQueries.GetMessageTemplateModelType(messageType);
            var modelType = model.GetType();

            if (expectedModelType != modelType)
                throw new InvalidTemplateModelException(messageType, expectedModelType, modelType);

            model = Mapper.Map(player, model);

            var parsedContent = Engine.Razor.RunCompile(
                template.MessageContent,
                template.Id.ToString(),
                modelType,
                model);

            var emailCommandMessage = new EmailCommandMessage(
                template.SenderEmail,
                template.SenderName,
                player.Email,
                string.Format("{0} {1}", player.FirstName, player.LastName),
                template.Subject,
                parsedContent);

            _serviceBus.PublishMessage(emailCommandMessage);
        }
    }
}