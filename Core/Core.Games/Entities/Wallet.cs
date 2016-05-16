using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Wallet;
using AFT.RegoV2.Core.Common.Events.Wallet;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Game.Data;
using AFT.RegoV2.Core.Game.Events;
using AFT.RegoV2.Core.Game.Exceptions;
using AFT.RegoV2.Shared;

namespace AFT.RegoV2.Core.Game.Entities
{
    public class Wallet
    {
        private readonly Data.Wallet _data;
        private readonly List<IDomainEvent> _events;

        internal Data.Wallet Data { get { return _data; } }
        internal IEnumerable<IDomainEvent> Events { get { return _events; } }

        public Wallet(Data.Wallet wallet)
        {
            _data = wallet;
            _events = new List<IDomainEvent>();
        }

        public Transaction Deposit(decimal amount, string transactionNumber)
        {
            ValidateOperationAmount(amount);
            _data.Main += amount;

            var transaction = new Transaction(_data)
            {
                Type = TransactionType.Deposit,
                MainBalanceAmount = amount,
                TransactionNumber = transactionNumber,
            };

            transaction.Description = "Game, Deposit, " + amount.ToString("F") + " ";

            AddTransaction(transaction);

            return transaction;
        }
        public Transaction Withdraw(decimal amount, string transactionNumber)
        {
            ValidateOperationAmount(amount);

            if (_data.Locks.Any(x => x.LockType == LockType.Withdrawal && x.Amount == amount))
                Unlock(amount, LockType.Withdrawal, String.Empty);

            if (_data.Free < amount)
                throw new InsufficientFundsException();

            _data.Main -= amount;

            var transaction = new Transaction(_data)
            {
                MainBalanceAmount = -amount,
                Type = TransactionType.Withdraw,
                TransactionNumber = transactionNumber,
            };

            transaction.Description = "Game, Withdraw, " + amount.ToString("F") + " ";
            
            AddTransaction(transaction);

            return transaction;
        }

        public Transaction TransferFundCredit(decimal amount, string transactionNumber)
        {
            ValidateOperationAmount(amount);
            _data.Main += amount;

            var transaction = new Transaction(_data)
            {
                PerformedBy = _data.PlayerId,
                Type = Data.Template.IsMain ? TransactionType.FundOut : TransactionType.FundIn,
                MainBalanceAmount = amount,
                TransactionNumber = transactionNumber,
            };

            transaction.Description = "Game, Transfer Fund Credit, " + amount.ToString("F") + " ";

            AddTransaction(transaction);

            return transaction;
        }
        public Transaction TransferFundDebit(decimal amount, string transactionNumber)
        {
            ValidateOperationAmount(amount);

            if (_data.Locks.Any(x => x.LockType == LockType.Withdrawal && x.Amount == amount))
                Unlock(amount, LockType.Withdrawal, String.Empty);

            if (_data.Free < amount)
                throw new InsufficientFundsException();

            _data.Main -= amount;

            var transaction = new Transaction(_data)
            {
                PerformedBy = _data.PlayerId,
                MainBalanceAmount = -amount,
                Type = Data.Template.IsMain ? TransactionType.FundIn : TransactionType.FundOut,
                TransactionNumber = transactionNumber,
            };

            transaction.Description = "Game, Transfer Fund Debit, " + amount.ToString("F") + " ";

            AddTransaction(transaction);

            return transaction;
        }

