using System;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Payment.Events
{
    public class PlayerAccountRestrictionsChanged :DomainEventBase
    {
        public bool? ExemptWithdrawalVerification { get; set; }
        public DateTimeOffset? ExemptWithdrawalFrom { get; set; }
        public DateTimeOffset? ExemptWithdrawalTo { get; set; }
        public int? ExemptLimit { get; set; }

        public PlayerAccountRestrictionsChanged()
        {
            
        }

        public PlayerAccountRestrictionsChanged(int? exemptLimit, DateTimeOffset? exemptWithdrawalTo, DateTimeOffset? exemptWithdrawalFrom, bool? exemptWithdrawalVerification)
        {
            ExemptLimit = exemptLimit;
            ExemptWithdrawalTo = exemptWithdrawalTo;
            ExemptWithdrawalFrom = exemptWithdrawalFrom;
            ExemptWithdrawalVerification = exemptWithdrawalVerification;
        }
    }
}
