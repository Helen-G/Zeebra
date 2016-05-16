namespace AFT.RegoV2.AdminWebsite.Controllers
{
    public class AddPlayerCommand
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
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
        public string Country { get; set; }
        public string Currency { get; set; }
        public string Culture { get; set; }
        public string Comments { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string PasswordConfirm { get; set; }
        public string DateOfBirth { get; set; }
        public string Brand { get; set; }
        public string Gender { get; set; }
        public string Title { get; set; }
        public string ContactPreference { get; set; }
        public string IdStatus { get; set; }
        public string AccountStatus { get; set; }
        public bool InternalAccount { get; set; }
        public bool AccountAlertEmail { get; set; }
        public bool AccountAlertSms { get; set; }
    }
}