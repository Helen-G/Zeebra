using System.Data.Entity;
using System.Linq;
using AFT.RegoV2.Core.Common.Data.Content;
using AFT.RegoV2.Core.Common.Extensions;
using AFT.RegoV2.Core.Content.ApplicationServices;
using FluentValidation;

namespace AFT.RegoV2.Core.Content.Validators
{
    public sealed class AddMessageTemplateValidator : MessageTemplateValidator<AddMessageTemplateData>
    {
        public AddMessageTemplateValidator(
            IContentRepository repository,
            IMessageTemplatesQueries messageTemplatesQueries) 
            : base(repository, messageTemplatesQueries)
        {
            ValidateBrand();
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

        private void ValidateBrand()
        {
            RuleFor(x => x.BrandId)
                .Must(x =>
                {
                    Brand = Repository.Brands
                        .Include(y => y.Languages)
                        .SingleOrDefault(y => y.Id == x);

                    return Brand != null;
                })
                .WithMessage(MessageTemplateValidationError.InvalidBrand);
        }
    }
}