using System;

namespace AFT.RegoV2.Core.Payment.Validators
{
    public class AddBankAccountData
    {
        public Guid Bank { get; set; }
        public string Currency { get; set; }
        public string AccountId { get; set; }
        public string AccountName { get; set; }
        public string AccountNumber { get; set; }
        public string AccountType { get; set; }
        public string Province { get; set; }
        public string Branch { get; set; }
        public string Remarks { get; set; }
    }
}