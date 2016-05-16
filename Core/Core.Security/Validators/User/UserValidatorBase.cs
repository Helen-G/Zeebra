using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AFT.RegoV2.Core.Security.ApplicationServices.Data.Users;
using FluentValidation;

namespace AFT.RegoV2.Core.Security.Validators
{
    public class UserValidatorBase<T> : AbstractValidator<T>
        where T : UserDataBase
    {
        public UserValidatorBase()
        {
            CascadeMode = CascadeMode.StopOnFirstFailure;
        }

        protected void ValidateUsername()
        {
            RuleFor(x => x.Username)
                .NotNull()
                .WithMessage("{\"text\": \"app:common.requiredField\"}")
                .Length(6, 50)
                .WithMessage("{{\"text\": \"app:common.exceedMaxLength\", \"variables\": {{\"length\": \"{0}\"}}}}", 50)
                .Matches(@"^[A-Za-z0-9]+(?:[._\'-][A-Za-z0-9]+)*$")
                .WithMessage("{\"text\": \"app:admin.messages.usernameInvalid\"}");
        }

        protected void ValidateFirstName()
        {
            RuleFor(x => x.FirstName)
                .NotNull()
                .WithMessage("{\"text\": \"app:common.requiredField\"}")
                .Length(1, 50)
                .WithMessage("{{\"text\": \"app:common.exceedMaxLength\", \"variables\": {{\"length\": \"{0}\"}}}}", 50)
                .Matches(@"^[A-Za-z0-9]+(?:[._\'-][A-Za-z0-9]+)*$")
                .WithMessage("{\"text\": \"app:admin.messages.firstNameInvalid\"}");
        }

        protected void ValidateLastName()
        {
            RuleFor(x => x.LastName)
                .NotNull()
                .WithMessage("{\"text\": \"app:common.requiredField\"}")
                .Length(1, 50)
                .WithMessage("{{\"text\": \"app:common.exceedMaxLength\", \"variables\": {{\"length\": \"{0}\"}}}}", 50)
                .Matches(@"^[A-Za-z0-9]+(?:[._\'-][A-Za-z0-9]+)*$")
                .WithMessage("{\"text\": \"app:admin.messages.lastNameInvalid\"}");
        }

        protected void ValidatePassword()
        {
            RuleFor(x => x.Password)
               .NotNull()
               .WithMessage("{\"text\": \"app:common.requiredField\"}")
               .Length(1, 50)
               .WithMessage("{{\"text\": \"app:common.exceedMaxLength\", \"variables\": {{\"length\": \"{0}\"}}}}", 50);
        }

        protected void ValidateAssignedLicensees()
        {
            RuleFor(x => x.AssignedLicensees)
                .NotEmpty()
                .WithMessage("{\"text\": \"app:admin.messages.licenseesRequired\"}")
                .Must(x => x.All(l => l != Guid.Empty))
                .WithMessage("{\"text\": \"app:admin.messages.licenseesRequired\"}");
        }

        protected void ValidateAllowedBrands()
        {
            RuleFor(x => x.AllowedBrands)
                .NotEmpty()
                .WithMessage("{\"text\": \"app:admin.messages.brandsRequired\"}")
                .Must(x => x.All(b => b != Guid.Empty))
                .WithMessage("{\"text\": \"app:admin.messages.brandsRequired\"}");
        }

        protected void ValidateCurrencies()
        {
            RuleFor(x => x.Currencies)
                .NotEmpty()
                .WithMessage("{\"text\": \"app:admin.messages.currenciesRequired\"}");
        }
    }
}