        public void IssueBonus(BalanceTarget target, decimal amount)
        {
            ValidateOperationAmount(amount);

            var transaction = new Transaction(_data)
            {
                Type = TransactionType.Bonus
            };

            if (target == BalanceTarget.Main)
            {
                _data.Main += amount;
                transaction.MainBalanceAmount = amount;
                transaction.MainBalance = _data.Main;
            }
            else
            {
                _data.Bonus += amount;
                transaction.BonusBalanceAmount = amount;
                transaction.BonusBalance = _data.Bonus;
            }

            transaction.Description = "Game, Issue Bonus, " + amount.ToString("F") + " ";

            AddTransaction(transaction);
        }
        public void AdjustBalances(AdjustmentParams adjustment)
        {
            _data.Temporary += adjustment.TemporaryBalanceAdjustment;
            _data.Main += adjustment.MainBalanceAdjustment;
            _data.Bonus += adjustment.BonusBalanceAdjustment;

            if (_data.Bonus < 0) throw new InsufficientFundsException();
            if (_data.Temporary < 0) throw new InsufficientFundsException();

            var transaction = new Transaction(_data)
            {
                Type = GetTransactionType(adjustment.Reason),
                MainBalanceAmount = adjustment.MainBalanceAdjustment,
                BonusBalanceAmount = adjustment.BonusBalanceAdjustment,
                TemporaryBalanceAmount = adjustment.TemporaryBalanceAdjustment,
                RelatedTransactionId = adjustment.RelatedTransactionId
            };

            var amount = adjustment.MainBalanceAdjustment + adjustment.BonusBalanceAdjustment +
                         adjustment.TemporaryBalanceAdjustment;
            transaction.Description = "Game, Adjust Balances, " + amount.ToString("F") + " ";

            AddTransaction(transaction);
        }
        public void AdjustBetWonTransaction(BetWonAdjustmentParams adjustment)
        {
            var transactionId = adjustment.RelatedTransactionId;
            var betWonTrx = _data.Transactions.Single(tr => tr.Id == transactionId);
            if (betWonTrx.Type != TransactionType.BetWon)
                throw new InvalidOperationException("Bet won transaction can be adjusted only.");

            var betWonCancelled = _data.Transactions.Any(tr =>
                tr.Type == TransactionType.BetCancelled &&
                tr.RelatedTransactionId == betWonTrx.Id);
            if (betWonCancelled == false)
            {
                var roundId = betWonTrx.RoundId.Value;
                var amount = betWonTrx.TemporaryBalanceAmount;
                var betPlacedTransactions = GetBetPlacedTransactions(roundId);

                _data.Temporary -= amount;
                var transaction = new Transaction(_data)
                {
                    Type = TransactionType.BetWonAdjustment,
                    GameId = betPlacedTransactions.First().GameId,
                    RoundId = roundId,
                    RelatedTransactionId = transactionId,
                    TemporaryBalanceAmount = -amount
                };

                if (_data.Temporary < 0) throw new InsufficientFundsException();

                if (adjustment.BetWonDuringRollover)
                {
                    _data.Bonus += amount;
                    transaction.BonusBalanceAmount = amount;
                    transaction.BonusBalance = _data.Bonus;
                }
                else
                {
                    ApplyBetWonProportion(betPlacedTransactions, amount, transaction);
                }

                transaction.Description = "Game, Adjust Bet Won, " + amount.ToString("F") + " "; 

                AddTransaction(transaction);
            }
        }

