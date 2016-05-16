using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Core.Bonus.Data;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Shared;

namespace AFT.RegoV2.Core.Bonus.Entities
{
    public class BonusRedemption
    {
        internal readonly Data.BonusRedemption Data;

        internal decimal RolloverLeft
        {
            get { return Data.Rollover - Data.Contributions.Sum(c => c.Contribution); }
        }

        internal Wallet Wallet
        {
            get { return Data.Player.Wallets.Single(w => w.TemplateId == Data.Bonus.Template.Info.WalletTemplateId); }
        }

        public BonusRedemption(Data.BonusRedemption data)
        {
            Data = data;
        }

        public bool IsActivationQualified()
        {
            return IsQualified(QualificationPhase.Activation);
        }

        public bool IsClaimQualified()
        {
            return IsQualified(QualificationPhase.Claim);
        }

        bool IsQualified(QualificationPhase phase)
        {
            var bonus = new Bonus(Data.Bonus);
            var player = new Player(Data.Player);

            return bonus.QualifiesFor(player, phase, Data.Parameters);
        }

        public void MakeClaimable()
        {
            var bonus = new Bonus(Data.Bonus);
            var player = new Player(Data.Player);

            bonus.Data.Statistic.TotalRedeemedAmount -= Data.Amount;
            Data.Amount = bonus.CalculateReward(player, Data.Parameters);
            bonus.Data.Statistic.TotalRedeemedAmount += Data.Amount;

            Data.ActivationState = ActivationStatus.Claimable;
            Data.UpdatedOn = SystemTime.Now.ToBrandOffset(Data.Bonus.Template.Info.Brand.TimezoneId);
        }

        public IssuanceParams Activate()
        {
            Data.ActivationState = ActivationStatus.Activated;
            Data.UpdatedOn = SystemTime.Now.ToBrandOffset(Data.Bonus.Template.Info.Brand.TimezoneId);

            if (Data.Bonus.Template.Info.IsWithdrawable == false)
            {
                Wallet.NotWithdrawableBalance += Data.Amount;
            }

            return GetIssuanceParams();
        }

        /// <summary>
        /// Calculates and sets the wagering requirement amount that player has to fulfill
        /// according to bonus's terms and conditions
        /// </summary>
        public void CalculateRolloverAmount()
        {
            decimal wageringMethodAmount;
            var wageringMethod = Data.Bonus.Template.Wagering.Method;
            switch (wageringMethod)
            {
                case WageringMethod.Bonus:
                    wageringMethodAmount = Data.Amount;
                    break;
                case WageringMethod.TransferAmount:
                    wageringMethodAmount = Data.Parameters.TransferAmount;
                    break;
                case WageringMethod.BonusAndTransferAmount:
                    wageringMethodAmount = Data.Amount + Data.Parameters.TransferAmount;
                    break;
                default:
                    throw new RegoException(string.Format("Wagering method is not valid: {0}", wageringMethod));
            }

            Data.Rollover = wageringMethodAmount * Data.Bonus.Template.Wagering.Multiplier;
        }

        public List<LockUnlockParams> GetLockUnlockParams()
        {
            var computedLocks = new List<LockUnlockParams>();

            //Data.Parameters.TransferAmount is 0 for bonus types except (First|Reload) deposit and Fund-in
            if (Data.Parameters.TransferAmount > 0)
            {
                computedLocks.Add(new LockUnlockParams
                {
                    Amount = Data.Parameters.TransferAmount,
                    WalletTemplateId = Data.Parameters.TransferWalletTemplateId,
                    Type = LockType.Bonus
                });
            }
            //Nothing to lock for IsAfterWager = true bonuses, as no bonus fund were issued at this point
            if (Data.Bonus.Template.Wagering.IsAfterWager == false)
            {
                computedLocks.Add(new LockUnlockParams
                {
                    Amount = Data.Amount,
                    WalletTemplateId = Data.Bonus.Template.Info.WalletTemplateId,
                    Type = LockType.Bonus
                });
            }

            return computedLocks;
        }

        public List<LockUnlockParams> ActivateRollover()
        {
            Data.RolloverState = RolloverStatus.Active;

            var computedLocks = GetLockUnlockParams();
            Data.LockedAmount = computedLocks.Sum(cl => cl.Amount);

            return computedLocks;
        }

        public void CompleteRollover()
        {
            Data.RolloverState = RolloverStatus.Completed;
        }

        public void ZeroOutRollover(BonusTransaction transaction)
        {
            Data.Contributions.Add(new RolloverContribution
            {
                Transaction = transaction,
                Contribution = RolloverLeft,
                Type = ContributionType.Threshold
            });
            Data.RolloverState = RolloverStatus.ZeroedOut;
        }

        public void Negate()
        {
            Data.ActivationState = ActivationStatus.Negated;

            RevertRedemptionImplactOnStatistics();
            Data.UpdatedOn = SystemTime.Now.ToBrandOffset(Data.Bonus.Template.Info.Brand.TimezoneId);
        }

        public void Cancel()
        {
            Data.Contributions.Add(new RolloverContribution
            {
                Contribution = RolloverLeft,
                Type = ContributionType.Cancellation
            });
            Data.ActivationState = ActivationStatus.Canceled;
            Data.RolloverState = RolloverStatus.None;

            RevertRedemptionImplactOnStatistics();
            Data.UpdatedOn = SystemTime.Now.ToBrandOffset(Data.Bonus.Template.Info.Brand.TimezoneId);
        }

        /// <summary>
        /// Returns multiplier using which game contributes to rollover
        /// </summary>
        public decimal GetGameToWageringContributionMultiplier(Guid gameId)
        {
            var contribution = Data.Bonus.Template.Wagering.GameContributions.SingleOrDefault(c => c.GameId == gameId);
            return contribution != null ? contribution.Contribution : 1m;
        }

        public bool WageringThresholdIsMet(decimal playableBalance)
        {
            return Data.Bonus.Template.Wagering.Threshold >= playableBalance;
        }

        public string GetUnlockDescription()
        {
            switch (Data.RolloverState)
            {
                case RolloverStatus.Completed:
                    return "Wagering requirement is Completed.";
                case RolloverStatus.None:
                    return "Wagering requirement is Canceled (bonus canceled).";
                case RolloverStatus.ZeroedOut:
                    return "Wagering requirement is Zeroed out.";
                default:
                    throw new RegoException(string.Format("Not supported rollover state: {0}", Data.RolloverState));
            }
        }

        IssuanceParams GetIssuanceParams()
        {
            var template = Data.Bonus.Template;
            var balanceTarget = template.Info.IsWithdrawable && template.Wagering.HasWagering == false
                ? BalanceTarget.Main
                : BalanceTarget.Bonus;

            return new IssuanceParams
            {
                Amount = Data.Amount,
                Target = balanceTarget,
                WalletTemplateId = template.Info.WalletTemplateId
            };
        }

        void RevertRedemptionImplactOnStatistics()
        {
            Data.Bonus.Statistic.TotalRedeemedAmount -= Data.Amount;
            Data.Bonus.Statistic.TotalRedemptionCount--;
        }
    }
}