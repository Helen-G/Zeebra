using System;

namespace AFT.RegoV2.Core.Common.Data.Bonus
{
    public class ManualByCsQualifiedBonus
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
    }

    public class QualifiedTransaction
    {
        public Guid Id { get; set; }
        public string CurrencyCode { get; set; }
        public decimal Amount { get; set; }
        public DateTimeOffset Date { get; set; }
        public decimal BonusAmount { get; set; }
    }

    public enum BonusQualificationStatus
    {
        Active,
        Expired
    }
}
