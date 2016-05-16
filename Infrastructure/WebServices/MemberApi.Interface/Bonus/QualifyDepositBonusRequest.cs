using System.Collections.Generic;

namespace AFT.RegoV2.MemberApi.Interface.Bonus
{
    public class QualifyDepositBonusRequest 
    {
        public decimal Amount { get; set; }
        public string BonusCode { get; set; }
    }

    public class QualifyDepositBonusResponse
    {
        public List<string> Errors { get; set; }
    }
}
