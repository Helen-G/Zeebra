using AFT.RegoV2.Core.Brand.Data;
using AFT.RegoV2.Core.Common.Data.Brand;
using FluentValidation;

namespace AFT.RegoV2.Core.Brand.Validators
{
    public class DeactivateBrandValidationData
    {
        public Core.Brand.Data.Brand Brand { get; set; }
    }

    public class DeactivateBrandValidator : AbstractValidator<DeactivateBrandValidationData>
    {
        public DeactivateBrandValidator()
        {
            CascadeMode = CascadeMode.StopOnFirstFailure;

            RuleFor(x => x.Brand)
                .NotNull()
                .WithMessage("{\"text\": \"app:brand.activation.noBrandFound\"}")
                .Must(x => x.Status == BrandStatus.Active)
                .WithMessage("{\"text\": \"app:brand.deactivation.notActive\"}");

            When(x => x.Brand != null && x.Brand.Status == BrandStatus.Active, () => RuleFor(x => x.Brand.Remarks)
                .Must(x => !string.IsNullOrWhiteSpace(x))
                .WithMessage("{\"text\": \"app:brand.noRemarks\"}"));
        }
    }
}
