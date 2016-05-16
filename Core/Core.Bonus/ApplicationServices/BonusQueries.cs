using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using AFT.RegoV2.ApplicationServices.Security;
using AFT.RegoV2.Core.Bonus.Data;
using AFT.RegoV2.Core.Bonus.Data.ViewModels;
using AFT.RegoV2.Core.Bonus.DomainServices;
using AFT.RegoV2.Core.Bonus.Entities;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Bonus;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Shared;
using FluentValidation.Results;
using BonusRedemption = AFT.RegoV2.Core.Bonus.Data.BonusRedemption;
using Player = AFT.RegoV2.Core.Bonus.Entities.Player;

namespace AFT.RegoV2.Core.Bonus.ApplicationServices
{
    public class BonusQueries : MarshalByRefObject, IBonusQueries
    {
        private readonly IBonusRepository _repository;
        private readonly IWalletQueries _walletQueries;

        public BonusQueries(IBonusRepository repository, IWalletQueries walletQueries)
        {
            _repository = repository;
            _walletQueries = walletQueries;
        }

        internal Data.Bonus[] GetQualifiedBonuses(Guid playerId, Trigger type, RedemptionParams redemptionParams = null)
        {
            redemptionParams = redemptionParams ?? new RedemptionParams();
            var player = _repository.GetLockedPlayer(playerId);
            return GetCurrentVersionBonuses(player.Data.Brand.Id)
                .Where(x => x.Template.Info.BonusTrigger == type)
                .ToList()
                .Select(x => new Entities.Bonus(x))
                .Where(bonus => bonus.QualifiesFor(player, QualificationPhase.Redemption, redemptionParams))
                .Select(x => x.Data)
                .ToArray();
        }
        internal Data.Bonus[] GetQualifiedHighDepositBonuses(Player player)
        {
            return GetCurrentVersionBonuses(player.Data.Brand.Id)
                .Where(x =>
                    x.Template.Info.BonusTrigger == Trigger.Deposit &&
                    x.Template.Info.DepositKind == DepositKind.High)
                .ToList()
                .Select(x => new Entities.Bonus(x))
                .Where(bonus => bonus.QualifiesFor(player, QualificationPhase.Redemption, new RedemptionParams()))
                .Select(x => x.Data)
                .ToArray();
        }
        internal List<Guid> GetQualifiedBonusIds(Guid playerId, string bonusCode, Trigger type, RedemptionParams redemptionParams)
        {
            if (string.IsNullOrWhiteSpace(bonusCode))
            {
                var bonuses = GetQualifiedBonuses(playerId, type, redemptionParams)
                    .Where(b =>
                        b.Template.Info.Mode == IssuanceMode.Automatic ||
                        b.Template.Info.Mode == IssuanceMode.ManualByPlayer);
                if (type == Trigger.Deposit)
                {
                    bonuses = bonuses.Where(b => b.Template.Info.DepositKind != DepositKind.High);
                }
                return bonuses
                    .Select(b => b.Id)
                    .ToList();
            }

            return GetQualificationFailures(playerId, bonusCode, redemptionParams, type).Any() ?
                new List<Guid>() :
                new List<Guid> { _repository.GetLockedBonus(bonusCode).Data.Id };
        }
        internal IQueryable<Data.Bonus> GetCurrentVersionBonuses(Guid brandId)
        {
            return GetCurrentVersionBonuses().Where(bonus => bonus.Template.Info.Brand.Id == brandId);
        }
        internal IQueryable<Data.Bonus> GetBonusesUsingTemplate(Template template)
        {
            return GetCurrentVersionBonuses(template.Info.Brand.Id)
                        .Where(bonus => bonus.Template.Id == template.Id && bonus.Template.Version == template.Version);
        }

        [Filtered]
        [Permission(Permissions.View, Module = Modules.BonusManager)]
        public IQueryable<Data.Bonus> GetCurrentVersionBonuses()
        {
            return _repository.GetCurrentVersionBonuses().AsNoTracking();
        }

        [Filtered]
        [Permission(Permissions.View, Module = Modules.BonusTemplateManager)]
        [Permission(Permissions.View, Module = Modules.BonusManager)]
        public IQueryable<Template> GetCurrentVersionTemplates()
        {
            var currentIdVersion = _repository.Templates
                .GroupBy(template => template.Id)
                .Select(group => new { Id = group.Key, Version = group.Max(obj => obj.Version) });

            return _repository.Templates
                                .Where(t => t.Status != TemplateStatus.Deleted)
                                .Where(template => currentIdVersion.Contains(new { template.Id, template.Version }))
                                .AsNoTracking();
        }

        [Filtered]
        public IQueryable<BonusRedemption> GetBonusRedemptions()
        {
            var redemptions = _repository.Players
                .SelectMany(p => p.Wallets)
                .SelectMany(w => w.BonusesRedeemed)
                .Include(br => br.Player)
                .Include(br => br.Player.Brand)
                .Include(br => br.Bonus);

            return redemptions.AsNoTracking();
        }
        public BonusRedemption GetBonusRedemption(Guid playerId, Guid redemptionId)
        {
            return _repository.GetBonusRedemption(playerId, redemptionId).Data;
        }

