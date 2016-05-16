using System;
using System.Linq;
using AFT.RegoV2.Core.Domain.Player;
using AFT.RegoV2.Core.Domain.Player.Data;
using AFT.RegoV2.Core.Player;
using AFT.RegoV2.Domain.Player.Resources;
using ServiceStack.FluentValidation;

namespace AFT.RegoV2.Core.Services.Player.Validators
{
    public class CommonPlayerSettings
    {
        public static int PasswordMinLength { get { return 6; } }
        public static int PasswordMaxLength { get { return 12; } }

        public static int FirstNameMinLength { get { return 1; } }
        public static int FirstNameMaxLength { get { return 50; } }
        public static string FirstNamePattern { get { return @"^[a-zA-Z0-9\-\'_\.]+$"; } }

        public static int LastNameMinLength { get { return 1; } }
        public static int LastNameMaxLength { get { return 20; } }
        public static string LastNamePattern { get { return @"^[a-zA-Z0-9\-\'_\.]+$"; } }

        public static string EmailPattern { get { return @"^([\!#\$%&'\*\+/\=?\^`\{\|\}~a-zA-Z0-9_-]+[\.]?)+[\!#\$%&'\*\+/\=?\^`\{\|\}~a-zA-Z0-9_-]+@{1}((([0-9A-Za-z_-]+)([\.]{1}[0-9A-Za-z_-]+)*\.{1}([A-Za-z]){1,6})|(([0-9]{1,3}[\.]{1}){3}([0-9]{1,3}){1}))$"; } }

        public static string PhonePattern { get { return @"^((\\+)|(00)|(\\*)|())[0-9]{3,14}((\\#)|())$"; } }

        public static string UsernamePatter { get { return @"^[A-Za-z0-9-_\.\']*$"; } }
    }

    public class SendNewPasswordValidator : AbstractValidator<SendNewPasswordData>
    {
        public SendNewPasswordValidator(IPlayerRepository repository)
        {
            When(p => !string.IsNullOrWhiteSpace(p.NewPassword), () =>
            {
                int passwordMinLength = CommonPlayerSettings.PasswordMinLength;
                int passwordMaxLength = CommonPlayerSettings.PasswordMaxLength;

                string passwordIsNotWithinItsAllowedRangeErrorMessage =
                    string.Format(Messages.PasswordIsNotWithinItsAllowedRangeErrorMessageFormat, passwordMinLength, passwordMaxLength);

                RuleFor(p => p.NewPassword)
                    .Length(passwordMinLength, passwordMaxLength)
                    .WithMessage(passwordIsNotWithinItsAllowedRangeErrorMessage);
            });

            RuleFor(p => p.PlayerId)
                    .Must((r, u) => repository.Players.Any(p => p.Id == u))
                    .WithMessage(Messages.InvalidPlayerId);
        }
    }

    public static class PasswordGenerator
    {
        public static string Create()
        {
            var passwordMinLength = CommonPlayerSettings.PasswordMinLength;
            var passwordMaxLength = CommonPlayerSettings.PasswordMaxLength;
            var alpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890".ToCharArray();
            var r = new Random();
            var passwordLength = r.Next(passwordMinLength, passwordMaxLength);
            var pass = new char[passwordLength];
            for (var i = 0; i < passwordLength; i++)
            {
                pass[i] = alpha[r.Next(alpha.Length - 1)];
            }
            return new string(pass);
        }
    }
}