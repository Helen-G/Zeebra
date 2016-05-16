using System;
using System.ComponentModel.DataAnnotations;
using AFT.RegoV2.Core.Common.Data.Payment;

namespace AFT.RegoV2.Core.Player.Data
{
    public class IdentificationDocumentSettings
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public Guid LicenseeId { get; set; }

        [Required]
        public Guid BrandId { get; set; }

        public Brand Brand { get; set; }

        public TransactionType? TransactionType { get; set; }

        public PaymentMethod? PaymentMethod { get; set; }

        public bool IdFront { get; set; }

        public bool IdBack { get; set; }

        public bool CreditCardFront { get; set; }

        public bool CreditCardBack { get; set; }

        public bool POA { get; set; }

        public bool DCF { get; set; }

        [Required]
        [MaxLength(200)]
        public string Remark { get; set; }

        public string CreatedBy { get; set; }
        public DateTimeOffset CreatedOn { get; set; }
        public string UpdatedBy { get; set; }
        public DateTimeOffset? UpdatedOn { get; set; }
    }

    public enum TransactionType
    {
        Withdraw,
        Deposit
    }
}
