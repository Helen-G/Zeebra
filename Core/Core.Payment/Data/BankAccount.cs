using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AFT.RegoV2.Domain.Payment.Data
{
    public class BankAccountId
    {
        private readonly Guid _id;

        public BankAccountId(Guid id)
        {
            _id = id;
        }

        public static implicit operator Guid(BankAccountId id)
        {
            return id._id;
        }

        public static implicit operator BankAccountId(Guid id)
        {
            return new BankAccountId(id);
        }
    }

    public class BankAccount
    {
        public BankAccount()
        {
            PaymentLevels = new List<PaymentLevel>();
        }

        public Guid Id { get; set; }
            
        [Required, MinLength(1), MaxLength(20)]
        public string AccountId { get; set; }

        [Required, MinLength(2), MaxLength(100)]
        public string AccountName { get; set; }

        [Required, MinLength(1), MaxLength(20)]
        public string AccountNumber { get; set; }

        [MinLength(1), MaxLength(20)]
        public string AccountType { get; set; }

        [Required, MinLength(1), MaxLength(200)]
        public string Province { get; set; }

        [Required, MinLength(1), MaxLength(200)]
        public string Branch { get; set; }

        [Required]
        public string CurrencyCode { get; set; }

        [Required]
        public Bank Bank { get; set; }

        [MinLength(1), MaxLength(200)]
        public string Remarks { get; set; }

        [Required]
        public BankAccountStatus Status { get; set; }

        [Required]
        public string CreatedBy { get; set; }

        public DateTime Created { get; set; }

        public string UpdatedBy { get; set; }

        public DateTime? Updated { get; set; }

        public virtual ICollection<PaymentLevel> PaymentLevels { get; set; }
    }
}