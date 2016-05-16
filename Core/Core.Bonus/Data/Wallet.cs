using System;
using System.Collections.Generic;

namespace AFT.RegoV2.Core.Bonus.Data
{
    public class Wallet: Identity
    {
        public Wallet()
        {
            BonusesRedeemed = new List<BonusRedemption>();
            Transactions = new List<BonusTransaction>();
        }

        public Guid TemplateId { get; set; }
        public decimal NotWithdrawableBalance { get; set; }
        public virtual List<BonusRedemption> BonusesRedeemed { get; set; }
        public virtual List<BonusTransaction> Transactions { get; set; }
    }
}