using System;

namespace AFT.RegoV2.Core.Common.Data
{
    
    public class BetTransaction
    {
        /// <summary>
        /// Record id
        /// </summary>
        public Guid Id { get; set; }    

        /// <summary>
        /// Parent Bet
        /// </summary>
        public Bet Bet { get; set; }

        /// <summary>
        /// Game server's transaction ID. Used sometimes to repeat the same call. We should rely on that, if it's not empty.
        /// </summary>
        public string ExternalTransactionId { get; set; }

        /// <summary>
        /// Used for Cancel and Adjustement transactions
        /// </summary>
        public string ExternalTransactionReferenceId { get; set; } 

        /// <summary>
        /// This ID identifies the wallet transaction associated with this bet transaction.
        /// </summary>
        public Guid WalletTransactionId { get; set; } 
        
        public string ExternalBatchId { get; set; }

        public decimal Amount { get; set; }

        public string Description { get; set; }

        public BetTransactionType TransactionType { get; set; }

        public DateTimeOffset CreatedOn { get; set; }

        /// <summary>
        /// Token ID
        /// </summary>
        public Guid TokenId { get; set; } 
    }

    public enum BetTransactionType
    {
        Placed,
        Won, 
        Adjustment,
        Cancel,
        Lost,
        Free    
    }
}
