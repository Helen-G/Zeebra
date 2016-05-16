using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using AFT.RegoV2.Core.Bonus.ApplicationServices;
using AFT.RegoV2.Core.Bonus.Data;
using AFT.RegoV2.Core.Bonus.Data.ViewModels;
using AFT.RegoV2.Core.Common.Utils;
using AutoMapper;

namespace AFT.RegoV2.Core.Bonus.DomainServices
{
    public class BonusBuilder
    {
        private readonly BonusValidator _bonusValidator;
        private readonly TemplateValidator _templateValidator;
        private readonly BonusQueries _bonusQueries;
        private readonly IBonusRepository _bonusRepository;

        static BonusBuilder()
        {
            CreateBonusMap();

            CreateTemplateInfoMap();
            CreateTemplateAvailabilityMap();
            CreateTemplateRulesMap();
            CreateTemplateWageringMap();
            CreateTemplateMap();
        }

        public BonusBuilder(BonusQueries bonusQueries, IBonusRepository bonusRepository)
        {
            _bonusValidator = new BonusValidator(bonusRepository);
            _templateValidator = new TemplateValidator(bonusRepository, bonusQueries);
            _bonusQueries = bonusQueries;
            _bonusRepository = bonusRepository;
        }

        public BuildResult<Data.Bonus> BuildBonus(BonusVM vm)
        {
            var bonus = Mapper.Map<Data.Bonus>(vm);

            var template = _bonusQueries.GetCurrentVersionTemplates().SingleOrDefault(a => a.Id == vm.TemplateId);
            if (template != null)
            {
                var timezoneId = template.Info.Brand.TimezoneId;

                bonus.ActiveFrom = ParseDateTimeStringToBrandOffset(vm.ActiveFrom, timezoneId);
                bonus.ActiveTo = ParseDateTimeStringToBrandOffset(vm.ActiveTo, timezoneId);
                bonus.DurationStart = bonus.DurationStart.DateTime.ToBrandDateTimeOffset(timezoneId);
                bonus.DurationEnd = bonus.DurationEnd.DateTime.ToBrandDateTimeOffset(timezoneId);
            }

            bonus.Template = template;

            return _bonusValidator.ValidateBonus(bonus);
        }

        public BuildResult<Template> BuildTemplate(TemplateVM vm)
        {
            Template template;
            if (vm.Id == Guid.Empty)
            {
                template = Mapper.Map<Template>(vm);
                if (vm.Info.BrandId.HasValue)
                {
                    template.Info.Brand = _bonusRepository.Brands.AsNoTracking().SingleOrDefault(p => p.Id == vm.Info.BrandId.Value);
                }
            }
            else
            {
                template = _bonusQueries.GetCurrentVersionTemplates().SingleOrDefault(t => t.Id == vm.Id && t.Version == vm.Version);
                template = Mapper.Map(vm, template);
            }

            return _templateValidator.ValidateTemplate(template);
        }

        static void CreateBonusMap()
        {
            Mapper.CreateMap<BonusVM, Data.Bonus>()
                .ForMember(dest => dest.ActiveFrom, opt => opt.Ignore())
                .ForMember(dest => dest.ActiveTo, opt => opt.Ignore())
                .ForMember(dest => dest.DurationType,
                    opt => opt.ResolveUsing(data => Enum.Parse(typeof(DurationType), data.DurationType)))
                .AfterMap((vm, bonus) =>
                {
                    bonus.DurationStart = bonus.DurationType == DurationType.Custom
                        ? DateTimeOffset.Parse(vm.DurationStart)
                        : DateTimeOffset.Parse(vm.ActiveFrom);

                    switch (bonus.DurationType)
                    {
                        case DurationType.Custom:
                            bonus.DurationEnd = DateTimeOffset.Parse(vm.DurationEnd);
                            break;
                        case DurationType.None:
                            bonus.DurationEnd = DateTimeOffset.Parse(vm.ActiveTo);
                            break;
                        case DurationType.StartDateBased:
                            {
                                var durationLength = new TimeSpan(vm.DurationDays, vm.DurationHours, vm.DurationMinutes, 0);
                                bonus.DurationEnd = DateTimeOffset.Parse(vm.ActiveFrom).Add(durationLength);
                            }
                            break;
                    }
                });
        }

