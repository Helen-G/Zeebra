using System;
using System.Collections.Generic;
using System.Linq;

namespace AFT.RegoV2.Core.Common.Data
{
    public class Bet
    {
        /// <summary>
        /// Record ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// External Bet ID. Identifies all bets within one hand. 
        /// </summary>
        public string ExternalBetId { get; set; } 

        public Guid PlayerId { get; set; }
        public GameEndpoint Game { get; set; }
        public Guid GameId { get; set; }
        public Guid BrandId { get; set; }

        
        public BetStatus Status { get; set; }

        public DateTimeOffset CreatedOn { get; set; }
        public DateTimeOffset? ClosedOn { get; set; }

        public List<BetTransaction> BetTransactions { get; set; } 

        public Bet()
        {
            ClosedOn = null;
        }

        public decimal Amount
        {
            get { return -BetTransactions.Where(x => x.TransactionType == BetTransactionType.Placed).Sum(x => x.Amount); }
        }

        public decimal WonAmount
        {
            get
            {
                return BetTransactions.Where(x =>
                    x.TransactionType == BetTransactionType.Won || x.TransactionType == BetTransactionType.Free
                    ).Sum(x => x.Amount);
            }
        }

    
        public decimal AdjustedAmount
        {
            get
            {
                return BetTransactions.Where(x =>
                    x.TransactionType == BetTransactionType.Adjustment || x.TransactionType == BetTransactionType.Cancel)
                    .Sum(x => x.Amount);
            }
        }
    }

    public enum BetStatus
    {
        New, Open, Closed
    }
}
