using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Core.Brand.Data;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Brand;
using AFT.RegoV2.Core.Common.Events.Brand;
using FluentValidation;

namespace AFT.RegoV2.Core.Brand.Validators
{
    public class ActivateBrandValidationData
    {
        public Data.Brand Brand { get; set; }
        public IEnumerable<PaymentLevelDTO> BrandPaymentLevels { get; set; }
        public IEnumerable<RiskLevel> BrandRiskLevels { get; set; }
        public string Remarks { get; set; }
    }

    public class ActivateBrandValidator : AbstractValidator<ActivateBrandValidationData>
    {
        public ActivateBrandValidator()
        {
            CascadeMode = CascadeMode.StopOnFirstFailure;

            RuleFor(x => x.Brand)
                .NotNull()
                .WithMessage("{\"text\": \"app:brand.noBrandFound\"}")
                .Must(x => x.Status == BrandStatus.Inactive)
                .WithMessage("{\"text\": \"app:brand.activation.notInactive\"}")
                .Must(x => x.WalletTemplates.Any())
                .WithMessage("{\"text\": \"app:brand.noAssignedWallet\"}")
                .Must(x => x.Licensee.AllowedBrandCount == 0 || x.Licensee.Brands.Count(y => y.Status == BrandStatus.Active) < x.Licensee.AllowedBrandCount)
                .WithMessage("{\"text\": \"app:brand.activation.licenseeBrandLimitExceeded\"}");

            When(x => x.Brand != null && x.Brand.Status == BrandStatus.Inactive, () =>
            {
                RuleFor(x => x.Brand.BrandCountries)
                    .Must(x => x.Any())
                    .WithMessage("{\"text\": \"app:brand.activation.noAssignedCountry\"}");

                RuleFor(x => x.Brand.BrandCurrencies)
                    .Must(x => x.Any())
                    .WithMessage("{\"text\": \"app:brand.activation.noAssignedCurrency\"}");

                RuleFor(x => x.Brand.BrandCultures)
                    .Must(x => x.Any())
                    .WithMessage("{\"text\": \"app:brand.activation.noAssignedLanguage\"}");

                RuleFor(x => x.Brand)
                    .Must(x => x.DefaultVipLevelId != null)
                    .WithName("DefaultVipLevel")
                    .WithMessage("{\"text\": \"app:brand.activation.noDefaultVipLevel\"}");

                RuleFor(x => x.Brand.Products)
                    .Must(x => x.Any())
                    .WithMessage("{\"text\": \"app:brand.activation.noGameProviders\"}");

                RuleFor(x => x.BrandRiskLevels)
                    .Must(x => x.Any())
                    .WithMessage("{\"text\": \"app:brand.activation.noRiskLevels\"}");

                RuleFor(x => x.BrandPaymentLevels)
                    .Must((data, paymentLevels) =>
                    {
                        var defaultPaymentLevels = paymentLevels.Where(x => x.IsDefault);
                        var currenciesHaveDefaultPaymentLevel =
                            data.Brand.BrandCurrencies.All(x => defaultPaymentLevels.Any(y => y.CurrencyCode == x.CurrencyCode));

                        return currenciesHaveDefaultPaymentLevel;
                    })
                    .WithMessage("{\"text\": \"app:brand.activation.noDefaultPaymentLevels\"}");

                RuleFor(x => x.Remarks)
                    .Must(x => !string.IsNullOrWhiteSpace(x))
                    .WithMessage("{\"text\": \"app:brand.noRemarks\"}");
            });
        }
    }
}
