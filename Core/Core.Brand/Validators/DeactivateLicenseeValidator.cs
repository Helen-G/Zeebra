using AFT.RegoV2.Core.Brand.Data;
using FluentValidation;

namespace AFT.RegoV2.Core.Brand.Validators
{
    public class DeactivateLicenseeData
    {
        public Licensee Licensee { get; set; }
        public string Remarks { get; set; }
    }

    public class DeactivateLicenseeValidator : AbstractValidator<DeactivateLicenseeData>
    {
        public DeactivateLicenseeValidator()
        {
            CascadeMode = CascadeMode.StopOnFirstFailure;

            RuleFor(x => x.Licensee)
                .NotNull()
                .WithMessage("{\"text\": \"app:licensee.noLicenseeFound\"}")
                .Must(x => x.Status == LicenseeStatus.Active)
                .WithMessage("{\"text\": \"app:licensee.notActive\"}");

            RuleFor(x => x.Remarks)
                .NotEmpty()
                .WithMessage("{\"text\": \"app:common.reequired\"}")
                .Length(1, 200)
                .WithMessage("{\"text\": \"app:licensee.remarksMax\"}");
        }
    }
}
