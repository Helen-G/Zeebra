using System;

namespace AFT.RegoV2.Core.Bonus.Data.ViewModels
{
    public class IssueBonusByCsVM
    {
        public Guid PlayerId { get; set; }
        public Guid BonusId { get; set; }
        public Guid TransactionId { get; set; }
    }
}
