using System;

namespace AFT.RegoV2.Core.Game.Data
{
    public class GameAction
    {
        /// <summary>
        /// Record id
        /// </summary>
        public Guid Id { get; set; }    

        /// <summary>
        /// Parent Bet
        /// </summary>
        public Round Round { get; set; }

        /// <summary>
        /// Game provider's transaction ID. Used sometimes to repeat the same call. We should rely on that, if it's not empty.
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

        public GameActionType GameActionType { get; set; }

        public DateTimeOffset CreatedOn { get; set; }

        /// <summary>
        /// Token ID
        /// </summary>
        public Guid TokenId { get; set; } 
    }

    public enum GameActionType
    {
        Placed,
        Won, 
        Adjustment,
        Cancel,
        Lost,
        Free    
    }
}
