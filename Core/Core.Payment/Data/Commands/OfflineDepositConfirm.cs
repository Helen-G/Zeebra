using System;
using System.ComponentModel.DataAnnotations;
using AFT.RegoV2.Domain.Payment.Data;

namespace AFT.RegoV2.Domain.Payment.Commands
{
    public class OfflineDepositConfirm
    {
        [Required]
        public Guid Id { get; set; }

        [Required, MinLength(2), MaxLength(100)]
        public string PlayerAccountName { get; set; }

        [Required, MinLength(1), MaxLength(50)]
        public string PlayerAccountNumber { get; set; }

        [MinLength(2), MaxLength(50)]
        public string ReferenceNumber { get; set; }

        [Range(0.01, int.MaxValue)]
        public decimal Amount { get; set; }

        public Guid BankId { get; set; }

        public TransferType TransferType { get; set; }

        public DepositMethod OfflineDepositType { get; set; }

        public string IdFrontImage { get; set; }

        public string IdBackImage { get; set; }

        public string ReceiptImage { get; set; }

        [MaxLength(200)]
        public string Remark { get; set; }
    }
}