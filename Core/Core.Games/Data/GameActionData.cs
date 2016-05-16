using System;

namespace AFT.RegoV2.Core.Game.Data
{
    public class GameActionData
    {
        public Guid TokenId { get; set; }
        public string RoundId { get; set; }
        public string ExternalTransactionId { get; set; }
        public string CurrencyCode { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public string TransactionReferenceId { get; set; }
        public string BatchId { get; set; }

        public static GameActionData NewGameActionData(string roundId, 
            decimal amount, 
            string currencyCode,
            Guid tokenId,
            string externalTransactionId = null,
            string description = null,
            string transactionReferenceId = null)
        {
            return new GameActionData
            {
                TokenId = tokenId,
                RoundId = roundId,
                Amount = amount,
                CurrencyCode = currencyCode,
                ExternalTransactionId = externalTransactionId ?? Guid.NewGuid().ToString(),
                TransactionReferenceId = transactionReferenceId,
                Description = description ?? String.Empty
            };
        }
    }
}
