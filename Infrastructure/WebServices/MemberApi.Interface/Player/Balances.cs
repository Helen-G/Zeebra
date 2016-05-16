using System;
using AFT.RegoV2.Shared;

namespace AFT.RegoV2.MemberApi.Interface.Player
{
    public class BalancesRequest 
    {
        public Guid? WalletId { get; set; }
    }

    public class BalancesResponse 
    {
        public decimal Main { get; set; }
        public decimal Bonus { get; set; }
        public decimal Free { get; set; }
        public decimal Playable { get; set; }
    }
}
