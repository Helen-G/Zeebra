using System.Linq;
using AFT.RegoV2.Core.Brand;
using AFT.RegoV2.Domain.Payment;
using FluentValidation;

namespace AFT.RegoV2.Core.Payment.Validators
{
    public class EditBankAccountValidator : AbstractValidator<EditBankAccountData>
    {
        public EditBankAccountValidator(IPaymentRepository paymentRepository, IBrandRepository brandRepository)
        {
            CascadeMode = CascadeMode.StopOnFirstFailure;

            RuleFor(x => x.Bank)
                .Must(x => paymentRepository.Banks.Any(y => y.Id == x))
                .WithMessage("{\"text\": \"app:bankAccounts.bankNotFound\"}");

            RuleFor(x => x.Currency)
                .NotNull()
                .WithMessage("{\"text\": \"app:common.requiredField\"}")
                .Must(x => brandRepository.Currencies.Any(y => y.Code == x))
                .WithMessage("{\"text\": \"app:bankAccounts.currencyNotFound\"}");

            RuleFor(x => x.AccountId)
                .Must((data, x) => paymentRepository.BankAccounts.Count(y => y.Id == data.Id) == 1)
                .WithMessage("{\"text\": \"app:bankAccounts.accountNotFound\"}")
                .NotNull()
                .WithMessage("{\"text\": \"app:common.requiredField\"}")
                .Length(1, 20)
                .WithMessage("{{\"text\": \"app:common.exceedMaxLength\", \"variables\": {{\"length\": \"{0}\"}}}}", 50)
                .Matches(@"^[a-zA-Z0-9]*$")
                .WithMessage("{\"text\": \"app:common.validationMessages.alphanumeric\"}");

            RuleFor(x => x.AccountName)
                .NotNull()
                .WithMessage("{\"text\": \"app:common.requiredField\"}")
                .Must(x => x.Length >= 2 && x.Length <= 100)
                .WithMessage("{{\"text\": \"app:common.numberOutOfRange\", \"variables\": {{\"minimum\": \"2\", \"maximum\": \"100\"}}}}")
                .Matches(@"^[a-zA-Z0-9-'\. ]*$")
                .WithMessage("{\"text\": \"app:common.validationMessages.alphanumericDashesApostrophesPeriodsSpaces\"}");

            RuleFor(x => x.AccountNumber)
                .NotNull()
                .WithMessage("{\"text\": \"app:common.requiredField\"}")
                .Length(1, 50)
                .WithMessage("{{\"text\": \"app:common.exceedMaxLength\", \"variables\": {{\"length\": \"{0}\"}}}}", 50)
                .Matches(@"^[0-9]*$")
                .WithMessage("{\"text\": \"app:common.validationMessages.numeric\"}");

            When(x => !string.IsNullOrEmpty(x.AccountType), () => RuleFor(x => x.AccountType)
                .Length(1, 20)
                .WithMessage("{{\"text\": \"app:common.exceedMaxLength\", \"variables\": {{\"length\": \"{0}\"}}}}", 20)
                .Matches(@"^[a-zA-Z0-9]*$")
                .WithMessage("{\"text\": \"app:common.validationMessages.alphanumeric\"}"));

            RuleFor(x => x.Province)
                .NotNull()
                .WithMessage("{\"text\": \"app:common.requiredField\"}")
                .Length(1, 50)
                .WithMessage("{{\"text\": \"app:common.exceedMaxLength\", \"variables\": {{\"length\": \"{0}\"}}}}", 50)
                .Matches(@"^[a-zA-Z0-9-_'\. ]*$")
                .WithMessage("{\"text\": \"app:common.validationMessages.alphanumericDashesUnderscoresApostrophesPeriodsSpaces\"}");

            RuleFor(x => x.Branch)
                .NotNull()
                .WithMessage("{\"text\": \"app:common.requiredField\"}")
                .Length(1, 50)
                .WithMessage("{{\"text\": \"app:common.exceedMaxLength\", \"variables\": {{\"length\": \"{0}\"}}}}", 50)
                .Matches(@"^[a-zA-Z0-9-'\. ]*$")
                .WithMessage("{\"text\": \"app:common.validationMessages.alphanumericDashesApostrophesPeriodsSpaces\"}");

            When(x => !string.IsNullOrEmpty(x.Remarks), () => RuleFor(x => x.Remarks)
                .Length(1, 200)
                .WithMessage("{{\"text\": \"app:common.exceedMaxLength\", \"variables\": {{\"length\": \"{0}\"}}}}", 200));
        }
    }
}
