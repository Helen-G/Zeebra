using System;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Player.Data;

namespace AFT.RegoV2.Core.Domain.Player.Data
{
    public class RegistrationData
    {
        public RegistrationData()
        {
            AccountStatus = Core.Player.Data.AccountStatus.Active.ToString();
            IdStatus = Core.Player.Data.IdStatus.Unverified.ToString();
        }

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
        public string CountryCode { get; set; }
        public string CurrencyCode { get; set; }
        public string CultureCode { get; set; }
        public string Comments { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string PasswordConfirm { get; set; }
        public string DateOfBirth { get; set; }
        public string BrandId { get; set; }
        public string Gender { get; set; }
        public string Title { get; set; }
        public string ContactPreference { get; set; }
        public string IpAddress { get; set; }
        public string DomainName { get; set; }
        public string SecurityQuestionId { get; set; }
        public string SecurityAnswer { get; set; }
        public string AccountStatus { get; set; }
        public bool AccountAlertEmail { get; set; }
        public bool AccountAlertSms { get; set; }
        public string IdStatus { get; set; }
        public bool InternalAccount { get; set; }
        public bool IsRegisteredFromAdminSite { get; set; }
        public string ReferralId { get; set; }
    }

    public class EditPlayerData
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
        public string CurrencyCode { get; set; }
        public ContactMethod ContactPreference { get; set; }
        public bool AccountAlertEmail { get; set; }
        public bool AccountAlertSms { get; set; }
        public bool MarketingAlertEmail { get; set; }
        public bool MarketingAlertSms { get; set; }
        public bool MarketingAlertPhone { get; set; }
        public Guid PaymentLevelId { get; set; }
    }

    public class SendNewPasswordData
    {
        public Guid PlayerId { get; set; }
        public string NewPassword { get; set; }
        public SendBy SendBy { get; set; }
    }

    public enum SendBy
    {
        Email,
        Sms
    }
}