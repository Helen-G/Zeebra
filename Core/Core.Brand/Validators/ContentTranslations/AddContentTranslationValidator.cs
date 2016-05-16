using FluentValidation;

namespace AFT.RegoV2.Core.Brand.Validators.ContentTranslations
{
    class AddContentTranslationValidator : AbstractValidator<AddContentTranslationData>
    {
        public AddContentTranslationValidator(IBrandRepository repository)
        {

        }
    }
}
