using System;
using AFT.RegoV2.Shared;

namespace AFT.RegoV2.MemberApi.Interface.Player
{
    public class ProfileRequest 
    {
    }

    public class ProfileResponse 
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string MailingAddressLine1 { get; set; }
        public string MailingAddressLine2 { get; set; }
        public string MailingAddressLine3 { get; set; }
        public string MailingAddressLine4 { get; set; }
        public string MailingAddressPostalCode { get; set; }
        public string MailingAddressCity { get; set; }
        public string PhysicalAddressLine1 { get; set; }
        public string PhysicalAddressLine2 { get; set; }
        public string PhysicalAddressLine3 { get; set; }
        public string PhysicalAddressLine4 { get; set; }
        public string PhysicalAddressPostalCode { get; set; }
        public string PhysicalAddressCity { get; set; }
        public string CountryCode { get; set; }
        public string CurrencyCode { get; set; }
        public string Comments { get; set; }
        public string Username { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string Title { get; set; }
        public string ContactPreference { get; set; }
        public string IdStatus { get; set; }
        public string AccountStatus { get; set; }
        public string IpAddress { get; set; }
        public string DomainName { get; set; }
        public bool IsPhoneNumberVerified { get; set; }
        public string SecurityQuestion { get; set; }
        public string SecurityQuestionId { get; set; }
        public string SecurityAnswer { get; set; }
    }
}
