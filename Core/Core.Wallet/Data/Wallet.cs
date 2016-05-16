using System;
using System.Collections.Generic;

namespace AFT.RegoV2.Core.Wallet.Data
{
    public class Wallet
    {
        public Wallet()
        {
            Id = Guid.NewGuid();
            Transactions = new List<Transaction>();
            Locks = new List<Lock>();
            Template = new WalletTemplate();
        }

        public Guid     Id { get; set; }
        public Guid     PlayerId { get; set; }
        public Guid     BrandId { get; set; }
        public virtual WalletTemplate Template { get; set; }
        public bool     HasWageringRequirement { get; set; }

        /// <summary>
        /// The amount available to bet, excluding Bonus Balance
        /// </summary>
        public decimal Main { get; set; }

        /// <summary>
        /// The total amount awarded from bonus redemptions and winnings made while fulfilling the wagering requirement of a bonus
        /// </summary>
        public decimal Bonus { get; set; }

        /// <summary>
        /// Bet winnings that have to be processed by Bonus domain in order to distribute funds between Main And Bonus.
        /// These funds can not be used for betting/withdrawal.
        /// </summary>
        public decimal Temporary { get; set; }

        /// <summary>
        /// The sum total of the entire wallet balance, regardless of restrictions
        /// </summary>
        public decimal Total { get { return Main + Bonus + Temporary; } }

        /// <summary>
        /// The amount available to bet, including Bonus Balance
        /// </summary>
        public decimal Playable { get { return Main + Bonus - FraudLock - WithdrawalLock; } }

        /// <summary>
        /// The amount available to freely transfer out of the wallet
        /// </summary>
        public decimal Free
        {
            get
            {
                var balance = Playable - Math.Max(Bonus, BonusLock);
                return balance < 0 ? 0 : balance;
            }
        }

        /// <summary>
        /// The total amount locked by initial bonus conditions. Once wagering requirements are met for a bonus, this lock is removed
        /// </summary>
        public decimal BonusLock { get; set; }

        /// <summary>
        /// Fraud Lock
        /// </summary>
        public decimal FraudLock { get; set; }

        /// <summary>
        /// Withdrawal Lock
        /// </summary>
        public decimal WithdrawalLock { get; set; }

        public virtual ICollection<Transaction> Transactions { get; set; }
        public virtual ICollection<Lock>        Locks { get; set; }
    }

    public class WalletTemplate
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid BrandId { get; set; }
        public bool IsMain { get; set; }
        public string CurrencyCode { get; set; }
        public bool IsArchived { get; set; }
    }
}