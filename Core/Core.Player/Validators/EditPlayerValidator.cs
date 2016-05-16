using System;
using System.Data.Entity;
using System.Linq;
using AFT.RegoV2.ApplicationServices.Player;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Domain.Player;
using AFT.RegoV2.Core.Domain.Player.Data;
using AFT.RegoV2.Core.Player;
using AFT.RegoV2.Domain.Player.Resources;
using ServiceStack.FluentValidation;

namespace AFT.RegoV2.Core.Services.Player.Validators
{
    public class EditPlayerValidator : AbstractValidator<EditPlayerData>
    {
        public EditPlayerValidator(IPlayerRepository repository, BrandQueries brandQueries)
        {
            CascadeMode = CascadeMode.StopOnFirstFailure;

            RuleFor(p => p.PlayerId)
                   .Must((r, u) => repository.Players.Any(p => p.Id == u))
                   .WithMessage(EditPlayerValidatorResponseCodes.InvalidPlayerId.ToString());

            RuleFor(p => p.Title)
                .Must(v => v.HasValue)
                .WithMessage(PlayerInfoValidatorResponseCodes.TitleIsRequired.ToString());

            RuleFor(p => p.FirstName)
                .Must(v => !string.IsNullOrWhiteSpace(v))
                .WithMessage(PlayerInfoValidatorResponseCodes.FirstNameRequired.ToString());

            RuleFor(p => p.LastName)
                .Must(v => !string.IsNullOrWhiteSpace(v))
                .WithMessage(PlayerInfoValidatorResponseCodes.LastNameRequired.ToString());

            RuleFor(p => p.Email)
                .Must(v => !string.IsNullOrWhiteSpace(v))
                .WithMessage(PlayerInfoValidatorResponseCodes.EmailIsRequired.ToString());

            RuleFor(p => p.Gender)
                .Must(v => v.HasValue)
                .WithMessage(PlayerInfoValidatorResponseCodes.GenderIsRequired.ToString());

            RuleFor(p => p.MailingAddressLine1)
                .Must(v => !string.IsNullOrWhiteSpace(v))
                .WithMessage(PlayerInfoValidatorResponseCodes.MailingAddressLine1IsRequired.ToString());

            RuleFor(p => p.MailingAddressCity)
                .Must(v => !string.IsNullOrWhiteSpace(v))
                .WithMessage(PlayerInfoValidatorResponseCodes.MailingAddressCityIsRequired.ToString());

            RuleFor(p => p.MailingAddressPostalCode)
                .Must(v => !string.IsNullOrWhiteSpace(v))
                .WithMessage(PlayerInfoValidatorResponseCodes.MailingAddressPostalCodeIsRequired.ToString());

            RuleFor(p => p.CountryCode)
                .Must(v => !string.IsNullOrWhiteSpace(v))
                .WithMessage(PlayerInfoValidatorResponseCodes.CountryCodeIsRequired.ToString());

            RuleFor(p => p.PhoneNumber)
                .Must(v => !string.IsNullOrWhiteSpace(v))
                .WithMessage(PlayerInfoValidatorResponseCodes.PhoneNumberIsRequired.ToString());

            When(p => !p.AccountAlertEmail, () =>
            {
                RuleFor(p => p.AccountAlertSms)
                    .Must(v => v)
                    .WithMessage(PlayerInfoValidatorResponseCodes.AccountAlertIsRequired.ToString());
            });

            When(p => !p.AccountAlertSms, () =>
            {
                RuleFor(p => p.AccountAlertEmail)
                    .Must(v => v)
                    .WithMessage(PlayerInfoValidatorResponseCodes.AccountAlertIsRequired.ToString());
            });

            When(p => !p.MarketingAlertEmail && !p.MarketingAlertPhone, () =>
            {
                RuleFor(p => p.MarketingAlertSms)
                    .Must(v => v)
                    .WithMessage(PlayerInfoValidatorResponseCodes.MarketingAlertIsRequired.ToString());
            });

            When(p => !p.MarketingAlertSms && !p.MarketingAlertPhone, () =>
            {
                RuleFor(p => p.MarketingAlertEmail)
                    .Must(v => v)
                    .WithMessage(PlayerInfoValidatorResponseCodes.MarketingAlertIsRequired.ToString());
            });

            When(p => !p.MarketingAlertSms && !p.MarketingAlertEmail, () =>
            {
                RuleFor(p => p.MarketingAlertPhone)
                    .Must(v => v)
                    .WithMessage(PlayerInfoValidatorResponseCodes.MarketingAlertIsRequired.ToString());
            });

            When(p => !string.IsNullOrWhiteSpace(p.FirstName), () =>
            {
                int firstNameMinLength = CommonPlayerSettings.FirstNameMinLength;
                int firstNameMaxLength = CommonPlayerSettings.FirstNameMaxLength;

                RuleFor(p => p.FirstName)
                    .Length(firstNameMinLength, firstNameMaxLength)
                    .WithMessage(PlayerInfoValidatorResponseCodes.FirstNameLengthIsNotInTheAllowedRange.ToString());

                RuleFor(p => p.FirstName)
                    .Matches(CommonPlayerSettings.FirstNamePattern)
                    .WithMessage(PlayerInfoValidatorResponseCodes.FirstNameFormatIsWrong.ToString());
            });

            When(p => !string.IsNullOrWhiteSpace(p.LastName), () =>
            {
                int lastNameMinLength = CommonPlayerSettings.LastNameMinLength;
                int lastNameMaxLength = CommonPlayerSettings.LastNameMaxLength;

                RuleFor(p => p.LastName)
                    .Length(lastNameMinLength, lastNameMaxLength)
                    .WithMessage(PlayerInfoValidatorResponseCodes.LastNameLengthIsNotInTheAllowedRange.ToString());

                RuleFor(p => p.LastName)
                    .Matches(CommonPlayerSettings.LastNamePattern)
                    .WithMessage(PlayerInfoValidatorResponseCodes.LastNameFormatIsWrong.ToString());
            });

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

            When(p => !string.IsNullOrWhiteSpace(p.Email), () =>
            {
                const int emailMaxLength = 50;

                RuleFor(p => p.Email)
                    .Length(0, emailMaxLength)
                    .WithMessage(PlayerInfoValidatorResponseCodes.EmailLengthIsNotInAllowedRange.ToString());

                RuleFor(p => p.Email)
                    .EmailAddress()
                    .WithMessage(PlayerInfoValidatorResponseCodes.EmailFormatIsWrong.ToString());

                RuleFor(p => p.Email)
                    .Must((r, e) => !repository.Players.AsNoTracking().Any(p => p.Email == e
                        && p.BrandId == repository.Players.FirstOrDefault(player => player.Id == r.PlayerId).BrandId
                        && p.Id != r.PlayerId))
                    .WithMessage(PlayerInfoValidatorResponseCodes.EmailAlreadyExists.ToString());
            });

            When(p => !string.IsNullOrWhiteSpace(p.PhoneNumber), () =>
            {
                const int phoneMinLength = 8;
                const int phoneMaxLength = 15;

                RuleFor(p => p.PhoneNumber)
                    .Length(phoneMinLength, phoneMaxLength)
                    .WithMessage(PlayerInfoValidatorResponseCodes.PhoneNumberLengthIsWrong.ToString());

                RuleFor(p => p.PhoneNumber)
                    .Matches(@"^((\\+)|(00)|(\\*)|())[0-9]{3,14}((\\#)|())$")
                    .WithMessage(PlayerInfoValidatorResponseCodes.PhoneNumberFormatIsWrong.ToString());
            });

            When(p => !string.IsNullOrWhiteSpace(p.MailingAddressLine1), () =>
            {
                const int addressMinLength = 1;
                const int addressMaxLength = 50;

                RuleFor(p => p.MailingAddressLine1)
                    .Length(addressMinLength, addressMaxLength)
                    .WithMessage(PlayerInfoValidatorResponseCodes.MailingAddressLine1LengthIsIncorrect.ToString());
            });

            When(p => !string.IsNullOrWhiteSpace(p.MailingAddressPostalCode), () =>
            {
                const int zipCodesMinLength = 1;
                const int zipCodeMaxLength = 10;

                RuleFor(p => p.MailingAddressPostalCode)
                    .Length(zipCodesMinLength, zipCodeMaxLength)
                    .WithMessage(PlayerInfoValidatorResponseCodes.MailingAddressPostalCodeLengthIsIncorrect.ToString());
            });

            When(p => !string.IsNullOrWhiteSpace(p.CountryCode) && brandQueries.DoesBrandExist(repository.Players.FirstOrDefault(player => player.Id == p.PlayerId).BrandId), () =>
                RuleFor(p => p.CountryCode)
                    .Must((r, c) => brandQueries.BrandHasCountry(repository.Players.FirstOrDefault(player => player.Id == r.PlayerId).BrandId, c))
                    .WithMessage(PlayerInfoValidatorResponseCodes.InvalidCountryCode.ToString()));

            When(p => !string.IsNullOrWhiteSpace(p.CurrencyCode) && brandQueries.DoesBrandExist(repository.Players.FirstOrDefault(player => player.Id == p.PlayerId).BrandId), () =>
                RuleFor(p => p.CurrencyCode)
                    .Must((r, c) => brandQueries.BrandHasCurrency(repository.Players.FirstOrDefault(player => player.Id == r.PlayerId).BrandId, c))
                    .WithMessage(PlayerInfoValidatorResponseCodes.InvalidCurrencyCode.ToString()));
        }
    }
}