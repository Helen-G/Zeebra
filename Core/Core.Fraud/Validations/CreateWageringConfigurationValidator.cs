using System.Linq;
using AFT.RegoV2.Core.Fraud.ApplicationServices.Data;
using FluentValidation;

namespace AFT.RegoV2.Core.Fraud.Validations
{
    public class CreateWageringConfigurationValidator : AbstractValidator<WagerConfigurationDTO>
    {
        public CreateWageringConfigurationValidator(IFraudRepository repository)
        {
            RuleFor(x => x)
                .Must(x => x.IsDepositWageringCheck || x.IsManualAdjustmentWageringCheck || x.IsRebateWageringCheck)
                .WithMessage("Please select the criteria")
                .WithName("BrandId");
            RuleFor(x => x)
                .Must(x =>
                {
                    return
                        !repository.WagerConfigurations.Any(y => y.BrandId == x.BrandId && y.CurrencyCode == x.Currency);
                })
                .WithName("Currency")
                .WithMessage("The brand already has wagering configuration for this currency");

            RuleFor(x => x.Currency)
                .NotNull()
                .NotEmpty()
                .WithMessage("Please select the currency");
        }
    }
}