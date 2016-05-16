using System;
using System.ComponentModel.DataAnnotations;
using AFT.RegoV2.Core.Player.Data;

namespace AFT.RegoV2.Core.Common.Data
{
    public class PlayerInfoLog
    {
        public Guid Id { get; set; }
        public virtual Core.Player.Data.Player Player { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }

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
        public string PasswordEncrypted { get; set; }
        public DateTime DateOfBirth { get; set; }
        public DateTimeOffset DateRegistered { get; set; }
        public Guid BrandId { get; set; }
        public Guid PaymentLevelId { get; set; }
        public Gender Gender { get; set; }
        public Title Title { get; set; }
        public ContactMethod ContactPreference { get; set; }
        public IdStatus IdStatus { get; set; }
        public AccountStatus AccountStatus { get; set; }
        public string IpAddress { get; set; }
        public string DomainName { get; set; }
        public string AccountActivationToken { get; set; }
        public bool IsPhoneNumberVerified { get; set; }
        public int MobileVerificationCode { get; set; }
        public Guid? SecurityQuestionId { get; set; }
        public string SecurityAnswer { get; set; }
        public bool InternalAccount { get; set; }
        public Guid? ReferralId { get; set; }
    }
}
