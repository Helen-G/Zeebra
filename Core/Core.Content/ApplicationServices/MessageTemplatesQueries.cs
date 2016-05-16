using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using AFT.RegoV2.ApplicationServices.Security;
using AFT.RegoV2.Core.Common.Data.Content;
using AFT.RegoV2.Core.Common.Data.Content.MessageTemplateModels;
using AFT.RegoV2.Core.Content.Data;
using AFT.RegoV2.Core.Content.Validators;
using AFT.RegoV2.Core.Security.Common;
using AutoMapper;
using FluentValidation.Results;

namespace AFT.RegoV2.Core.Content.ApplicationServices
{
    public class MessageTemplatesQueries : MarshalByRefObject, IMessageTemplatesQueries
    {
        private readonly IContentRepository _contentRepository;

        static MessageTemplatesQueries()
        {
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

        public MessageTemplatesQueries(IContentRepository contentRepository)
        {
            _contentRepository = contentRepository;
        }

        //TODO: REMOVE
        public MessageTemplateUiData GetTemplate(Guid id)
        {
            var template = _contentRepository.MessageTemplates.Find(id);
            var templateUi = Mapper.Map<MessageTemplateUiData>(template);

            return templateUi;
        }

        public IEnumerable<Language> GetBrandLanguages(Guid id)
        {
            return _contentRepository.Brands
                .Include(x => x.Languages)
                .Single(x => x.Id == id)
                .Languages;
        }

        [Permission(Permissions.View, Module = Modules.MessageTemplateManager)]
        public IQueryable<MessageTemplate> GetTemplates()
        {
            return _contentRepository.MessageTemplates
                .Include(x => x.Brand)
                .Include(x => x.Language)
                .AsQueryable();
        }

        public ValidationResult ValidateCanAdd(AddMessageTemplateData data)
        {
            return new AddMessageTemplateValidator(_contentRepository, this).Validate(data);
        }

        public ValidationResult ValidateCanEdit(EditMessageTemplateData data)
        {
            return new EditMessageTemplateValidator(_contentRepository, this).Validate(data);
        }

        public ValidationResult ValidateCanActivate(ActivateMessageTemplateData data)
        {
            return new ActivateMessageTemplateValidator(_contentRepository).Validate(data);
        }

        public Type GetMessageTemplateModelType(MessageType messageType)
        {
            switch (messageType)
            {
                case MessageType.PlayerRegistered:
                    return typeof(PlayerRegisteredModel);
                default:
                    throw new InvalidEnumArgumentException("messageType", (int)messageType, typeof(MessageType));
            }
        }
    }
}