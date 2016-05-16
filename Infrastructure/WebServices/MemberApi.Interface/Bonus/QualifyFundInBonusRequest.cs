using System;
using System.Collections.Generic;

namespace AFT.RegoV2.MemberApi.Interface.Bonus
{
    public class QualifyFundInBonusRequest 
    {
        public Guid WalletId { get; set; }
        public decimal Amount { get; set; }
        public string BonusCode { get; set; }
    }

    public class QualifyFundInBonusResponse
    {
        public List<string> Errors { get; set; }
    }
}
