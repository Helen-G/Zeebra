using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using AFT.RegoV2.Core.Brand.Data;
using AFT.RegoV2.Core.Common.Data.Brand;
using FluentValidation;

namespace AFT.RegoV2.Core.Brand.Validators
{
    public class AssignBrandProductValidator : AbstractValidator<AssignBrandProductsData>
    {
        public AssignBrandProductValidator(IBrandRepository brandRepository, IEnumerable<LicenseeProduct> gameProviders)
        {
            CascadeMode = CascadeMode.StopOnFirstFailure;

            RuleFor(x => x.BrandId)
                .Must(x => brandRepository.Brands.SingleOrDefault(y => y.Id == x) != null)
                .WithMessage("{\"text\": \"app:brand.invalidBrand\"}");

            RuleFor(x => x.ProductsIds)
                .Must(
                    products =>
                        products.All(
                            product =>
                                gameProviders.Any(gameProvider => gameProvider.ProductId == product)))
                .WithMessage("{\"text\": \"app:brand.invalidGameProvider\"}")
                .Must((data, x) =>
                {
                    var brand = brandRepository.Brands.Include(y => y.Products).Single(y => y.Id == data.BrandId);

                    if (brand.Status != BrandStatus.Active) 
                        return true;

                    var noProductsRemoved = brand.Products.All(y => x.Contains(y.ProductId));

                    return noProductsRemoved;
                })
                .WithMessage("{\"text\": \"app:brand.removedGameProviderActiveBrandError\"}");
        }
    }
}
