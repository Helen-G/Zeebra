using System;
using System.Linq;
using System.Text.RegularExpressions;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Domain.Player;
using AFT.RegoV2.Core.Player;
using ServiceStack.FluentValidation;

namespace AFT.RegoV2.Domain.BoundedContexts.Player.Validators
{
    public class ReferalDataValidator : AbstractValidator<ReferralData>
    {
        public ReferalDataValidator(IPlayerRepository repository)
        {
            CascadeMode = CascadeMode.Continue;

            RuleFor(data => data.ReferrerId)
                .Must(id => id != Guid.Empty)
                .WithMessage(ReferalDataValidatorResponseCodes.ReferrerIdIsRequired.ToString());

            RuleFor(data => data.ReferrerId)
                .Must(id => repository.Players.Any(p => p.Id == id))
                .WithMessage(ReferalDataValidatorResponseCodes.ReferrerIdNotFound.ToString())
                .When(data => data.ReferrerId != Guid.Empty);

            RuleFor(data => data.PhoneNumbers)
                .NotNull()
                .WithMessage(ReferalDataValidatorResponseCodes.PhoneNumbersAreMissing.ToString());

            RuleFor(data => data.PhoneNumbers)
                .Must(list =>
                {
                    if (list.Any(s => s == null)) return false;
                    return list.All(number => Regex.IsMatch(number, "^[0-9]{8,15}$"));
                })
                .WithMessage(ReferalDataValidatorResponseCodes.PhoneNumbersAreNotValid.ToString())
                .When(data => data.PhoneNumbers != null);
        }
    }
}
