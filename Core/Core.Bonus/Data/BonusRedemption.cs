using System;
using System.Collections.Generic;

namespace AFT.RegoV2.Core.Bonus.Data
{
    public class BonusRedemption: Identity
    {
        public BonusRedemption()
        {
            Contributions = new List<RolloverContribution>();
            Parameters = new RedemptionParams();
        }

        public virtual Player Player { get; set; }
        public virtual Bonus Bonus { get; set; }
        public ActivationStatus ActivationState { get; set; }
        public RolloverStatus RolloverState { get; set; }
        public decimal Amount { get; set; }
        /// <summary>
        /// Total amount locked by rollover
        /// </summary>
        public decimal LockedAmount { get; set; }
        /// <summary>
        /// Is synonym of "Wagering requirement" term
        /// </summary>
        public decimal Rollover { get; set; }
        public DateTimeOffset CreatedOn { get; set; }
        public DateTimeOffset? UpdatedOn { get; set; }
        public RedemptionParams Parameters { get; set; }

        public virtual List<RolloverContribution> Contributions { get; set; }
    }

    public enum ActivationStatus
    {
        /// <summary>
        /// Bonus is redeemed, but player yet has to perform activation actions (finish deposit process etc.)
        /// </summary>
        Pending,
        /// <summary>
        /// All activation actions are performed, but player has to manually claim the reward
        /// </summary>
        Claimable,
        /// <summary>
        /// Bonus reward is credited to player. Rollover is applied
        /// </summary>
        Activated,
        /// <summary>
        /// Player became not qualified for the bonus between Pending and Activated states
        /// </summary>
        Negated,
        /// <summary>
        /// Player canceled bonus
        /// </summary>
        Canceled
    }

    public enum RolloverStatus
    {
        /// <summary>
        /// Rollover is not applicable (bonus redemption is not in activated state)
        /// </summary>
        None,
        /// <summary>
        /// Rollover is in progress
        /// </summary>
        Active,
        /// <summary>
        /// Rollover is completed or activated bonus with no rollover specified
        /// </summary>
        Completed,
        /// <summary>
        /// Rollover is zeroed out due to wagering threshold
        /// </summary>
        ZeroedOut
    }
}