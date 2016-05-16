using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Core.Common.Data.Content;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Content.Data;
using AFT.RegoV2.Core.Content.Validators;
using FluentValidation.Results;

namespace AFT.RegoV2.Core.Content.ApplicationServices
{
    public interface IMessageTemplatesQueries : IApplicationService
    {
        MessageTemplateUiData GetTemplate(Guid id);
        IQueryable<MessageTemplate> GetTemplates();

        IEnumerable<Language> GetBrandLanguages(Guid id);
        ValidationResult ValidateCanAdd(AddMessageTemplateData data);
        ValidationResult ValidateCanEdit(EditMessageTemplateData data);
        ValidationResult ValidateCanActivate(ActivateMessageTemplateData data);
        Type GetMessageTemplateModelType(MessageType messageType);
    }
}