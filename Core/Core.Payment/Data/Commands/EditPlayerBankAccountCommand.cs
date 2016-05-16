using System;

namespace AFT.RegoV2.Domain.Payment.Commands
{
    public class EditPlayerBankAccountCommand
    {
        public Guid? Id { get; set; }
        public Guid? PlayerId { get; set; }
        public Guid Bank { get; set; }
        public string AccountName { get; set; }
        public string AccountNumber { get; set; }
        public string Province { get; set; }
        public string City { get; set; }
        public string Branch { get; set; }
        public string SwiftCode { get; set; }
        public string Address { get; set; }
    }
}