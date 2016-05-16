using System;
using AFT.RegoV2.Shared;
using ServiceStack.ServiceHost;

namespace AFT.RegoV2.MemberApi.Interfaces.Bonus
{

    [Route("/api/bonus/QualifyFundInBonus")]
    public class QualifyFundInBonus : RequestBase, IReturn<QualifyFundInBonusResponse>
    {
        public Guid WalletId { get; set; }
        public decimal Amount { get; set; }
        public string BonusCode { get; set; }
    }

    public class QualifyFundInBonusResponse : ResponseBase
    {
    }
}
