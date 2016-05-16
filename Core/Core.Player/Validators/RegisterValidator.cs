using System;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Transactions;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Domain.Player;
using AFT.RegoV2.Core.Domain.Player.Data;
using AFT.RegoV2.Core.Player;
using AFT.RegoV2.Core.Player.Data;
using AFT.RegoV2.Domain.Player.Resources;
using ServiceStack.FluentValidation;

namespace AFT.RegoV2.Core.Services.Player.Validators
{
    public class RegisterValidator : AbstractValidator<RegistrationData>
    {
        public RegisterValidator(IPlayerRepository repository, BrandQueries brandQueries)
        {
            CascadeMode = CascadeMode.StopOnFirstFailure;

            RuleFor(p => p.FirstName)
                .Must(v => !string.IsNullOrWhiteSpace(v))
                .WithMessage(PlayerInfoValidatorResponseCodes.FirstNameRequired.ToString());

            RuleFor(p => p.LastName)
                .Must(v => !string.IsNullOrWhiteSpace(v))
                .WithMessage(PlayerInfoValidatorResponseCodes.LastNameRequired.ToString());

            RuleFor(p => p.Email)
                .Must(v => !string.IsNullOrWhiteSpace(v))
                .WithMessage(PlayerInfoValidatorResponseCodes.EmailIsRequired.ToString());

            RuleFor(p => p.PhoneNumber)
                .Must(v => !string.IsNullOrWhiteSpace(v))
                .WithMessage(PlayerInfoValidatorResponseCodes.PhoneNumberIsRequired.ToString());

            RuleFor(p => p.MailingAddressLine1)
                .Must(v => !string.IsNullOrWhiteSpace(v))
                .WithMessage(PlayerInfoValidatorResponseCodes.MailingAddressLine1IsRequired.ToString());

            RuleFor(p => p.MailingAddressCity)
                .Must(v => !string.IsNullOrWhiteSpace(v))
                .WithMessage(PlayerInfoValidatorResponseCodes.MailingAddressCityIsRequired.ToString());

            RuleFor(p => p.MailingAddressPostalCode)
                .Must(v => !string.IsNullOrWhiteSpace(v))
                .WithMessage(PlayerInfoValidatorResponseCodes.MailingAddressPostalCodeIsRequired.ToString());

            RuleFor(p => p.PhysicalAddressLine1)
                .Must(v => !string.IsNullOrWhiteSpace(v))
                .WithMessage(RegisterValidatorResponseCodes.PhysicalAddressLine1Required.ToString());

            RuleFor(p => p.PhysicalAddressCity)
                .Must(v => !string.IsNullOrWhiteSpace(v))
                .WithMessage(RegisterValidatorResponseCodes.PhysicalAddressCityRequired.ToString());

            RuleFor(p => p.PhysicalAddressPostalCode)
                .Must(v => !string.IsNullOrWhiteSpace(v))
                .WithMessage(RegisterValidatorResponseCodes.PhysicalAddressPostalCodeRequired.ToString());

            RuleFor(p => p.CountryCode)
                .Must(v => !string.IsNullOrWhiteSpace(v))
                .WithMessage(PlayerInfoValidatorResponseCodes.CountryCodeIsRequired.ToString());

            RuleFor(p => p.CurrencyCode)
                .Must(v => !string.IsNullOrWhiteSpace(v))
                .WithMessage(RegisterValidatorResponseCodes.CurrencyCodeRequired.ToString());

            RuleFor(p => p.CultureCode)
                .Must(v => !string.IsNullOrWhiteSpace(v))
                .WithMessage(RegisterValidatorResponseCodes.CultureCodeRequired.ToString());

            RuleFor(p => p.Username)
                .Must(v => !string.IsNullOrWhiteSpace(v))
                .WithMessage(RegisterValidatorResponseCodes.UsernameRequired.ToString());

            RuleFor(p => p.Password)
                .Must(v => !string.IsNullOrWhiteSpace(v))
                .WithMessage(RegisterValidatorResponseCodes.PasswordRequired.ToString());

            RuleFor(p => p.PasswordConfirm)
                .Must(v => !string.IsNullOrWhiteSpace(v))
                .WithMessage(RegisterValidatorResponseCodes.PasswordConfirmRequired.ToString());

            RuleFor(p => p.DateOfBirth)
                .Must(v => !string.IsNullOrWhiteSpace(v))
                .WithMessage(PlayerInfoValidatorResponseCodes.DateOfBirthIsMissingOrIncorrect.ToString());

            RuleFor(p => p.BrandId)
                .Must(v => !string.IsNullOrWhiteSpace(v))
                .WithMessage(RegisterValidatorResponseCodes.BrandIdRequired.ToString());

            RuleFor(p => p.Gender)
                .Must(v => !string.IsNullOrWhiteSpace(v))
                .WithMessage(PlayerInfoValidatorResponseCodes.GenderIsRequired.ToString());

            RuleFor(p => p.Title)
                .Must(v => !string.IsNullOrWhiteSpace(v))
                .WithMessage(PlayerInfoValidatorResponseCodes.TitleIsRequired.ToString());

            RuleFor(p => p.ContactPreference)
                .Must(v => !string.IsNullOrWhiteSpace(v))
                .WithMessage(RegisterValidatorResponseCodes.ContactPreferenceRequired.ToString());

            RuleFor(p => p.AccountStatus)
                .Must(v => !string.IsNullOrWhiteSpace(v))
                .WithMessage(RegisterValidatorResponseCodes.AccountStatusRequired.ToString());

            RuleFor(p => p.IdStatus)
                .Must(v => !string.IsNullOrWhiteSpace(v))
                .WithMessage(RegisterValidatorResponseCodes.IdStatusRequired.ToString());

            When(p => !string.IsNullOrWhiteSpace(p.FirstName), () =>
            {
                var firstNameMinLength = CommonPlayerSettings.FirstNameMinLength;
                var firstNameMaxLength = CommonPlayerSettings.FirstNameMaxLength;

                RuleFor(p => p.FirstName)
                    .Length(firstNameMinLength, firstNameMaxLength)
                    .WithMessage(PlayerInfoValidatorResponseCodes.FirstNameLengthIsNotInTheAllowedRange.ToString());

                RuleFor(p => p.FirstName)
                    .Matches(CommonPlayerSettings.FirstNamePattern)
                    .WithMessage(PlayerInfoValidatorResponseCodes.FirstNameFormatIsWrong.ToString());
            });

            When(p => !string.IsNullOrWhiteSpace(p.LastName), () =>
            {
                var lastNameMinLength = CommonPlayerSettings.LastNameMinLength;
                var lastNameMaxLength = CommonPlayerSettings.LastNameMaxLength;

                RuleFor(p => p.LastName)
                    .Length(lastNameMinLength, lastNameMaxLength)
                    .WithMessage(PlayerInfoValidatorResponseCodes.FirstNameLengthIsNotInTheAllowedRange.ToString());

                RuleFor(p => p.LastName)
                    .Matches(CommonPlayerSettings.LastNamePattern)
                    .WithMessage(PlayerInfoValidatorResponseCodes.LastNameFormatIsWrong.ToString());
            });

            When(p => !string.IsNullOrWhiteSpace(p.Email), () =>
            {
                const int emailMaxLength = 50;

                RuleFor(p => p.Email)
                    .Length(0, emailMaxLength)
                    .WithMessage(PlayerInfoValidatorResponseCodes.EmailLengthIsNotInAllowedRange.ToString());

                RuleFor(p => p.Email)
                    .Matches(CommonPlayerSettings.EmailPattern)
                    .WithMessage(PlayerInfoValidatorResponseCodes.EmailFormatIsWrong.ToString());

                RuleFor(p => p.Email)
                    .Must((r, e) => !repository.Players.AsNoTracking().Any(p => p.Email == e && p.BrandId == new Guid(r.BrandId)))
                    .WithMessage(PlayerInfoValidatorResponseCodes.EmailAlreadyExists.ToString());
            });

            When(p => !string.IsNullOrWhiteSpace(p.PhoneNumber), () =>
            {
                const int phoneMinLength = 8, phoneMaxLength = 15;

                RuleFor(p => p.PhoneNumber)
                    .Length(phoneMinLength, phoneMaxLength)
                    .WithMessage(PlayerInfoValidatorResponseCodes.PhoneNumberLengthIsWrong.ToString());

                RuleFor(p => p.PhoneNumber)
                    .Matches(CommonPlayerSettings.PhonePattern)
                    .WithMessage(PlayerInfoValidatorResponseCodes.PhoneNumberFormatIsWrong.ToString());
            });

            When(p => !string.IsNullOrWhiteSpace(p.MailingAddressLine1), () =>
            {
                const int addressMinLength = 1, addressMaxLength = 50;

                RuleFor(p => p.MailingAddressLine1)
                    .Length(addressMinLength, addressMaxLength)
                    .WithMessage(PlayerInfoValidatorResponseCodes.MailingAddressLine1LengthIsIncorrect.ToString());
            });

            When(p => !string.IsNullOrWhiteSpace(p.MailingAddressPostalCode), () =>
            {
                const int zipCodesMinLength = 1, zipCodeMaxLength = 10;

                RuleFor(p => p.MailingAddressPostalCode)
                    .Length(zipCodesMinLength, zipCodeMaxLength)
                    .WithMessage(PlayerInfoValidatorResponseCodes.MailingAddressPostalCodeLengthIsIncorrect.ToString());
            });

            When(p => brandQueries.DoesBrandExist(new Guid(p.BrandId)) && !string.IsNullOrWhiteSpace(p.CountryCode),
                () => RuleFor(p => p.CountryCode)
                    .Must((r, c) => brandQueries.BrandHasCountry(new Guid(r.BrandId), c))
                    .WithMessage(PlayerInfoValidatorResponseCodes.InvalidCountryCode.ToString()));

            When(p => brandQueries.DoesBrandExist(new Guid(p.BrandId)) && !string.IsNullOrWhiteSpace(p.CurrencyCode),
                () => RuleFor(p => p.CurrencyCode)
                    .Must((r, c) => brandQueries.BrandHasCurrency(new Guid(r.BrandId), c))
                    .WithMessage(PlayerInfoValidatorResponseCodes.InvalidCurrencyCode.ToString()));

            When(p => brandQueries.DoesBrandExist(new Guid(p.BrandId)) && !string.IsNullOrWhiteSpace(p.CultureCode),
                () => RuleFor(p => p.CultureCode)
                    .Must((r, c) => brandQueries.BrandHasCulture(new Guid(r.BrandId), c))
                    .WithMessage(RegisterValidatorResponseCodes.InvalidCultureCode.ToString()));

            When(p => !string.IsNullOrWhiteSpace(p.Username), () =>
            {
                const int usernameMinLength = 6;
                const int usernameMaxLength = 12;

                RuleFor(p => p.Username)
                    .Length(usernameMinLength, usernameMaxLength)
                    .WithMessage(RegisterValidatorResponseCodes.UsernameLengthIsNotInAllowedRange.ToString());

                RuleFor(p => p.Username)
                    .Matches(CommonPlayerSettings.UsernamePatter)
                    .WithMessage(RegisterValidatorResponseCodes.UsernameFormatIsWrong.ToString());

                RuleFor(p => p.Username)
                    .Must((r, u) => !repository.Players.AsNoTracking().Any(p => p.Username == u && p.BrandId == new Guid(r.BrandId)))
                    .WithMessage(RegisterValidatorResponseCodes.UsernameAlreadyExists.ToString());
            });

            When(p => !string.IsNullOrWhiteSpace(p.Password), () =>
            {
                int passwordMinLength = CommonPlayerSettings.PasswordMinLength;
                int passwordMaxLength = CommonPlayerSettings.PasswordMaxLength;

                RuleFor(p => p.Password)
                    .Length(passwordMinLength, passwordMaxLength)
                    .WithMessage(RegisterValidatorResponseCodes.PasswordLengthIsNotInItsAllowedRange.ToString());
            });

            When(p => !string.IsNullOrWhiteSpace(p.Password) && !string.IsNullOrWhiteSpace(p.PasswordConfirm), () =>
                RuleFor(p => p.PasswordConfirm)
                    .Must((r, p) => r.Password == p)
                    .WithMessage(RegisterValidatorResponseCodes.ConfirmationPasswordDoesntMatch.ToString())
            );

            When(p => !string.IsNullOrWhiteSpace(p.DateOfBirth), () =>
            {
                RuleFor(p => p.DateOfBirth)
                    .Must(p => ValidatorUtils.ToDateTime(p, "yyyy/MM/dd") != null)
                    .WithMessage(PlayerInfoValidatorResponseCodes.DateOfBirthIsMissingOrIncorrect.ToString());

                RuleFor(p => p.DateOfBirth)
                    .Must(p =>
                    {
                        DateTime? dateTime = ValidatorUtils.ToDateTime(p, "yyyy/MM/dd");
                        if (dateTime == null)
                        {
                            return true;
                        }
                        return dateTime <= DateTime.UtcNow.AddYears(-18);
                    })
                    .WithMessage(PlayerInfoValidatorResponseCodes.AgeIsUnderAllowed.ToString());
            });

            When(p => !string.IsNullOrWhiteSpace(p.BrandId), () => RuleFor(p => p.BrandId)
                .Must(brand => brandQueries.DoesBrandExist(new Guid(brand)))
                .WithMessage(RegisterValidatorResponseCodes.BrandIsUnknown.ToString()));

            When(p => !string.IsNullOrEmpty(p.Gender), () => RuleFor(p => p.Gender)
                .Must(g => Enum.IsDefined(typeof(Gender), g))
                .WithMessage(PlayerInfoValidatorResponseCodes.GenderIsRequired.ToString()));

            When(p => !string.IsNullOrEmpty(p.Title), () => RuleFor(p => p.Title)
                .Must(g => Enum.IsDefined(typeof(Title), g))
                .WithMessage(PlayerInfoValidatorResponseCodes.TitleIsRequired.ToString()));

            When(p => !string.IsNullOrEmpty(p.ContactPreference), () => RuleFor(p => p.ContactPreference)
                .Must(cp => Enum.IsDefined(typeof(ContactMethod), cp))
                .WithMessage(RegisterValidatorResponseCodes.InvalidContractPreference.ToString()));

            When(p => !string.IsNullOrWhiteSpace(p.SecurityQuestionId) && repository.SecurityQuestions.AsNoTracking().FirstOrDefault(q => q.Id.Equals(new Guid(p.SecurityQuestionId))) != null,
                () => RuleFor(p => p.SecurityQuestionId)
                            .Must(
                                (r, q) => repository.SecurityQuestions.AsNoTracking().Any(q1 => q1.Id.Equals(new Guid(r.SecurityQuestionId))))
                            .WithMessage(RegisterValidatorResponseCodes.InvalidSecurityQuestionId.ToString()));

            When(p => !p.IsRegisteredFromAdminSite, () =>
            {
                RuleFor(p => p.SecurityQuestionId)
                    .Must(sid => !string.IsNullOrWhiteSpace(sid))
                    .WithMessage(RegisterValidatorResponseCodes.SecurityQuestionIdRequired.ToString());

                RuleFor(p => p.SecurityAnswer)
                    .Must(ans => !string.IsNullOrWhiteSpace(ans))
                    .WithMessage(RegisterValidatorResponseCodes.SecurityAnswerIsMissing.ToString());
            });


            When(p => !string.IsNullOrEmpty(p.IdStatus), () => RuleFor(p => p.IdStatus)
                .Must(id => Enum.IsDefined(typeof(IdStatus), id))
                .WithMessage(RegisterValidatorResponseCodes.InvalidIdStatus.ToString()));

            When(p => !string.IsNullOrEmpty(p.AccountStatus), () => RuleFor(p => p.AccountStatus)
                .Must(s => Enum.IsDefined(typeof(AccountStatus), s))
                .WithMessage(RegisterValidatorResponseCodes.InvalidPlayerStatus.ToString()));
        }
    }

    public static class ValidatorUtils
    {
        public static DateTime? ToDateTime(string strDate, string format = "yyyy/MM/dd")
        {
            DateTime date;
            if (DateTime.TryParseExact(strDate, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
            {
                return date;
            }
            return null;
        }
    }
}