using System;
using System.Linq;
using AFT.RegoV2.Core.Bonus.ApplicationServices;
using AFT.RegoV2.Core.Bonus.Data;
using AFT.RegoV2.Core.Bonus.Data.ViewModels;
using AFT.RegoV2.Core.Bonus.Resources;
using FluentValidation;

namespace AFT.RegoV2.Core.Bonus.DomainServices
{
    internal class TemplateValidator : AbstractValidator<Template>
    {
        public TemplateValidator(IBonusRepository repository, BonusQueries queries)
        {
            CascadeMode = CascadeMode.Continue;

            When(template => template.Id != Guid.Empty, () =>
            {
                RuleFor(template => template)
                    .Must(template => queries.GetCurrentVersionTemplates().SingleOrDefault(t => t.Id == template.Id) != null)
                    .WithMessage(ValidatorMessages.TemplateDoesNotExist)
                    .WithName("Template");
                RuleFor(template => template)
                    .Must(template => queries.GetCurrentVersionTemplates().SingleOrDefault(t => t.Id == template.Id && t.Version == template.Version) != null)
                    .WithMessage(ValidatorMessages.TemplateVersionIsNotCurrent)
                    .WithName("Template");
            });

            When(template => template.Info != null, () => ValidateInfo(repository));
            When(template => template.Availability != null, () => ValidateAvailability(repository));
            When(template => template.Rules != null, ValidateRules);
            When(template => template.Wagering != null, () => ValidateWagering(repository));

            RuleFor(template => template)
                .Must(template => queries.GetBonusesUsingTemplate(template).Any(bonus => bonus.IsActive) == false)
                .WithMessage(ValidatorMessages.AllBonusesShouldBeInactive)
                .WithName("Template")
                .When(template => template.Status == TemplateStatus.Complete);
        }

        private void ValidateInfo(IBonusRepository repository)
        {
            RuleFor(template => template.Info.Name)
                    .NotEmpty()
                    .WithMessage(ValidatorMessages.NameIsNotSpecified);
            When(template => string.IsNullOrWhiteSpace(template.Info.Name) == false, () => RuleFor(template => template.Info.Name)
                .Matches(@"^[a-zA-Z0-9_\-\s]*$")
                .WithMessage(ValidatorMessages.TemplateNameIsInvalid)
                .Length(1, 50)
                .WithMessage(ValidatorMessages.NameLengthIsInvalid, 1, 50)
                .Must((template, name) =>
                {
                    var templates = repository.Templates.Where(t => t.Info.Name == name);
                    if (template.Info.Brand != null)
                    {
                        templates = templates.Where(t => t.Info.Brand.Id == template.Info.Brand.Id);
                    }
                    if (template.Id != Guid.Empty)
                    {
                        templates = templates.Where(t => t.Id != template.Id);
                    }

                    return templates.Any() == false;
                })
                .WithMessage(ValidatorMessages.NameIsNotUnique));

            RuleFor(template => template.Info.Description)
                .Length(1, 200)
                .WithMessage(ValidatorMessages.DescriptionLengthIsInvalid, 1, 200)
                .When(template => string.IsNullOrWhiteSpace(template.Info.Description) == false);

            RuleFor(template => template.Info.Brand)
                .NotNull()
                .WithMessage(ValidatorMessages.TemplateBrandDoesNotExist)
                .OverridePropertyName("Info.BrandId");

            RuleFor(template => template.Info.WalletTemplateId)
                .NotEmpty()
                .WithMessage(ValidatorMessages.TemplateReceivingWalletIsNotSpecified);

            RuleFor(template => template.Info.WalletTemplateId)
                .Must((template, walletId) => template.Info.Brand.WalletTemplates.Select(c => c.Id).Contains(walletId))
                .WithMessage(ValidatorMessages.TemplateReceivingWalletIsInvalid)
                .When(template => template.Info.Brand != null && template.Info.WalletTemplateId != Guid.Empty);

            RuleFor(template => template.Info)
                .Must(info =>
                {
                    var bonusTrigger = info.BonusTrigger;
                    var isReloadOrFirstDeposit = bonusTrigger == Trigger.Deposit &&
                                        info.DepositKind != DepositKind.High;
                    var isFundIn = bonusTrigger == Trigger.FundIn;
                    if (isFundIn || isReloadOrFirstDeposit)
                    {
                        return true;
                    }

                    return info.Mode != IssuanceMode.AutomaticWithCode;
                })
                .WithMessage(ValidatorMessages.TemplateModeIsIncorrect)
                .OverridePropertyName("Info.Mode");
        }

