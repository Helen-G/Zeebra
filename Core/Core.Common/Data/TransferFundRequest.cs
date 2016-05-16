using System;
using System.ComponentModel.DataAnnotations;

namespace AFT.RegoV2.Core.Common.Data
{
    public class TransferFundRequest
    {
        [Required]
        public Guid PlayerId { get; set; }

        [Required]
        public TransferFundType TransferType { get; set; }

        [Required]
        public string WalletId { get; set; }

        [Range(0.01, int.MaxValue)]
        public decimal Amount { get; set; }

        public string BonusCode { get; set; }
    }
}
