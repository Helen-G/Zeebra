using System;
using AFT.RegoV2.Core.Common.Data;

namespace AFT.RegoV2.Domain.Payment.ApplicationServices.Data
{
    public class SaveTransferSettingsCommand
    {
        public Guid Id { get; set; }
        public Guid Licensee { get; set; }
        public Guid Brand { get; set; }
        public string TimezoneId { get; set; }
        public TransferFundType TransferType { get; set; }
        public string Currency { get; set; }
        //public string VipLevel { get; set; }
        public Guid VipLevel { get; set; }
        public string Wallet { get; set; }
        public decimal MinAmountPerTransaction { get; set; }
        public decimal MaxAmountPerTransaction { get; set; }
        public decimal MaxAmountPerDay { get; set; }
        public int MaxTransactionPerDay { get; set; }
        public int MaxTransactionPerWeek { get; set; }
        public int MaxTransactionPerMonth { get; set; }
    }
}