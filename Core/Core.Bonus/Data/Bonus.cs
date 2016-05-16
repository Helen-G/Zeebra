using System;

namespace AFT.RegoV2.Core.Bonus.Data
{
    public class Bonus : Identity
    {
        /// <summary>
        /// We need version for making sure that after bonus update, already redeemed/activated bonuses are not affected
        /// </summary>
        public int Version { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public DateTimeOffset ActiveFrom { get; set; }
        public DateTimeOffset ActiveTo { get; set; }
        public string Description { get; set; }
        public DurationType DurationType { get; set; }
        public DateTimeOffset DurationStart { get; set; }
        public DateTimeOffset DurationEnd { get; set; }
        /// <summary>
        /// Specifies number of days after ActiveTo date player can redeem the bonus
        /// </summary>
        public int DaysToClaim { get; set; }

        public bool IsActive { get; set; }
        public Guid StatisticId { get; set; }
        public virtual BonusStatistic Statistic { get; set; }
        public virtual Template Template { get; set; }

        public string CreatedBy { get; set; }
        public DateTimeOffset CreatedOn { get; set; }
        public string UpdatedBy { get; set; }
        public DateTimeOffset? UpdatedOn { get; set; }
    }

    public enum DurationType
    {
        /// <summary>
        /// Matches bonus activity date range
        /// </summary>
        None,
        /// <summary>
        /// Duration calculated based on bonus ActiveFrom date
        /// </summary>
        StartDateBased,
        /// <summary>
        /// Date time range inside bonus activity date range
        /// </summary>
        Custom
    }
}