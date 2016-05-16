using System;
using System.ComponentModel.DataAnnotations;
using AFT.RegoV2.Core.Common.Data.Payment;
using AFT.RegoV2.Core.Payment.Data;

namespace AFT.RegoV2.Domain.Payment.Data
{
    public class OfflineWithdrawId
    {
        private readonly Guid _id;

        public OfflineWithdrawId(Guid id)
        {
            _id = id;
        }

        public static implicit operator Guid(OfflineWithdrawId id)
        {
            return id._id;
        }

        public static implicit operator OfflineWithdrawId(Guid id)
        {
            return new OfflineWithdrawId(id);
        }
    }

    public class OfflineWithdraw
    {
        public Guid Id { get; set; }

        public PlayerBankAccount PlayerBankAccount { get; set; }

        [Required]
        public string TransactionNumber { get; set; }

        [Range(0.01, int.MaxValue)]
        public decimal Amount { get; set; }

        public DateTimeOffset Created { get; set; }

        [Required]
        public string CreatedBy { get; set; }

        public DateTimeOffset? Verified { get; set; }

        public string VerifiedBy { get; set; }

        public DateTimeOffset? Unverified { get; set; }

        public string UnverifiedBy { get; set; }

        public DateTimeOffset? Approved { get; set; }

        public string ApprovedBy { get; set; }

        public DateTimeOffset? Rejected { get; set; }

        public string RejectedBy { get; set; }

        [MinLength(1), MaxLength(200)]
        public string Remarks { get; set; }

        public WithdrawalStatus Status { get; set; }

        public PaymentMethod PaymentMethod { get; set; }

        public bool AutoVerify { get; set; }

        public DateTimeOffset? AutoVerifyTime { get; set; }

        public bool Exempted { get; set; }

        public DateTimeOffset? ExemptionCheckTime { get; set; }

        public bool AutoWagerCheck { get; set; }

        public DateTimeOffset? AutoWagerCheckTime { get; set; }

        public bool WagerCheck { get; set; }

        public string InvestigatedBy { get; set; }

        public DateTimeOffset? InvestigatedDate { get; set; }

        public string AcceptedBy { get; set; }

        public DateTimeOffset? AcceptedTime { get; set; }

        public string RevertedBy { get; set; }

        public DateTimeOffset? RevertedTime { get; set; }

        public string CanceledBy { get; set; }

        public DateTimeOffset? CanceledTime { get; set; }
    }
}