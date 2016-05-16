using System;
using AFT.RegoV2.Core.Brand.Data;
using FluentValidation;

namespace AFT.RegoV2.Core.Brand.Validators
{
    public class ActivateLicenseeData
    {
        public Licensee Licensee { get; set; }
        public string Remarks { get; set; }
    }

    public class ActivateLicenseeValidator : AbstractValidator<ActivateLicenseeData>
    {
        public ActivateLicenseeValidator()
        {
            CascadeMode = CascadeMode.StopOnFirstFailure;

            CascadeMode = CascadeMode.StopOnFirstFailure;

            RuleFor(x => x.Licensee)
                .NotNull()
                .WithMessage("{\"text\": \"app:licensee.noLicenseeFound\"}")
                .Must(x => x.Status == LicenseeStatus.Inactive || x.Status == LicenseeStatus.Deactivated)
                .WithMessage("{\"text\": \"app:licensee.notInactive\"}")
                .Must(x => x.ContractEnd == null || x.ContractEnd > DateTimeOffset.UtcNow)
                .WithMessage("{\"text\": \"app:licensee.cantActivateContractExpired\"}");

            RuleFor(x => x.Remarks)
                .NotEmpty()
                .WithMessage("{\"text\": \"app:common.reequired\"}")
                .Length(1, 200)
                .WithMessage("{\"text\": \"app:licensee.remarksMax\"}");           
        }
    }
}
