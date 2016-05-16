using System;
using AFT.RegoV2.Core.Common.Data.Content;
using AFT.RegoV2.Core.Common.Data.Content.MessageTemplateModels;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Content.Data;
using AFT.RegoV2.Core.Content.Validators;

namespace AFT.RegoV2.Core.Content.ApplicationServices
{
    public interface IMessageTemplatesCommands : IApplicationService
    {
        Guid Create(MessageTemplateUiData data);
        Guid Update(MessageTemplateUiData data);
        void Delete(Guid id);

        Guid Add(AddMessageTemplateData data);
        void Edit(EditMessageTemplateData data);
        void Activate(ActivateMessageTemplateData data);
        void SendEmailIfTemplateExists(
            Guid playerId,
            MessageType messageType,
            IMessageTemplateModel model);
    }
}