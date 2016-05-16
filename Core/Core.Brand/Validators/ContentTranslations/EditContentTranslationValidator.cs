using FluentValidation;

namespace AFT.RegoV2.Core.Brand.Validators.ContentTranslations
{
    class EditContentTranslationValidator : AbstractValidator<EditContentTranslationData>
    {
        public EditContentTranslationValidator(IBrandRepository repository)
        {

        }
    }
}