        private void ValidateAvailability(IBonusRepository repository)
        {
            RuleFor(template => template.Availability.RedemptionsLimit)
                .GreaterThanOrEqualTo(0)
                .WithMessage(ValidatorMessages.TemplatePlayerRedemptionLimitIsNegative);

            RuleFor(template => template.Availability.ParentBonusId)
                .Must(id => repository.Bonuses.Any(a => a.Id == id))
                .WithMessage(ValidatorMessages.TemplateParentBonusDoesNotExist)
                .When(bonus => bonus.Availability.ParentBonusId.HasValue);

            RuleFor(template => template.Availability.PlayerRegistrationDateTo)
                .Must((template, registrationDateTo) => registrationDateTo >= template.Availability.PlayerRegistrationDateFrom)
                .WithMessage(ValidatorMessages.TemplatePlayerRegistrationDateRangeIsInvalid)
                .When(template => template.Availability.PlayerRegistrationDateFrom.HasValue &&
                    template.Availability.PlayerRegistrationDateTo.HasValue);

            RuleFor(template => template.Availability.WithinRegistrationDays)
                .GreaterThanOrEqualTo(0)
                .WithMessage(ValidatorMessages.TemplateWithinRegistrationDaysIsNegative);

            RuleFor(template => template.Availability.ExcludeBonuses)
                .Must((template, excludes) => excludes.Select(a => a.ExcludedBonusId).Contains(template.Availability.ParentBonusId.Value) == false)
                .WithMessage(ValidatorMessages.TemplateBonusExcludesContainsParentBonus)
                .When(template => template.Availability.ParentBonusId.HasValue);

            RuleFor(template => template.Availability.PlayerRedemptionsLimit)
                .GreaterThanOrEqualTo(0)
                .WithMessage(ValidatorMessages.TemplatePlayerRedemptionsIsNegative);

            RuleFor(template => template.Availability.VipLevels)
                .Must((template, vips) =>
                {
                    var brandVips = template.Info.Brand.Vips.Select(v => v.Code);
                    return vips.Select(v => v.Code).All(brandVips.Contains);
                })
                .WithMessage(ValidatorMessages.TemplateVipsAreInvalid);

            RuleFor(template => template.Availability.ExcludeRiskLevels)
                .Must((template, riskLevels) =>
                {
                    var rls = template.Info.Brand.RiskLevels.Select(x => x.Id);
                    return riskLevels.Select(v => v.ExcludedRiskLevelId).All(rls.Contains);
                })
                .WithMessage(ValidatorMessages.TemplateRiskLevelsAreInvalid);
        }

        private void ValidateRules()
        {
            RuleFor(template => template.Rules.RewardTiers)
                .NotEmpty()
                .WithMessage(ValidatorMessages.TemplateCurrenciesAreEmpty);

            RuleFor(template => template.Rules.RewardType)
                .Must((template, type) =>
                {
                    switch (template.Info.BonusTrigger)
                    {
                        case Trigger.Deposit:
                            if (template.Info.DepositKind == DepositKind.High)
                            {
                                return (type == BonusRewardType.TieredAmount);
                            }
                            return true;
                        case Trigger.FundIn:
                            return true;
                        case Trigger.MobilePlusEmailVerification:
                            return (type == BonusRewardType.Amount);
                        case Trigger.ReferFriend:
                            return (type == BonusRewardType.TieredAmount);
                        default:
                            return false;
                    }
                })
                .WithMessage(ValidatorMessages.TemplateRewardTypeIsNotSupported);

            RuleFor(template => template.Rules.FundInWallets)
                .NotEmpty()
                .WithMessage(ValidatorMessages.TemplateNoFundInWallets)
                .When(template => template.Info.BonusTrigger == Trigger.FundIn);

            RuleFor(template => template.Rules.FundInWallets)
                .Must((template, wallets) =>
                {
                    var brandWallets = template.Info.Brand.WalletTemplates.Select(c => c.Id);
                    return wallets.Select(w => w.WalletId).All(brandWallets.Contains);
                })
                .WithMessage(ValidatorMessages.TemplateFundInWalletsAreInvalid)
                .When(template => template.Info.BonusTrigger == Trigger.FundIn && template.Rules.FundInWallets.Any());

            When(template => template.Rules.RewardTiers.Any(), () =>
            {
                RuleFor(template => template.Rules.RewardTiers)
                    .Must(rewardTiers => rewardTiers.All(t => t.BonusTiers.Any()))
                    .WithMessage(ValidatorMessages.TemplateBonusTierAtLeastOneIsRequired);

                RuleFor(template => template.Rules.RewardTiers)
                    .Must(rewardTiers => rewardTiers.All(t => t.RewardAmountLimit >= 0))
                    .WithMessage(ValidatorMessages.TemplateRewardLimitIsNegative);

                RuleFor(template => template.Rules.RewardTiers)
                    .Must((template, rewardTiers) =>
                    {
                        var brandCurrencies = template.Info.Brand.Currencies.Select(c => c.Code);
                        return rewardTiers.Select(rw => rw.CurrencyCode).All(brandCurrencies.Contains);
                    })
                    .WithMessage(ValidatorMessages.TemplateRewardCurrenciesAreInvalid)
                    .When(template => template.Info.Brand != null);

                When(template => template.Rules.RewardTiers.All(t => t.BonusTiers.Any()), () =>
                {
                    RuleFor(template => template.Rules.RewardTiers)
                        .Must(rewardTiers =>
                        {
                            var isValid = true;
                            foreach (var rewardTier in rewardTiers)
                            {
                                var tiers = rewardTier.HighDepositTiers;
                                var currencyTiersAreInvalid = tiers.Any(t1 => tiers.Any(t2 => t1 != t2 && t1.From == t2.From));
                                if (currencyTiersAreInvalid) isValid = false;
                            }
                            return isValid;
                        })
                        .WithMessage(ValidatorMessages.TemplateDuplicateHighDepositTiers)
                        .When(template => template.Info.DepositKind == DepositKind.High);

                    When(template => template.Info.DepositKind != DepositKind.High, () =>
                    {
                        When(template => template.Rules.RewardType == BonusRewardType.Amount || template.Rules.RewardType == BonusRewardType.Percentage,
                            () =>
                            {
                                RuleFor(template => template.Rules.RewardTiers)
                                    .Must(rewardTiers => rewardTiers.All(t => t.Tiers.Single().Reward > 0))
                                    .WithMessage(ValidatorMessages.TemplateRewardValueIsInvalid);

                                RuleFor(template => template.Rules.RewardTiers)
                                    .Must(rewardTiers => rewardTiers.All(t => t.Tiers.Single().MaxAmount >= 0))
                                    .WithMessage(ValidatorMessages.TemplateTransactionAmountLimitIsNegative);
                            });
                        When(template => template.Rules.RewardType == BonusRewardType.TieredAmount || template.Rules.RewardType == BonusRewardType.TieredPercentage,
                            () =>
                            {
                                RuleFor(template => template.Rules.RewardTiers)
                                    .Must(rewardTiers => rewardTiers.All(t => t.Tiers.All(r => r.From > 0 && r.From <= (r.To ?? decimal.MaxValue) && r.Reward > 0 && r.MaxAmount >= 0)))
                                    .WithMessage(ValidatorMessages.TemplateBonusTierIsInvalid);

                                RuleFor(template => template.Rules.RewardTiers)
                                    .Must(rewardTiers =>
                                    {
                                        var isValid = true;
                                        foreach (var rewardTier in rewardTiers)
                                        {
                                            var tiers = rewardTier.Tiers.OrderBy(t => t.DateCreated).ToList();
                                            for (var i = 0; i < tiers.Count() - 1; i++)
                                            {
                                                var thisTier = tiers[i];
                                                var nextTier = tiers[i + 1];
                                                if (thisTier.From >= nextTier.From)
                                                {
                                                    isValid = false;
                                                    break;                                                    
                                                }
                                            }
                                        }
                                        return isValid;
                                    })
                                    .WithMessage(ValidatorMessages.TemplateBonusTiersOverlap)
                                    .When(template => template.Rules.RewardTiers.All(t => t.Tiers.All(r => r.From > 0 && r.From <= (r.To ?? decimal.MaxValue) && r.Reward > 0 && r.MaxAmount >= 0)));
                            });
                    });
                });
            });
        }

