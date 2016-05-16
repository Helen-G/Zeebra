using System.Linq;
using AFT.RegoV2.Core.Fraud.ApplicationServices.Data;
using FluentValidation;

namespace AFT.RegoV2.Core.Fraud.Validations
{
    public class UpdateWageringConfigurationValidator : AbstractValidator<WagerConfigurationDTO>
    {
        public UpdateWageringConfigurationValidator(IFraudRepository repository)
        {
            RuleFor(x => x)
                .Must(x => x.IsDepositWageringCheck || x.IsManualAdjustmentWageringCheck || x.IsRebateWageringCheck)
                .WithMessage("Please select the criteria")
                .WithName("BrandId");
            RuleFor(x => x.Currency)
                .NotNull()
                .NotEmpty()
                .WithMessage("Please select the currency");
        }
    }
}