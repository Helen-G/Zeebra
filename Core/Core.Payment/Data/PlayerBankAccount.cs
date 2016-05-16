using System;
using System.ComponentModel.DataAnnotations;
using AFT.RegoV2.Domain.Payment.Data;

namespace AFT.RegoV2.Core.Payment.Data
{
    public class PlayerBankAccountId
    {
        private readonly Guid _id;

        public PlayerBankAccountId(Guid id)
        {
            _id = id;
        }

        public static implicit operator Guid(PlayerBankAccountId id)
        {
            return id._id;
        }

        public static implicit operator PlayerBankAccountId(Guid id)
        {
            return new PlayerBankAccountId(id);
        }
    }

    public class PlayerBankAccount
    {
        public Guid Id { get; set; }
            
        [Required]
        public Player Player { get; set; }

        [Required, MinLength(2), MaxLength(100)]
        public string AccountName { get; set; }

        [Required, MinLength(1), MaxLength(50)]
        public string AccountNumber { get; set; }

        [Required, MinLength(1), MaxLength(200)]
        public string Province { get; set; }

        [Required, MinLength(1), MaxLength(200)]
        public string City { get; set; }

        [MinLength(1), MaxLength(200)]
        public string Branch { get; set; }

        [MinLength(1), MaxLength(200)]
        public string SwiftCode { get; set; }

        [MinLength(1), MaxLength(200)]
        public string Address { get; set; }

        [Required]
        public Bank Bank { get; set; }

        [MinLength(1), MaxLength(200)]
        public string Remarks { get; set; }

        [Required]
        public BankAccountStatus Status { get; set; }

        public bool EditLock { get; set; }

        [Required]
        public string CreatedBy { get; set; }

        public DateTimeOffset Created { get; set; }

        public string UpdatedBy { get; set; }

        public DateTimeOffset? Updated { get; set; }

        public string VerifiedBy { get; set; }

        public DateTimeOffset? Verified { get; set; }

        public string RejectedBy { get; set; }

        public DateTimeOffset? Rejected { get; set; }

        public bool IsCurrent { get; set; }
    }
}