        private void ValidateWagering(IBonusRepository repository)
        {
            RuleFor(template => template.Wagering.Multiplier)
                .GreaterThanOrEqualTo(0)
                .WithMessage(ValidatorMessages.TemplateWageringConditionIsNegative);

            RuleFor(template => template.Wagering.Multiplier)
                .GreaterThan(0)
                .WithMessage(ValidatorMessages.TemplateWageringConditionIsZeroOrLess)
                .When(template => template.Wagering.HasWagering);

            RuleFor(template => template.Wagering.Threshold)
                .GreaterThanOrEqualTo(0)
                .WithMessage(ValidatorMessages.TemplateWageringThresholdIsNegative);

            RuleFor(template => template.Wagering.GameContributions)
                .Must(contributions => contributions.All(c => c.Contribution >= 0))
                .WithMessage(ValidatorMessages.TemplateOneOfGameContributionsIsNegative);

            RuleFor(template => template.Wagering.GameContributions)
                .Must(contributions =>
                {
                    var gameIds = contributions.Select(c => c.GameId);
                    var dbGameIds = repository.Games.Select(g => g.Id);
                    return gameIds.All(dbGameIds.Contains);
                })
                .WithMessage(ValidatorMessages.TemplateOneOfGameContributionsPointsToInvalidGame)
                .When(template => template.Wagering.GameContributions.Any());

            RuleFor(template => template.Wagering.Method)
                .Must(method => method == WageringMethod.Bonus)
                .WithMessage(ValidatorMessages.TemplateWageringMethodIsNotSupportedByBonusTrigger)
                .When(template =>
                    template.Info.BonusTrigger == Trigger.ReferFriend ||
                    template.Info.BonusTrigger == Trigger.MobilePlusEmailVerification ||
                    (template.Info.BonusTrigger == Trigger.Deposit && template.Info.DepositKind == DepositKind.High));

            RuleFor(template => template.Wagering.IsAfterWager)
                .Must(isAfterWager => isAfterWager == false)
                .WithMessage(ValidatorMessages.TemplateWageringIsAfterWagerIsNotApplicable)
                .When(template => template.Wagering.HasWagering == false);
        }

        public BuildResult<Template> ValidateTemplate(Template instance)
        {
            return new BuildResult<Template>(instance, Validate(instance));
        }
    }
}