        List<string> GetQualificationFailures(Guid playerId, string bonusCode, RedemptionParams redemptionParams, Trigger expectedBonusTrigger)
        {
            var bonus = _repository.GetLockedBonusOrNull(bonusCode);

            var qualificationFailures = new List<string>();
            if (bonus == null || bonus.Data.Template.Info.BonusTrigger != expectedBonusTrigger || bonus.Data.Template.Info.Mode != IssuanceMode.AutomaticWithCode)
            {
                qualificationFailures.Add("Bonus code is not valid.");
            }
            else
            {
                var player = _repository.GetLockedPlayer(playerId);
                qualificationFailures = bonus.QualifyFor(player, QualificationPhase.Redemption, redemptionParams).ToList();
            }

            return qualificationFailures;
        }

        //IBonusQueries members
        public IEnumerable<OfflineDepositQualifiedBonus> GetOfflineDepositQualifiedBonuses(Guid playerId)
        {
            var player = _repository.GetLockedPlayer(playerId);
            var qualifiedBonuses = GetCurrentVersionBonuses(player.Data.Brand.Id)
                .Where(x => x.Template.Info.BonusTrigger == Trigger.Deposit && x.Template.Info.Mode == IssuanceMode.AutomaticWithCode)
                .ToList()
                .Select(x => new Entities.Bonus(x))
                .Where(bonus => bonus.QualifiesFor(player, QualificationPhase.PreRedemption, new RedemptionParams()))
                .Select(x => x.Data)
                .ToArray();

            foreach (var bonus in qualifiedBonuses)
            {
                var bonusTiers =
                    bonus.Template.Rules.RewardTiers
                    .Single(t => t.CurrencyCode == player.Data.CurrencyCode).Tiers;
                yield return new OfflineDepositQualifiedBonus
                {
                    Name = bonus.Name,
                    Code = bonus.Code,
                    MinDeposit = bonusTiers.Select(r => r.From).Min(),
                    MaxDeposit = bonusTiers.Select(r => r.To).Max() ?? decimal.MaxValue
                };
            }
        }
        public IEnumerable<ManualByCsQualifiedBonus> GetManualByCsQualifiedBonuses(Guid playerId)
        {
            var player = _repository.GetLockedPlayer(playerId);
            var now = DateTimeOffset.Now.ToBrandOffset(player.Data.Brand.TimezoneId);
            return GetCurrentVersionBonuses(player.Data.Brand.Id)
                .ToList()
                .Select(x => new Entities.Bonus(x))
                .Where(bonus => bonus.QualifiesFor(player, QualificationPhase.PreRedemption, new RedemptionParams { IsIssuedByCs = true }))
                .Select(b => b.Data)
                .Select(b => new ManualByCsQualifiedBonus
                {
                    Id = b.Id,
                    Name = b.Name,
                    Code = b.Code,
                    Description = b.Description,
                    Type = GetTemplateType(b.Template.Info),
                    Status = (now.Date >= b.ActiveTo.Date ? BonusQualificationStatus.Expired : BonusQualificationStatus.Active).ToString()
                });
        }

        public List<QualifiedTransaction> GetManualByCsQualifiedTransactions(Guid playerId, Guid bonusId)
        {
            var player = _repository.GetLockedPlayer(playerId);
            var bonus = _repository.GetLockedBonus(bonusId);

            return bonus.GetQualifiedTransactions(player).ToList();
        }

        public List<string> GetDepositQualificationFailures(Guid playerId, string bonusCode, decimal depositAmount)
        {
            return GetQualificationFailures(playerId, bonusCode, new RedemptionParams { TransferAmount = depositAmount }, Trigger.Deposit);
        }
        public List<string> GetFundInQualificationFailures(Guid playerId, string bonusCode, decimal fundInAmount, Guid walletTemplateId)
        {
            return GetQualificationFailures(playerId, bonusCode, new RedemptionParams { TransferAmount = fundInAmount, TransferWalletTemplateId = walletTemplateId }, Trigger.FundIn);
        }
        public string GetBonusName(string bonusCode)
        {
            var bonus = _repository.GetLockedBonus(bonusCode);

            return bonus.Data.Name;
        }
        public ClaimableBonusRedemption[] GetClaimableRedemptions(Guid playerId)
        {
            var player = _repository.GetLockedPlayer(playerId);

            return player.GetClaimableRedemptions();
        }

        public static string GetTemplateType(TemplateInfo info)
        {
            switch (info.BonusTrigger)
            {
                case Trigger.Deposit:
                    if (info.DepositKind == DepositKind.First)
                        return BonusType.FirstDeposit.ToString();
                    if (info.DepositKind == DepositKind.High)
                        return BonusType.HighDeposit.ToString();
                    return BonusType.ReloadDeposit.ToString();
                case Trigger.MobilePlusEmailVerification:
                    return BonusType.MobilePlusEmailVerification.ToString();
                case Trigger.ReferFriend:
                    return BonusType.ReferFriend.ToString();
                case Trigger.FundIn:
                    return BonusType.FundIn.ToString();
                default:
                    throw new RegoException(string.Format("Unknown bonus trigger: {0}", info.BonusTrigger));
            }
        }


        //Validation calls
        public ValidationResult GetValidationResult(IssueBonusByCsVM model)
        {
            return new IssueByCsValidator(this, _repository, _walletQueries).Validate(model);
        }
        public ValidationResult GetValidationResult(ToggleBonusStatusVM model)
        {
            return new ToggleBonusStatusValidator(this).Validate(model);
        }
        public ValidationResult GetValidationResult(Guid templateId)
        {
            return new TemplateDeletionValidator(this).Validate(templateId);
        }
    }
}