using System;

namespace AFT.RegoV2.Core.Bonus.Data
{
    public class RedemptionParams
    {
        /// <summary>
        /// Deposit/fund-in transaction identifier
        /// </summary>
        public Guid? TransferExternalId { get; set; }
        public Guid? TransferWalletTemplateId { get; set; }
        /// <summary>
        /// Deposit/fund-in amount
        /// </summary>
        public decimal TransferAmount { get; set; }
        public bool IsIssuedByCs { get; set; }
    }
}