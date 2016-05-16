using System;
using AFT.RegoV2.Core.Player.Data;

namespace AFT.RegoV2.AdminWebsite.Controllers
{
    public class EditPlayerCommand
    {
        public Guid PlayerId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DateOfBirth { get; set; }
        public Title? Title { get; set; }
        public Gender? Gender { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string MailingAddressLine1 { get; set; }
        public string MailingAddressLine2 { get; set; }
        public string MailingAddressLine3 { get; set; }
        public string MailingAddressLine4 { get; set; }
        public string MailingAddressCity { get; set; }
        public string MailingAddressPostalCode { get; set; }
        public string PhysicalAddressLine1 { get; set; }
        public string PhysicalAddressLine2 { get; set; }
        public string PhysicalAddressLine3 { get; set; }
        public string PhysicalAddressLine4 { get; set; }
        public string PhysicalAddressCity { get; set; }
        public string PhysicalAddressPostalCode { get; set; }
        public string CountryCode { get; set; }
        public ContactMethod ContactPreference { get; set; }
        public bool AccountAlertEmail { get; set; }
        public bool AccountAlertSms { get; set; }
        public bool MarketingAlertEmail { get; set; }
        public bool MarketingAlertSms { get; set; }
        public bool MarketingAlertPhone { get; set; }
        public Guid PaymentLevelId { get; set; }
    }
}