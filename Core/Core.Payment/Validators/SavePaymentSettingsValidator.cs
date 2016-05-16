using AFT.RegoV2.Domain.Payment.ApplicationServices.Data;
using FluentValidation;

namespace AFT.RegoV2.Core.Payment.Validators
{
    class SavePaymentSettingsValidator : AbstractValidator<SavePaymentSettingsCommand>
    {
        public SavePaymentSettingsValidator()
        {
            RuleFor(x => x.MinAmountPerTransaction)
               .Must(x => x >= 0)
               .WithMessage(PaymentSettingsErrors.MinAmountPerTransactionError.ToString());

            RuleFor(x => x.MaxAmountPerTransaction)
                .Must(x => x >= 0)
                .WithMessage(PaymentSettingsErrors.MaxAmountPerTransactionError.ToString());

            RuleFor(x => x.MaxAmountPerTransaction)
                .Must((data, x) =>
                {
                    if (data.MinAmountPerTransaction != 0 && data.MaxAmountPerTransaction != 0)
                        return x > data.MinAmountPerTransaction;

                    return true;
                })
                .WithMessage(PaymentSettingsErrors.MaxminAmountPerTransactionError.ToString());

            RuleFor(x => x.MaxAmountPerDay)
                .Must(x => x >= 0)
                .WithMessage(PaymentSettingsErrors.MaxAmountPerDayError.ToString());

            RuleFor(x => x.MaxAmountPerDay)
                .Must((data, x) =>
                {
                    if (data.MinAmountPerTransaction != 0 && data.MaxAmountPerDay != 0)
                        return x >= data.MinAmountPerTransaction;

                    return true;
                })
                .WithMessage(PaymentSettingsErrors.MinAmountPerTransactionErrorAmountPerDay.ToString());

            RuleFor(x => x.MaxAmountPerTransaction)
               .Must((data, x) =>
               {
                   if (data.MaxAmountPerTransaction != 0 && data.MaxAmountPerDay != 0)
                       return x <= data.MaxAmountPerDay;

                   return true;
               })
               .WithMessage(PaymentSettingsErrors.MaxAmountPerTransactionErrorAmountPerDay.ToString());

            RuleFor(x => x.MaxTransactionPerDay)
                .Must(x => x >= 0)
                .WithMessage(PaymentSettingsErrors.MaxTransactionPerDayError.ToString());

            RuleFor(x => x.MaxTransactionPerWeek)
                .Must(x => x >= 0)
                .WithMessage(PaymentSettingsErrors.MaxTransactionPerWeekError.ToString());

            RuleFor(x => x.MaxTransactionPerWeek)
                .Must((data, x) =>
                {
                    if (data.MaxTransactionPerWeek != 0 && data.MaxTransactionPerDay != 0)
                        return x >= data.MaxTransactionPerDay;

                    return true;
                })
                .WithMessage(PaymentSettingsErrors.MaxTransactionPerWeekErrorPerDay.ToString());

            RuleFor(x => x.MaxTransactionPerMonth)
               .Must(x => x >= 0)
               .WithMessage(PaymentSettingsErrors.MaxTransactionPerMonthError.ToString());

            RuleFor(x => x.MaxTransactionPerMonth)
                .Must((data, x) =>
                {
                    if (data.MaxTransactionPerWeek != 0 && data.MaxTransactionPerMonth != 0)
                        return x >= data.MaxTransactionPerWeek;

                    return true;
                })
                .WithMessage(PaymentSettingsErrors.MaxTransactionPerMonthErrorPerWeek.ToString());

            RuleFor(x => x.MaxTransactionPerMonth)
                .Must((data, x) =>
                {
                    if (data.MaxTransactionPerDay != 0 && data.MaxTransactionPerMonth != 0)
                        return x >= data.MaxTransactionPerDay;

                    return true;
                })
                .WithMessage(PaymentSettingsErrors.MaxTransactionPerMonthErrorPerDay.ToString());
         }
    }
}
