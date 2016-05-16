using AFT.RegoV2.Shared;
using ServiceStack.ServiceHost;

namespace AFT.RegoV2.MemberApi.Interfaces.Bonus
{
    [Route("/api/bonus/qualifyDepositBonus")]
    public class QualifyDepositBonus : RequestBase, IReturn<QualifyDepositBonusResponse>
    {
        public decimal Amount { get; set; }
        public string BonusCode { get; set; }
    }

    public class QualifyDepositBonusResponse : ResponseBase
    {
    }
}
