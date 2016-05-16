using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Exceptions;
using AFT.RegoV2.Core.Game.Data;
using AFT.RegoV2.Shared;

namespace AFT.RegoV2.Core.Game.Entities
{
    public class Round
    {
        public Data.Round Data { get; private set; }

        public decimal Amount
        {
            get { return -Data.GameActions.Where(x => x.GameActionType == GameActionType.Placed).Sum(x => x.Amount); }
        }

        public decimal WonAmount
        {
            get
            {
                return Data.GameActions.Where(x =>
                    x.GameActionType == GameActionType.Won || x.GameActionType == GameActionType.Free
                    ).Sum(x => x.Amount);
            }
        }


        public decimal AdjustedAmount
        {
            get
            {
                return Data.GameActions.Where(x =>
                    x.GameActionType == GameActionType.Adjustment || x.GameActionType == GameActionType.Cancel)
                    .Sum(x => x.Amount);
            }
        }

        public Round()
        {
        }

        public Round(Data.Round data)
        {
            Data = data;
        }

        public Round(string externalBetId, TokenData tokenData)
        {
            Data = new Data.Round
            {
                Id = Guid.NewGuid(),
                CreatedOn = DateTimeOffset.Now,
                Status = RoundStatus.New,
                ExternalRoundId = externalBetId,
                PlayerId = tokenData.PlayerId,
                GameId = tokenData.GameId,
                BrandId = tokenData.BrandId,
                GameActions = new List<GameAction>()
            };
        }

        public Guid Place(decimal amount, string description, Guid walletTransactionId, Guid tokenId, string externalTransactionId = null)
        {
            if (amount <= 0)
            {
                throw new ArgumentOutOfRangeException("amount", "Amount must be a positive number.");
            }

            var betTxId = CreateGameAction(amount, GameActionType.Placed, description, walletTransactionId, externalTransactionId, tokenId);

            Data.Status = RoundStatus.Open;

            return betTxId;
        }


        public Guid Win(decimal amount, string description, Guid walletTransactionId, Guid tokenId, string externalTransactionId = null, string batchId = null)
        {
            if (Data.Status == RoundStatus.New)
            {
                throw new RegoException("Cannot win an unopened bet.");
            }
            if (amount <= 0)
            {
                throw new ArgumentOutOfRangeException("amount", "Amount must be a positive number.");
            }
            var betTxId = CreateGameAction(amount, GameActionType.Won, description, walletTransactionId, externalTransactionId, tokenId, batchId:batchId);

            CloseBet();

            return betTxId;
        }

        public Guid Free(decimal amount, string description, Guid walletTransactionId, Guid tokenId, string externalTransactionId = null, string batchId = null)
        {
            if (amount <= 0)
            {
                throw new ArgumentOutOfRangeException("amount", "Amount must be a positive number.");
            }

            var betTxId = CreateGameAction(amount, GameActionType.Free, description, walletTransactionId, externalTransactionId, tokenId);

            CloseBet();

            return betTxId;
        }

        private void CloseBet()
        {
            Data.Status = RoundStatus.Closed;
            Data.ClosedOn = DateTimeOffset.Now;
        }


        public Guid Adjust(decimal amount, string description, Guid walletTransactionId, Guid tokenId, string externalTransactionId = null, string transactionReferenceId = null, string batchId = null)
        {
            if (Data.Status == RoundStatus.New)
            {
                throw new RegoException("Cannot adjust an unopened bet.");
            }

            if (String.IsNullOrWhiteSpace(transactionReferenceId) == false)
            {
                GetGameActionByReferenceId(transactionReferenceId);
            }


            return CreateGameAction(amount, GameActionType.Adjustment, description, walletTransactionId, externalTransactionId, tokenId, transactionReferenceId, batchId);
        }

        public Guid Lose(string description, Guid tokenId, string externalTransactionId = null, string batchId = null)
        {
            CloseBet();

            return CreateGameAction(0, GameActionType.Lost, description, Guid.Empty, externalTransactionId, tokenId, batchId);
        }

        public Guid Cancel(decimal amount, string description, Guid walletTransactionId, string externalTransactionId, string transactionReferenceId, Guid tokenId, string batchId = null)
        {
            if (String.IsNullOrWhiteSpace(transactionReferenceId) == false)
            {
                GetGameActionByReferenceId(transactionReferenceId);
            }
            
            return CreateGameAction(amount, GameActionType.Cancel, description, walletTransactionId, externalTransactionId, tokenId, transactionReferenceId, batchId);
        }

        public GameAction GetGameActionByReferenceId(string transactionReferenceId)
        {
            var gameAction = Data.GameActions.SingleOrDefault(x => x.ExternalTransactionId == transactionReferenceId);
            if (gameAction == null)
                throw new GameActionNotFoundException();

            return gameAction;
        }

        private Guid CreateGameAction(decimal amount, GameActionType betEventType, string description, Guid walletTransactionId, string externalTransactionId,
            Guid tokenId,
            string transactionReferenceId = null,
            string batchId = null)
        {
            if (externalTransactionId == null)
            {
                throw new ArgumentNullException("externalTransactionId");
            }
            var gameActionId = Guid.NewGuid();

            if (betEventType == GameActionType.Placed)
            {
                amount = -amount;
            }

            Data.GameActions.Add(new GameAction
            {
                Id = gameActionId,
                ExternalTransactionId = externalTransactionId,
                ExternalTransactionReferenceId = transactionReferenceId,
                CreatedOn = DateTimeOffset.Now,
                Round = Data,
                GameActionType = betEventType,
                Amount = amount,
                Description = description,
                WalletTransactionId = walletTransactionId,
                ExternalBatchId = batchId,
                TokenId = tokenId
            });

            return gameActionId;
        }
    }
}