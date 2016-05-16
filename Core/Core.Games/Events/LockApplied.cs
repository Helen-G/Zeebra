using System;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Wallet;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Game.Data;

namespace AFT.RegoV2.Core.Game.Events
{
    public class LockApplied : DomainEventBase
    {
        public LockApplied() { } // default constructor is required for publishing event to MQ

        public LockApplied(Lock lockData, Wallet data)
        {
            LockId = lockData.Id;
            LockType = lockData.LockType;
            Description = lockData.Description;
            Wallet = new WalletData
            {
                Id = data.Id,
                PlayerId = data.PlayerId,
                WalletTemplateId = data.Template.Id,
                Total = data.Total,
                Main = data.Main,
                Bonus = data.Bonus,
                BonusLock = data.BonusLock,
                FraudLock = data.FraudLock,
                WithdrawalLock = data.WithdrawalLock
            };
        }

        public WalletData       Wallet { get; set; }
        public Guid             LockId { get; set; }
        public LockType         LockType { get; set; }
        public string           Description { get; set; }
        public Guid?            PerformedBy { get; set; }
    }
}