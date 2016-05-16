using System;
using System.Linq;
using AFT.RegoV2.Domain.Payment;
using AFT.RegoV2.Domain.Payment.Commands;
using FluentValidation;

namespace AFT.RegoV2.Core.Services.Payment.Validators
{
    public class SetCurrentPlayerBankAccountValidator : AbstractValidator<SetCurrentPlayerBankAccountCommand>
    {
        public SetCurrentPlayerBankAccountValidator(IPaymentRepository repository)
        {
            RuleFor(x => x.PlayerBankAccountId)
                .Must(x => repository.PlayerBankAccounts.SingleOrDefault(y => y.Id == x) != null)
                .WithMessage("{\"text\": \"app:common.idDoesNotExist\"}");
        }
    }
}