using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Core.Bonus.Data;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Wallet;
using AFT.RegoV2.Core.Common.Utils;

namespace AFT.RegoV2.Core.Bonus.Entities
{
    public class Player
    {
        private readonly Data.Player _data;
        public Data.Player Data { get { return _data; } }

        internal bool ContactsVerified { get { return _data.IsEmailVerified && _data.IsMobileVerified; } }
        public List<Data.BonusRedemption> BonusesRedeemed { get { return _data.Wallets.SelectMany(w => w.BonusesRedeemed).ToList(); } }

        public decimal MonthlyAccumulatedDepositAmount
        {
            get
            {
                var now = SystemTime.Now.ToBrandOffset(Data.Brand.TimezoneId);
                return Data.Wallets
                .SelectMany(w => w.Transactions)
                .Where(t => t.Type == TransactionType.Deposit)
                .Where(t => t.CreatedOn.Month == now.Month && t.CreatedOn.Year == now.Year)
                .Sum(t => t.TotalAmount);
            }
        }

        public Player(Data.Player data)
        {
            _data = data;
        }

        public BonusRedemption Redeem(Bonus bonus, RedemptionParams redemptionParams)
        {
            var bonusReward = bonus.CalculateReward(this, redemptionParams);

            var bonusRedemption = new Data.BonusRedemption
            {
                Amount = bonusReward,
                Player = _data,
                Bonus = bonus.Data,
                CreatedOn = SystemTime.Now.ToBrandOffset(bonus.Data.Template.Info.Brand.TimezoneId)
            };
            if (redemptionParams != null)
            {
                bonusRedemption.Parameters = redemptionParams;
            }

            _data
                .Wallets
                .Single(w => w.TemplateId == bonus.Data.Template.Info.WalletTemplateId)
                .BonusesRedeemed.Add(bonusRedemption);

            bonus.Data.Statistic.TotalRedeemedAmount += bonusReward;
            bonus.Data.Statistic.TotalRedemptionCount++;

            return new BonusRedemption(bonusRedemption);
        }

        public ClaimableBonusRedemption[] GetClaimableRedemptions()
        {
            return BonusesRedeemed
                .Where(r => r.ActivationState == ActivationStatus.Claimable)
                .OrderBy(r => r.CreatedOn)
                .Select(br => new ClaimableBonusRedemption
                {
                    Id = br.Id,
                    BonusName = br.Bonus.Name,
                    RewardAmount = br.Amount,
                    ClaimableFrom = br.Bonus.DurationStart,
                    ClaimableTo = br.Bonus.ActiveTo.AddDays(br.Bonus.DaysToClaim)
                })
                .ToArray();
        }

        public void VerifyMobileNumber()
        {
            Data.IsMobileVerified = true;
        }

        public void VerifyEmailAddress()
        {
            Data.IsEmailVerified = true;
        }

        public bool CompletedReferralRequirements(decimal betAmount)
        {
            var referFriendBonus = Data.ReferredWith;

            if (referFriendBonus != null)
            {
                var firstDepositAmount =
                    Data.Wallets.SelectMany(w => w.Transactions)
                        .Where(t => t.Type == TransactionType.Deposit)
                        .OrderBy(t => t.CreatedOn)
                        .First()
                        .TotalAmount;
                var requiredRollover = firstDepositAmount * referFriendBonus.Template.Rules.ReferFriendWageringCondition;
                var actualRollover = Data.AccumulatedWageringAmount;
                return firstDepositAmount >= referFriendBonus.Template.Rules.ReferFriendMinDepositAmount &&
                       actualRollover >= requiredRollover &&
                       actualRollover - betAmount < requiredRollover;
            }
            return false;
        }

        public List<BonusRedemption> GetRedemptionsWithActiveRollover(Guid walletStructureId)
        {
            return _data
                .Wallets
                .Single(w => w.TemplateId == walletStructureId)
                .BonusesRedeemed
                            .Where(redemption => redemption.RolloverState == RolloverStatus.Active)
                            .OrderBy(redemption => redemption.CreatedOn)
                            .ToList()
                            //placing an redemption with contributions on top of the list
                            .OrderBy(redemption => redemption.Contributions.Any() == false)
                            .Select(br => new BonusRedemption(br))
                            .ToList();
        }

        public decimal GetWageringToDistribute(Guid walletStructureId, Guid roundId)
        {
            var betPlacedTransactions = _data.Wallets
                .Single(w => w.TemplateId == walletStructureId)
                .Transactions
                .Where(tr => tr.RoundId.HasValue)
                .Where(tr => tr.Type == TransactionType.BetPlaced && tr.RoundId == roundId)
                .ToList();

            var betPlacedTotal = betPlacedTransactions.Sum(tr => tr.TotalAmount);
            var betPlacedCount = betPlacedTransactions.Count;
            return Math.Round(betPlacedTotal / betPlacedCount, 6);
        }
    }
}