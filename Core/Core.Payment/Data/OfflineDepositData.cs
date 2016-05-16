using System;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Payment;

namespace AFT.RegoV2.Domain.Payment.Data
{
    public enum DepositMethod
    {
        InternetBanking,
        ATM,
        CounterDeposit
    }

    public class OfflineDepositId
    {
        private readonly Guid _id;

        public OfflineDepositId(Guid id)
        {
            _id = id;
        }

        public static implicit operator Guid(OfflineDepositId id)
        {
            return id._id;
        }

        public static implicit operator OfflineDepositId(Guid id)
        {
            return new OfflineDepositId(id);
        }
    }

    public class OfflineDeposit
    {
        public Guid Id { get; set; }

        public Guid BrandId { get; set; }
        public Brand Brand { get; set; }

        public Guid PlayerId { get; set; }
        public Core.Payment.Data.Player Player { get; set; }

        public Guid BankAccountId { get; set; }
        public BankAccount BankAccount { get; set; }

        public string CurrencyCode { get; set; }

        public string TransactionNumber { get; set; }

        public DateTime Created { get; set; }

        public string CreatedBy { get; set; }

        public DateTime? Verified { get; set; }

        public string VerifiedBy { get; set; }

        public DateTime? Approved { get; set; }

        public string ApprovedBy { get; set; }

        public NotificationMethod NotificationMethod { get; set; }

        public OfflineDepositStatus Status { get; set; }

        public string PlayerAccountName { get; set; }

        public string PlayerAccountNumber { get; set; }

        public string ReferenceNumber { get; set; }

        public decimal Amount { get; set; }

        public decimal ActualAmount { get; set; }

        public decimal Fee { get; set; }

        public PaymentMethod PaymentMethod { get; set; }

        public TransferType TransferType { get; set; }

        public DepositMethod DepositMethod { get; set; }

        public DepositType DepositType { get; set; }

        public string IdFrontImage { get; set; }

        public string IdBackImage { get; set; }

        public string ReceiptImage { get; set; }

        public string Remark { get; set; }

        public string PlayerRemark { get; set; }

        public decimal DepositWagering { get; set; }

        public string BonusCode { get; set; }

        public UnverifyReasons? UnverifyReason { get; set; }
    }
}