using System;
using AFT.RegoV2.Shared;

namespace AFT.RegoV2.MemberApi.Interface.Player
{
    public class ChangeContactInfoRequest 
    {
        public Guid PlayerId { get; set; }
        public string PhoneNumber { get; set; }
        public string MailingAddressLine1 { get; set; }
        public string MailingAddressLine2 { get; set; }
        public string MailingAddressLine3 { get; set; }
        public string MailingAddressLine4 { get; set; }
        public string MailingAddressCity { get; set; }
        public string MailingAddressPostalCode { get; set; }
        public string CountryCode { get; set; }
        public string ContactPreference { get; set; }
    }

    public class ChangeContactInfoResponse 
    {
    }
}