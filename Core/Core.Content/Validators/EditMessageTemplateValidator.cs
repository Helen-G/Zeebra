using System.Data.Entity;
using System.Linq;
using AFT.RegoV2.Core.Common.Data.Content;
using AFT.RegoV2.Core.Common.Extensions;
using AFT.RegoV2.Core.Content.ApplicationServices;
using FluentValidation;

namespace AFT.RegoV2.Core.Content.Validators
{
    public sealed class EditMessageTemplateValidator : MessageTemplateValidator<EditMessageTemplateData>
    {
        private bool _isValidId;

        public EditMessageTemplateValidator(
            IContentRepository repository,
            IMessageTemplatesQueries messageTemplatesQueries) 
            : base(repository, messageTemplatesQueries)
        {
            ValidatedId();

            if (!_isValidId) return;

            ValidateLanguage();
            ValidateMessageType();
            ValidateMessageDeliveryMethod();
            ValidateTemplateName();
            ValidateFromName();
            ValidateFromEmail();
            ValidateSubject();
            ValidateFromNumber();
            ValidateMessageContent();
        }

        private void ValidatedId()
        {
            RuleFor(x => x.Id)
                .Must(x =>
                {
                    var messageTemplate = Repository.MessageTemplates
                        .Include(y => y.Brand.Languages)
                        .SingleOrDefault(y => y.Id == x);

                    if (messageTemplate != null)
                        Brand = messageTemplate.Brand;

                    _isValidId = messageTemplate != null;

                    return _isValidId;
                })
                .WithMessage(MessageTemplateValidationError.InvalidId);
        }

        protected override void ValidateTemplateName()
        {
            When(x => Brand != null, () =>
                RuleFor(x => x.TemplateName)
                    .NotEmpty()
                    .WithMessage(MessageTemplateValidationError.Required)
                    .Must((data, name) => !Repository.MessageTemplates
                        .Any(y =>
                            y.Id != data.Id &&
                            y.BrandId == Brand.Id &&
                            y.MessageType == data.MessageType &&
                            y.LanguageCode == data.LanguageCode &&
                            y.TemplateName == name))
                    .WithMessage(MessageTemplateValidationError.TemplateNameInUse));
        }
    }
}