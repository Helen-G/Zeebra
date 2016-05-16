using System;
using System.Linq;
using AFT.RegoV2.Core.Bonus.Data;
using AFT.RegoV2.Core.Bonus.Data.ViewModels;
using AFT.RegoV2.Core.Bonus.Resources;
using AFT.RegoV2.Core.Common.Utils;
using FluentValidation;

namespace AFT.RegoV2.Core.Bonus.DomainServices
{
    internal class BonusValidator : AbstractValidator<Data.Bonus>
    {
        public BonusValidator(IBonusRepository repository)
        {
            CascadeMode = CascadeMode.Continue;

            When(bonus => bonus.Id != Guid.Empty, () =>
            {
                Func<Data.Bonus, IQueryable<Data.Bonus>> bonuses = bonus => repository.Bonuses.Where(b => b.Id == bonus.Id);
                RuleFor(bonus => bonus)
                    .Must(bonus => bonuses(bonus).Any())
                    .WithMessage(ValidatorMessages.BonusDoesNotExist)
                    .WithName("Bonus");

                RuleFor(bonus => bonus)
                    .Must(bonus => bonus.Version == bonuses(bonus).Max(b => b.Version))
                    .WithMessage(ValidatorMessages.BonusVersionIsNotCurrent)
                    .WithName("Bonus")
                    .When(bonus => bonuses(bonus).Any());
            });

            RuleFor(bonus => bonus.Name)
                .NotEmpty()
                .WithMessage(ValidatorMessages.NameIsNotSpecified);
            When(bonus => string.IsNullOrWhiteSpace(bonus.Name) == false, () => RuleFor(bonus => bonus.Name)
                .Matches(@"^[a-zA-Z0-9_\-\s]*$")
                .WithMessage(ValidatorMessages.BonusNameIsInvalid)
                .Length(1, 50)
                .WithMessage(ValidatorMessages.NameLengthIsInvalid, 1, 50)
                .Must((bonus, name) =>
                {
                    if (bonus.Id == Guid.Empty)
                    {
                        return repository.Bonuses.Any(b => b.Name == name) == false;
                    }

                    return repository.Bonuses.Any(b => b.Name == name && b.Id != bonus.Id) == false;
                })
                .WithMessage(ValidatorMessages.NameIsNotUnique));

            RuleFor(bonus => bonus.Template)
                .NotNull()
                .WithMessage(ValidatorMessages.BonusTemplateIsNotAssigned)
                .OverridePropertyName("TemplateId");

            RuleFor(bonus => bonus.Description)
                .Length(1, 200)
                .WithMessage(ValidatorMessages.DescriptionLengthIsInvalid, 1, 200)
                .When(bonus => string.IsNullOrWhiteSpace(bonus.Description) == false);

            RuleFor(bonus => bonus.DaysToClaim)
                .GreaterThanOrEqualTo(0)
                .WithMessage(ValidatorMessages.BonusDaysToClaimIsNegative);

            When(b => b.Template != null, () =>
            {
                RuleFor(bonus => bonus.Code)
                    .NotEmpty()
                    .When(bonus => bonus.Template.Info.Mode == IssuanceMode.AutomaticWithCode)
                    .WithMessage(ValidatorMessages.BonusCodeIsNotSpecified);
                When(bonus => string.IsNullOrWhiteSpace(bonus.Code) == false, () => RuleFor(bonus => bonus.Code)
                    .Matches(@"^[a-zA-Z0-9]*$")
                    .WithMessage(ValidatorMessages.BonusCodeIsInvalid)
                    .Length(1, 20)
                    .WithMessage(ValidatorMessages.BonusCodeLengthIsInvalid, 1, 20)
                    .Must((bonus, code) =>
                    {
                        if (bonus.Id == Guid.Empty)
                        {
                            return repository.Bonuses.Any(b => b.Code == code) == false;
                        }

                        return repository.Bonuses.Any(b => b.Code == code && b.Id != bonus.Id) == false;
                    })
                    .WithMessage(ValidatorMessages.BonusCodeIsNotUnique));

                RuleFor(bonus => bonus.ActiveTo)
                    .Must((bonus, activeTo) => activeTo >= bonus.ActiveFrom)
                    .WithMessage(ValidatorMessages.BonusActivityRangeIsInvalid)
                    .Must((bonus, activeTo) => activeTo > DateTimeOffset.Now.ToBrandOffset(bonus.Template.Info.Brand.TimezoneId))
                    .WithMessage(ValidatorMessages.BonusActiveToIsInvalid);

                When(bonus => bonus.DurationType != DurationType.None, () =>
                {
                    Func<Data.Bonus, DateTimeOffset, bool> isNonZeroLengthDuration = (bonus, durationEnd) => durationEnd - bonus.DurationStart > TimeSpan.Zero;

                    RuleFor(bonus => bonus.DurationEnd)
                        .Must(isNonZeroLengthDuration)
                        .WithMessage(ValidatorMessages.BonusDurationIsZeroLength)
                        .OverridePropertyName("DurationType");

                    When(bonus => isNonZeroLengthDuration(bonus, bonus.DurationEnd), () =>
                    {
                        RuleFor(bonus => bonus.DurationEnd)
                            .Must((bonus, durationEnd) => durationEnd > bonus.DurationStart && durationEnd <= bonus.ActiveTo)
                            .WithMessage(ValidatorMessages.BonusDurationDaterangeIsIncorrect)
                            .When(bonus => bonus.DurationType == DurationType.StartDateBased)
                            .OverridePropertyName("DurationType");

                        RuleFor(bonus => bonus.DurationEnd)
                            .Must((bonus, durationEnd) =>
                                durationEnd > bonus.DurationStart &&
                                durationEnd <= bonus.ActiveTo &&
                                bonus.DurationStart >= bonus.ActiveFrom)
                            .WithMessage(ValidatorMessages.BonusDurationDaterangeIsIncorrect)
                            .When(bonus => bonus.DurationType == DurationType.Custom)
                            .OverridePropertyName("DurationType");
                    });
                });
            });
        }

        public BuildResult<Data.Bonus> ValidateBonus(Data.Bonus instance)
        {
            return new BuildResult<Data.Bonus>(instance, Validate(instance));
        }
    }
}