        public Transaction PlaceBet(decimal amount, Guid roundId, Guid gameId)
        {
            ValidateOperationAmount(amount);
            if (_data.Playable < amount) throw new InsufficientFundsException();

            decimal mainBalanceDebit;
            var bonusBalanceDebit = 0m;
            if (_data.Main >= amount)
            {
                mainBalanceDebit = -amount;
            }
            else
            {
                mainBalanceDebit = -_data.Main;
                bonusBalanceDebit = _data.Main - amount;
            }

            _data.Main += mainBalanceDebit;
            _data.Bonus += bonusBalanceDebit;

            var transaction = new Transaction(_data)
            {
                Type = TransactionType.BetPlaced,
                MainBalanceAmount = mainBalanceDebit,
                BonusBalanceAmount = bonusBalanceDebit,
                GameId = gameId,
                RoundId = roundId
            };

            transaction.Description = "Game, Place Bet, " + amount.ToString("F") + " "; 
            
            AddTransaction(transaction);

            return transaction;
        }
        public Transaction WinBet(Guid roundId, decimal amount)
        {
            ValidateOperationAmount(amount);
            var betPlacedTransactions = GetBetPlacedTransactions(roundId);

            var transaction = new Transaction(_data)
            {
                Type = TransactionType.BetWon,
                GameId = betPlacedTransactions.First().GameId,
                RoundId = roundId
            };

            if (_data.HasWageringRequirement)
            {
                _data.Temporary += amount;
                transaction.TemporaryBalanceAmount = amount;
                transaction.TemporaryBalance = _data.Temporary;
            }
            else
            {
                ApplyBetWonProportion(betPlacedTransactions, amount, transaction);
            }

            transaction.Description = "Game, Win Bet, " + amount.ToString("F") + " ";
            
            AddTransaction(transaction);

            return transaction;
        }
        public Transaction LoseBet(Guid roundId)
        {
            var betPlacedTransactions = GetBetPlacedTransactions(roundId);

            var averageBetAmountFromMb = betPlacedTransactions.Average(tr => tr.MainBalanceAmount);
            var averageBetAmountFromBb = betPlacedTransactions.Average(tr => tr.BonusBalanceAmount);
            var mbContributionRounded = Math.Round(averageBetAmountFromMb, 6);
            var bbContributionRounded = Math.Round(averageBetAmountFromBb, 6);

            var transaction = new Transaction(_data)
            {
                Type = TransactionType.BetLost,
                MainBalanceAmount = mbContributionRounded,
                BonusBalanceAmount = bbContributionRounded,
                GameId = betPlacedTransactions.First().GameId,
                RoundId = roundId
            };

            var amount = mbContributionRounded + bbContributionRounded;
            transaction.Description = "Game, Loose Bet, " + amount.ToString("F") + " "; 

            AddTransaction(transaction);

            return transaction;
        }
        public Transaction CancelBet(Guid transactionId)
        {
            var trxToCancel = _data.Transactions.SingleOrDefault(tr => tr.Id == transactionId);
            if (trxToCancel == null)
            {
                throw new RegoException("Bet with such transactionId does not exist.");
            }
            var trxType = trxToCancel.Type;
            if (trxType != TransactionType.BetPlaced && trxType != TransactionType.BetWon &&
                trxType != TransactionType.BetLost)
            {
                throw new InvalidOperationException("Transaction type is not supported.");
            }
            var duplicateCancellation = _data.Transactions.Any(tr =>
                    tr.Type == TransactionType.BetCancelled &&
                    tr.RelatedTransactionId == trxToCancel.Id);
            if (duplicateCancellation)
            {
                throw new InvalidOperationException("Bet was already canceled.");
            }

            _data.Main -= trxToCancel.MainBalanceAmount;
            _data.Bonus -= trxToCancel.BonusBalanceAmount;
            _data.Temporary -= trxToCancel.TemporaryBalanceAmount;

            var transaction = new Transaction(_data)
            {
                Type = TransactionType.BetCancelled,
                GameId = trxToCancel.GameId,
                RoundId = trxToCancel.RoundId,
                RelatedTransactionId = trxToCancel.Id,
                MainBalanceAmount = -trxToCancel.MainBalanceAmount,
                BonusBalanceAmount = -trxToCancel.BonusBalanceAmount,
                TemporaryBalanceAmount = -trxToCancel.TemporaryBalanceAmount
            };

            var amount = trxToCancel.MainBalanceAmount + trxToCancel.BonusBalanceAmount +
                         trxToCancel.TemporaryBalanceAmount;
            transaction.Description = "Game, Cancel Bet, " + amount.ToString("F") + " ";

            AddTransaction(transaction);

            return transaction;
        }

