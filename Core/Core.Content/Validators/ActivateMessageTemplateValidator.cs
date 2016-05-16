using System.Linq;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Content;
using AFT.RegoV2.Core.Common.Extensions;
using AFT.RegoV2.Core.Content.Data;
using FluentValidation;

namespace AFT.RegoV2.Core.Content.Validators
{
    public class ActivateMessageTemplateValidator : AbstractValidator<ActivateMessageTemplateData>
    {
        public ActivateMessageTemplateValidator(
            IContentRepository repository)
        {
            CascadeMode = CascadeMode.StopOnFirstFailure;

            MessageTemplate messageTemplate = null;

            RuleFor(x => x.Id)
                .Must(x =>
                {
                    messageTemplate = repository.MessageTemplates.SingleOrDefault(y => y.Id == x);
                    return messageTemplate != null;
                })
                .WithMessage(MessageTemplateValidationError.InvalidId)
                .Must(x => messageTemplate.Status != Status.Active)
                .WithMessage(MessageTemplateValidationError.AlreadyActive);
        }
    }
}