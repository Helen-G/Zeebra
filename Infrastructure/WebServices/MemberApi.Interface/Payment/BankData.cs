using System;

namespace AFT.RegoV2.MemberApi.Interface.Payment
{
    public class BankData
    {
        public string BankName { get; set; }

        public string Province { get; set; }

        public string City { get; set; }

        public string Branch { get; set; }

        public string SwiftCode { get; set; }

        public string BankAccountName { get; set; }

        public string BankAccountNumber { get; set; }

        public Guid PlayerBankAccountId { get; set; }

        public DateTimeOffset BankTime { get; set; }

        public DateTimeOffset BankAccountTime { get; set; }
    }
}