        public Lock Lock(decimal amount, LockType type, string description)
        {
            ValidateOperationAmount(amount);
            if (_data.Total < amount)
                throw new InsufficientFundsException();

            switch (type)
            {
                case LockType.Withdrawal:
                    _data.WithdrawalLock += amount;
                    break;
                case LockType.Fraud:
                    _data.FraudLock += amount;
                    break;
                case LockType.Bonus:
                    _data.BonusLock += amount;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var lockData = new Lock
            {
                Amount = amount,
                LockType = type,
                Description = description,
                CreatedOn = DateTimeOffset.Now.ToBrandOffset(Data.Brand.TimezoneId)
            };
            AddLock(lockData);

            return lockData;
        }
        public Lock Unlock(decimal amount, LockType type, string description)
        {
            ValidateOperationAmount(amount);

            switch (type)
            {
                case LockType.Withdrawal:
                    if (_data.WithdrawalLock < amount)
                        throw new InvalidUnlockAmount();

                    _data.WithdrawalLock -= amount;
                    break;
                case LockType.Bonus:
                    if (_data.BonusLock < amount)
                        throw new InvalidUnlockAmount();

                    _data.BonusLock -= amount;
                    break;
                case LockType.Fraud:
                    if (_data.FraudLock < amount)
                        throw new InvalidUnlockAmount();

                    _data.FraudLock -= amount;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var lockData = new Lock
            {
                Amount = -amount,
                LockType = type,
                Description = description,
                CreatedOn = DateTimeOffset.Now.ToBrandOffset(Data.Brand.TimezoneId)
            };
            AddLock(lockData);

            return lockData;
        }

        public void SetHasWageringRequirement(bool flagValue)
        {
            _data.HasWageringRequirement = flagValue;
        }

        private void AddTransaction(Transaction transaction)
        {
            _data.Transactions.Add(transaction);
            _events.Add(new TransactionProcessed
            {
                TransactionId = transaction.Id,
                CreatedOn = transaction.CreatedOn,
                RelatedTransactionId = transaction.RelatedTransactionId,
                Wallet = new WalletData
                {
                    Id = transaction.WalletId,
                    PlayerId = Data.PlayerId,
                    WalletTemplateId = Data.Template.Id,
                    Total = Data.Total,
                    Playable = Data.Playable,
                    Main = Data.Main,
                    Bonus = Data.Bonus,
                    Temporary = Data.Temporary,
                    BonusLock = Data.BonusLock,
                    FraudLock = Data.FraudLock,
                    WithdrawalLock = Data.WithdrawalLock
                },
                PaymentType = transaction.Type,
                MainBalanceAmount = transaction.MainBalanceAmount,
                BonusBalanceAmount = transaction.BonusBalanceAmount,
                TemporaryBalanceAmount = transaction.TemporaryBalanceAmount,
                RoundId = transaction.RoundId,
                GameId = transaction.GameId,
                Description = transaction.Description,
                PerformedBy = transaction.PerformedBy,
                TransactionNumber = transaction.TransactionNumber,
            });
        }
        private void AddLock(Lock lockData)
        {
            _data.Locks.Add(lockData);
            _events.Add(new LockApplied(lockData, _data));
        }

        private void ValidateOperationAmount(decimal amount)
        {
            if (amount <= 0)
                throw new InvalidAmountException();
        }
        private void ApplyBetWonProportion(List<Transaction> betPlacedTransactions, decimal amount, Transaction transaction)
        {
            var betMbAmount = betPlacedTransactions.Sum(tr => Math.Abs(tr.MainBalanceAmount));
            var betBbAmount = betPlacedTransactions.Sum(tr => Math.Abs(tr.BonusBalanceAmount));
            var betTotalAmount = betMbAmount + betBbAmount;
            var mbContributionRounded = Math.Round(amount * (betMbAmount / betTotalAmount), 6);
            var bbContributionRounded = Math.Round(amount * (betBbAmount / betTotalAmount), 6);

            _data.Main += mbContributionRounded;
            _data.Bonus += bbContributionRounded;

            transaction.MainBalance = _data.Main;
            transaction.BonusBalance = _data.Bonus;

            transaction.MainBalanceAmount = mbContributionRounded;
            transaction.BonusBalanceAmount = bbContributionRounded;
        }
        private TransactionType GetTransactionType(AdjustmentReason adjustmentReason)
        {
            switch (adjustmentReason)
            {
                case AdjustmentReason.BonusCancelled:
                    return TransactionType.BonusCancelled;
                case AdjustmentReason.WageringFinished:
                    return TransactionType.WageringFinished;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        private List<Transaction> GetBetPlacedTransactions(Guid roundId)
        {
            var betPlacedTransactions =
                _data.Transactions.Where(tr => tr.RoundId == roundId && tr.Type == TransactionType.BetPlaced).ToList();
            if (betPlacedTransactions.Any() == false)
            {
                Trace.WriteLine("No bets were placed with this roundId.");
                throw new RegoException("No bets were placed with this roundId.");
            }

            return betPlacedTransactions;
        }
    }
}