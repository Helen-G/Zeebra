using System;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Common.Events
{
    public class PlayerRegistered : DomainEventBase
    {
        public Guid PlayerId { get; set; }
        public Guid BrandId { get; set; }
        public string CountryCode { get; set; }
        public string CountryName { get; set; }
        public Guid VipLevelId { get; set; }
        public string VipLevel { get; set; }
        public string CurrencyCode { get; set; }
        public DateTimeOffset DateRegistered { get; set; }
        public string DisplayName { get; set; }
        public Guid? PaymentLevelId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string AccountActivationToken { get; set; }
        public Guid? ReferralId { get; set; }
        public string IPAddress { get; set; }
        public string Title { get; set; }
        public string Gender { get; set; }
        public DateTimeOffset DateOfBirth { get; set; }
        public string[] AddressLines { get; set; }
        public string ZipCode { get; set; }
        public string Language { get; set; }
        public string CultureCode { get; set; }
        public string Status { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool AccountAlertEmail { get; set; }
        public bool AccountAlertSms { get; set; }
    }
}