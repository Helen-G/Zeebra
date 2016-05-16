using AFT.RegoV2.AdminWebsite.Resources;
using AFT.RegoV2.AdminWebsite.ViewModels;
using FluentValidation;

namespace AFT.RegoV2.AdminWebsite.Validators
{
    public class LoginValidator : AbstractValidator<LoginViewModel>
    {
        public LoginValidator()
        {
            RuleFor(x => x.Username).NotEmpty()
                .WithMessage(ValidationErrors.UsernameIsRequired);
            RuleFor(x => x.Password).NotEmpty()
                .WithMessage(ValidationErrors.PasswordIsRequired);
        }
    }
}