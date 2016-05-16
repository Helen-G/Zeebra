using System;
using System.Collections.Generic;
using AFT.RegoV2.Core.Common.Data;

namespace AFT.RegoV2.Core.Player.Data
{
    public class Player
    {
        public Guid             Id { get; set; }
        public Guid             BrandId { get; set; }
        /// <summary>
        /// if not specified - Brand's DefaultVipLevelId will be used
        /// </summary>
        public Guid?            VipLevelId { get; set; } 
        public Guid             PaymentLevelId { get; set; }

        public string           FirstName { get; set; }
        public string           LastName { get; set; }
        public string           Email { get; set; }
        public string           PhoneNumber { get; set; }
        public string           MailingAddressLine1 { get; set; }
        public string           MailingAddressLine2 { get; set; }
        public string           MailingAddressLine3 { get; set; }
        public string           MailingAddressLine4 { get; set; }
        public string           MailingAddressCity { get; set; }
        public string           MailingAddressPostalCode { get; set; }
        public string           PhysicalAddressLine1 { get; set; }
        public string           PhysicalAddressLine2 { get; set; }
        public string           PhysicalAddressLine3 { get; set; }
        public string           PhysicalAddressLine4 { get; set; }
        public string           PhysicalAddressCity { get; set; }
        public string           PhysicalAddressPostalCode { get; set; }
        public string           CountryCode { get; set; }
        public string           CurrencyCode { get; set; }
        public string           CultureCode { get; set; }
        public string           Comments { get; set; }
        public string           Username { get; set; }
        public string           PasswordEncrypted { get; set; }
        public DateTime         DateOfBirth { get; set; }
        public DateTimeOffset   DateRegistered { get; set; }
        public Gender           Gender { get; set; }
        public Title            Title { get; set; }
        public ContactMethod    ContactPreference { get; set; }
        public bool             AccountAlertEmail { get; set; }
        public bool             AccountAlertSms { get; set; }
        public bool             MarketingAlertEmail { get; set; }
        public bool             MarketingAlertSms { get; set; }
        public bool             MarketingAlertPhone { get; set; }
        public IdStatus         IdStatus { get; set; }
        public AccountStatus    AccountStatus { get; set; }
        public string           IpAddress { get; set; }
        public string           DomainName { get; set; }
        public string           AccountActivationEmailToken { get; set; }
        public string           AccountActivationSmsToken { get; set; }
        public bool             IsPhoneNumberVerified { get; set; }
        public string           EmailVerificationToken { get; set; }
        public int              MobileVerificationCode { get; set; }
        public Guid?            SecurityQuestionId { get; set; }
        public string           SecurityAnswer { get; set; }
        public bool             InternalAccount { get; set; }
        public Guid?            ReferralId { get; set; }
        public int              FailedLoginAttempts { get; set; }
        public DateTimeOffset?  LastActivityDate { get; set; }

        public Brand                                Brand { get; set; }
        public VipLevel                             VipLevel { get; set; }
        public ICollection<IdentityVerification>    IdentityVerifications { get; set; }


        public bool IsOnline
        {
            get { return LastActivityDate.HasValue && LastActivityDate > DateTimeOffset.Now.AddMinutes(-20); }
        }

    }

    public enum Gender
    {
        Male,
        Female
    }

    public enum Title
    {
        Mr,
        Ms,
        Mrs,
        Miss
    }

    public enum AccountStatus
    {
        Active,
        Inactive,
        SelfExcluded,
        Locked
    }

    public enum IdStatus 
    {
        Verified,
        Unverified
    }

    public enum ContactMethod
    {
        Email,
        Chat,
        Phone,
        Sms
    }
}
