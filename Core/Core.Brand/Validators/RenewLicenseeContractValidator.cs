using System;
using System.Globalization;
using System.Linq;
using FluentValidation;

namespace AFT.RegoV2.Core.Brand.Validators
{
    public class RenewLicenseeContractValidator : AbstractValidator<RenewLicenseeContractData>
    {
        public RenewLicenseeContractValidator(IBrandRepository repository)
        {
            CascadeMode = CascadeMode.StopOnFirstFailure;

            var parsedContractStart = new DateTimeOffset();

            RuleFor(x => x.ContractStart)
                .Must((data, x) => data.LicenseeId != Guid.Empty)
                .WithMessage("{\"text\": \"app:licensee.idNotFound\"}")
                .Must((data, x) => repository.Licensees.Any(y => data.LicenseeId == y.Id))
                .WithMessage("{\"text\": \"app:licensee.idNotFound\"}")
                .Must(x => DateTimeOffset.TryParseExact(
                    x,
                    "yyyy/MM/dd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal,
                    out parsedContractStart))
                .WithMessage("{\"text\": \"app:common.validationMessages.incorrectDateFormat\"}")
                .Must((data, x) =>
                    parsedContractStart > repository.Licensees.Single(y => data.LicenseeId == y.Id).ContractEnd)
                .WithMessage("{\"text\": \"app:licensee.contractStartNotAfterCurrent\"}");

            When(x => !string.IsNullOrEmpty(x.ContractEnd), () =>
            {
                var parsedContractEnd = new DateTimeOffset();

                RuleFor(x => x.ContractEnd)
                    .Must(x => DateTimeOffset.TryParseExact(
                        x,
                        "yyyy/MM/dd",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeUniversal,
                        out parsedContractEnd))
                    .WithMessage("{\"text\": \"app:common.validationMessages.incorrectDateFormat\"}")
                    .Must((data, x) => parsedContractEnd > parsedContractStart)
                    .WithMessage("{\"text\": \"app:licensee.contractEndNotAftereStart\"}");
            });
        }
    }
}
