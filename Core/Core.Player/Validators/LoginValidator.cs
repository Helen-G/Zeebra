using System;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Domain.Player;
using AFT.RegoV2.Core.Player.Data;
using AFT.RegoV2.Core.Services.Security;
using FluentValidation;

namespace AFT.RegoV2.Core.Player.Validators
{
    public class LoginValidator : AbstractValidator<LoginData>
    {
        public LoginValidator(IPlayerRepository repository)
        {
            CascadeMode = CascadeMode.StopOnFirstFailure;

            RuleFor(x => x.Username)
                .NotEmpty()
                .WithMessage(PlayerAccountResponseCode.UsernameShouldNotBeEmpty.ToString());

            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage(PlayerAccountResponseCode.PasswordShouldNotBeEmpty.ToString());

        }
    }

    public class PlayerAccountValidator : AbstractValidator<Data.Player>
    {
        public PlayerAccountValidator(BrandQueries brandQueries, string password)
        {
            CascadeMode = CascadeMode.StopOnFirstFailure;

            RuleFor(x => x)
                .NotNull()
                .WithMessage(PlayerAccountResponseCode.UsernamePasswordCombinationIsNotValid.ToString())
                .WithName("Player")
                ;

            When(player => player != null, () => RuleFor(x => x)
                .Must(player =>
                {
                    var passwordEncrypted = PasswordHelper.EncryptPassword(player.Id, password);
                    return player.PasswordEncrypted == passwordEncrypted;
                })
                .WithMessage(PlayerAccountResponseCode.UsernamePasswordCombinationIsNotValid.ToString())
                .WithName("Player"));

            When(player => player != null, () => RuleFor(x => x.AccountStatus)
                .NotEqual(AccountStatus.Inactive)
                .WithMessage(PlayerAccountResponseCode.NonActive.ToString()));

            When(player => player != null, () => RuleFor(x => x.AccountStatus)
                .NotEqual(AccountStatus.Locked)
                .WithMessage(PlayerAccountResponseCode.AccountLocked.ToString()));

            When(player => player != null, () => RuleFor(x => x.BrandId)
                .Must(brandQueries.IsBrandActive)
                .WithMessage(PlayerAccountResponseCode.InactiveBrand.ToString()));

        }
    }
}
