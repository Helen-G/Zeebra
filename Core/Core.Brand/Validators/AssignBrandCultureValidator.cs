using System.Data.Entity;
using System.Linq;
using AFT.RegoV2.Core.Brand.Data;
using AFT.RegoV2.Core.Common.Data.Brand;
using FluentValidation;

namespace AFT.RegoV2.Core.Brand.Validators
{
    public class AssignBrandCultureValidator : AbstractValidator<AssignBrandCultureData>
    {
        public AssignBrandCultureValidator(IBrandRepository brandRepository)
        {
            CascadeMode = CascadeMode.StopOnFirstFailure;

            RuleFor(x => x.Brand)
                .Must(x => brandRepository.Brands.SingleOrDefault(y => y.Id == x) != null)
                .WithMessage("{\"text\": \"app:brand.noBrandFound\"}");

            RuleFor(x => x.Cultures)
                .NotNull()
                .Must(x => x.Any())
                .WithMessage("{\"text\": \"app:brand.noAssignedLanguage\"}")
                .Must((data, x) =>
                {
                    var hasValidCultures = x.All(y => brandRepository.Cultures.Any(z => z.Code == y));
                    
                    return hasValidCultures;
                })
                .WithMessage("{\"text\": \"app:brand.invalidLanguage\"}")
                .Must((data, x) =>
                {
                    var brand = brandRepository.Brands
                        .Include(y => y.BrandCultures.Select(z => z.Culture))
                        .Single(y => y.Id == data.Brand);

                    if (brand.Status != BrandStatus.Active) return true;

                    var noCulturesRemoved = brand.BrandCultures.All(y => x.Contains(y.Culture.Code));
                    
                    return noCulturesRemoved;
                })
                .WithMessage("{\"text\": \"app:brand.removedLanguageActiveBrandError\"}");

            RuleFor(x => x.DefaultCulture)
                .NotEmpty().WithMessage("{\"text\": \"app:common.requiredField\"}")
                .Must((data, x) => data.Cultures.Contains(x))
                .WithMessage("{\"text\": \"app:brand.invalidDefaultLanguage\"}")
                .Must((data, x) =>
                {
                    var brand = brandRepository.Brands.SingleOrDefault(y => y.Id == data.Brand);

                    if (brand == null || brand.Status != BrandStatus.Active) return true;

                    var activeBrandDefaultNotChanged = brand.DefaultCulture == x;

                    return activeBrandDefaultNotChanged;
                })
                .WithMessage("{\"text\": \"app:brand.activeBrandDefaultCultureChangedError\"}");
        }
    }
}
