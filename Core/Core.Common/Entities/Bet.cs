using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Exceptions;
using AFT.RegoV2.Shared;

namespace AFT.RegoV2.Core.Common.Entities
{
    public class Bet
    {
        public Data.Bet Data { get; private set; }

        public Bet()
        {
        }

        public Bet(Data.Bet data)
        {
            Data = data;
        }

        public Bet(string externalBetId, TokenData tokenData)
        {
            Data = new Data.Bet
            {
                Id = Guid.NewGuid(),
                CreatedOn = DateTimeOffset.Now,
                Status = BetStatus.New,
                ExternalBetId = externalBetId,
                PlayerId = tokenData.PlayerId,
                GameId = tokenData.GameId,
                BrandId = tokenData.BrandId,
                BetTransactions = new List<BetTransaction>()
            };
        }

        public Guid Place(decimal amount, string description, Guid walletTransactionId, Guid tokenId, string externalTransactionId = null)
        {
            if (amount <= 0)
            {
                throw new ArgumentOutOfRangeException("amount", "Amount must be a positive number.");
            }

            var betTxId = CreateBetTransaction(amount, BetTransactionType.Placed, description, walletTransactionId, externalTransactionId, tokenId);

            Data.Status = BetStatus.Open;

            return betTxId;
        }


        public Guid Win(decimal amount, string description, Guid walletTransactionId, Guid tokenId, string externalTransactionId = null, string batchId = null)
        {
            if (Data.Status == BetStatus.New)
            {
                throw new RegoException("Cannot win an unopened bet.");
            }
            if (amount <= 0)
            {
                throw new ArgumentOutOfRangeException("amount", "Amount must be a positive number.");
            }
            var betTxId = CreateBetTransaction(amount, BetTransactionType.Won, description, walletTransactionId, externalTransactionId, tokenId, batchId:batchId);

            CloseBet();

            return betTxId;
        }

        public Guid Free(decimal amount, string description, Guid walletTransactionId, Guid tokenId, string externalTransactionId = null, string batchId = null)
        {
            if (amount <= 0)
            {
                throw new ArgumentOutOfRangeException("amount", "Amount must be a positive number.");
            }

            var betTxId = CreateBetTransaction(amount, BetTransactionType.Free, description, walletTransactionId, externalTransactionId, tokenId);

            CloseBet();

            return betTxId;
        }

        private void CloseBet()
        {
            Data.Status = BetStatus.Closed;
            Data.ClosedOn = DateTimeOffset.Now;
        }


        public Guid Adjust(decimal amount, string description, Guid walletTransactionId, Guid tokenId, string externalTransactionId = null, string transactionReferenceId = null, string batchId = null)
        {
            if (Data.Status == BetStatus.New)
            {
                throw new RegoException("Cannot adjust an unopened bet.");
            }

            if (String.IsNullOrWhiteSpace(transactionReferenceId) == false)
            {
                GetBetTransaction(transactionReferenceId);
            }


            return CreateBetTransaction(amount, BetTransactionType.Adjustment, description, walletTransactionId, externalTransactionId, tokenId, transactionReferenceId, batchId);
        }

        public Guid Lose(string description, Guid tokenId, string externalTransactionId = null, string batchId = null)
        {
            CloseBet();

            return CreateBetTransaction(0, BetTransactionType.Lost, description, Guid.Empty, externalTransactionId, tokenId, batchId);
        }

        public Guid Cancel(decimal amount, string description, Guid walletTransactionId, string externalTransactionId, string transactionReferenceId, Guid tokenId, string batchId = null)
        {
            if (String.IsNullOrWhiteSpace(transactionReferenceId) == false)
            {
                GetBetTransaction(transactionReferenceId);
            }
            
            return CreateBetTransaction(amount, BetTransactionType.Cancel, description, walletTransactionId, externalTransactionId, tokenId, transactionReferenceId, batchId);
        }

        public BetTransaction GetBetTransaction(string transactionReferenceId)
        {
            var transaction = Data.BetTransactions.SingleOrDefault(x => x.ExternalTransactionId == transactionReferenceId);
            if (transaction == null)
                throw new BetTransactionNotFoundException();

            return transaction;
        }

        private Guid CreateBetTransaction(decimal amount, BetTransactionType betEventType, string description, Guid walletTransactionId, string externalTransactionId,
            Guid tokenId,
            string transactionReferenceId = null,
            string batchId = null)
        {
            if (externalTransactionId == null)
            {
                throw new ArgumentNullException("externalTransactionId");
            }
            var betTransactionId = Guid.NewGuid();

            if (betEventType == BetTransactionType.Placed)
            {
                amount = -amount;
            }

            Data.BetTransactions.Add(new BetTransaction
            {
                Id = betTransactionId,
                ExternalTransactionId = externalTransactionId,
                ExternalTransactionReferenceId = transactionReferenceId,
                CreatedOn = DateTimeOffset.Now,
                Bet = Data,
                TransactionType = betEventType,
                Amount = amount,
                Description = description,
                WalletTransactionId = walletTransactionId,
                ExternalBatchId = batchId,
                TokenId = tokenId
            });

            return betTransactionId;
        }
    }
}