using System.Data.Entity;
using System.Linq;
using AFT.RegoV2.Core.Brand.Data;
using AFT.RegoV2.Core.Common.Data.Brand;
using FluentValidation;

namespace AFT.RegoV2.Core.Brand.Validators
{
    public class AssignBrandCountryValidator : AbstractValidator<AssignBrandCountryData>
    {
        public AssignBrandCountryValidator(IBrandRepository brandRepository)
        {
            CascadeMode = CascadeMode.StopOnFirstFailure;

            RuleFor(x => x.Brand)
                .Must(x => brandRepository.Brands.SingleOrDefault(y => y.Id == x) != null)
                .WithMessage("{\"text\": \"app:brand.invalidBrand\"}");

            RuleFor(x => x.Countries)
                .NotNull()
                .Must(x => x.Any())
                .WithMessage("{\"text\": \"app:brand.noAssignedCountry\"}")
                .Must((data, x) =>
                {
                    var hasValidCountries = x.All(y => brandRepository.Countries.Any(z => z.Code == y));
                    
                    return hasValidCountries;
                })
                .WithMessage("{\"text\": \"app:brand.invalidCountry\"}")
                .Must((data, x) =>
                {
                    var brand = brandRepository.Brands
                        .Include(y => y.BrandCountries)
                        .Single(y => y.Id == data.Brand);

                    if (brand.Status != BrandStatus.Active) return true;

                    var noCountriesRemoved = brand.BrandCountries.All(y => x.Contains(y.CountryCode));

                    return noCountriesRemoved;
                })
                .WithMessage("{\"text\": \"app:brand.removedCountryActiveBrandError\"}");
        }
    }
}