        static void CreateTemplateInfoMap()
        {
            Mapper.CreateMap<TemplateInfoVM, TemplateInfo>()
                .AfterMap((vm, info) =>
                {
                    var bonusType = (BonusType) Enum.Parse(typeof (BonusType), vm.TemplateType);
                    if (bonusType == BonusType.FirstDeposit)
                    {
                        info.BonusTrigger = Trigger.Deposit;
                        info.DepositKind = DepositKind.First;
                    }
                    else if (bonusType == BonusType.ReloadDeposit)
                    {
                        info.BonusTrigger = Trigger.Deposit;
                        info.DepositKind = DepositKind.Reload;
                    }
                    else if (bonusType == BonusType.HighDeposit)
                    {
                        info.BonusTrigger = Trigger.Deposit;
                        info.DepositKind = DepositKind.High;
                    }
                    else if(bonusType == BonusType.FundIn)
                    {
                        info.BonusTrigger = Trigger.FundIn;
                    }
                    else if (bonusType == BonusType.MobilePlusEmailVerification)
                    {
                        info.BonusTrigger = Trigger.MobilePlusEmailVerification;
                    }
                    else if (bonusType == BonusType.ReferFriend)
                    {
                        info.BonusTrigger = Trigger.ReferFriend;
                    }
                });
        }
        static void CreateTemplateAvailabilityMap()
        {
            Mapper.CreateMap<TemplateAvailabilityVM, TemplateAvailability>()
                .ForMember(dest => dest.VipLevels,
                    opt => opt.ResolveUsing(data => data.VipLevels.Select(code => new BonusVip { Code = code })))
                .ForMember(dest => dest.ExcludeBonuses,
                    opt =>
                        opt.MapFrom(
                            data =>
                                data.ExcludeBonuses.Select(
                                    excludedBonusId => new BonusExclude { ExcludedBonusId = excludedBonusId })))
                .ForMember(dest => dest.ExcludeRiskLevels,
                    opt =>
                        opt.MapFrom(
                            data =>
                                data.ExcludeRiskLevels.Select(
                                    excludedRiskLevelId => new RiskLevelExclude { ExcludedRiskLevelId = excludedRiskLevelId })))
                .ForMember(dest => dest.PlayerRegistrationDateFrom, opt => opt.Ignore())
                .ForMember(dest => dest.PlayerRegistrationDateTo, opt => opt.Ignore());
        }
        static void CreateTemplateRulesMap()
        {
            Mapper.CreateMap<TemplateRulesVM, TemplateRules>()
                .ForMember(dest => dest.FundInWallets,
                    opt =>
                        opt.MapFrom(data => data.FundInWallets.Select(walletId => new BonusFundInWallet { WalletId = walletId })));

            Mapper.CreateMap<RewardTierVM, RewardTier>()
                .ForMember(dest => dest.BonusTiers, opt => opt.MapFrom(data => new List<TierBase>()));
        }
        static void CreateTemplateWageringMap()
        {
            Mapper.CreateMap<TemplateWageringVM, TemplateWagering>()
                .ForMember(dest => dest.GameContributions,
                    opt =>
                        opt.MapFrom(
                            data =>
                                data.GameContributions.Select(
                                    c => new GameContribution { GameId = c.GameId, Contribution = (c.Contribution / 100) })));
        }
        static void CreateTemplateMap()
        {
            Mapper.CreateMap<TemplateVM, Template>()
                .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedOn, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .AfterMap((templateVm, template) =>
                {
                    //Empty id means saving Info. This will discard any data except Info. 
                    if (templateVm.Id == Guid.Empty)
                    {
                        template.Availability = null;
                        template.Rules = null;
                        template.Wagering = null;
                        template.Notification = null;
                    }
                    else
                    {
                        if (templateVm.Rules != null)
                        {
                            foreach (var vmRewardTier in templateVm.Rules.RewardTiers)
                            {
                                var templateRewardTier =
                                    template.Rules.RewardTiers.Single(t => t.CurrencyCode == vmRewardTier.CurrencyCode);
                                if (template.Info.DepositKind == DepositKind.High)
                                {
                                    vmRewardTier.BonusTiers.ForEach(
                                        tier => templateRewardTier.BonusTiers.Add(new HighDepositTier
                                        {
                                            DateCreated = tier.DateCreated,
                                            From = tier.From,
                                            Reward = tier.Reward,
                                            NotificationPercentThreshold = tier.NotificationPercentThreshold / 100
                                        }));
                                }
                                else
                                {
                                    vmRewardTier.BonusTiers.ForEach(tier => templateRewardTier.BonusTiers.Add(new BonusTier
                                    {
                                        DateCreated = tier.DateCreated,
                                        From = tier.From,
                                        To = tier.To,
                                        Reward = tier.Reward,
                                        MaxAmount = tier.MaxAmount
                                    }));
                                    if (templateVm.Rules.RewardType == BonusRewardType.Percentage ||
                                        templateVm.Rules.RewardType == BonusRewardType.TieredPercentage)
                                    {
                                        templateRewardTier.BonusTiers.ForEach(t => t.Reward = (t.Reward / 100));
                                    }
                                }
                            }
                        }

                        if (templateVm.Availability != null)
                        {
                            var timezoneId = template.Info.Brand.TimezoneId;

                            template.Availability.PlayerRegistrationDateFrom =
                                string.IsNullOrEmpty(templateVm.Availability.PlayerRegistrationDateFrom)
                                    ? (DateTimeOffset?)null
                                    : ParseDateTimeStringToBrandOffset(templateVm.Availability.PlayerRegistrationDateFrom,
                                        timezoneId);
                            template.Availability.PlayerRegistrationDateTo =
                                string.IsNullOrEmpty(templateVm.Availability.PlayerRegistrationDateTo)
                                    ? (DateTimeOffset?)null
                                    : ParseDateTimeStringToBrandOffset(templateVm.Availability.PlayerRegistrationDateTo,
                                        timezoneId);
                        }
                        if (template.Info != null)
                        {
                            template.Info.Id = Guid.NewGuid();
                        }
                        if (template.Availability != null)
                        {
                            template.Availability.Id = Guid.NewGuid();
                            template.Availability.VipLevels.ForEach(v => v.Id = Guid.NewGuid());
                            template.Availability.ExcludeBonuses.ForEach(ex => ex.Id = Guid.NewGuid());
                            template.Availability.ExcludeRiskLevels.ForEach(ex => ex.Id = Guid.NewGuid());
                        }
                        if (template.Rules != null)
                        {
                            template.Rules.Id = Guid.NewGuid();
                            template.Rules.RewardTiers.ForEach(rt =>
                            {
                                rt.Id = Guid.NewGuid();
                                rt.BonusTiers.ForEach(bt => bt.Id = Guid.NewGuid());
                            });
                            template.Rules.FundInWallets.ForEach(fw => fw.Id = Guid.NewGuid());
                        }
                        if (template.Wagering != null)
                        {
                            template.Wagering.Id = Guid.NewGuid();
                            template.Wagering.GameContributions.ForEach(gc => gc.Id = Guid.NewGuid());
                        }
                        if (template.Notification != null)
                        {
                            template.Notification.Id = Guid.NewGuid();
                        }
                    }
                })
                .ForAllMembers(opt => opt.Condition(srs => !srs.IsSourceValueNull));
        }

        static DateTimeOffset ParseDateTimeStringToBrandOffset(string dateTime, string brandTimezoneId)
        {
            var dateTimeParsed = DateTime.Parse(dateTime);

            return dateTimeParsed.ToBrandDateTimeOffset(brandTimezoneId);
        }
    }
}