using System;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Domain.Player.Events
{
    public class PlayerUpdated : DomainEventBase
    {
        public PlayerData Player { get; set; }

        public class PlayerData
        {
            public Guid             Id { get; set; }
            public Guid?            PaymentLevelId { get; set; }
            public string           VipLevel { get; set; }
            public Guid             VipLevelId { get; set; }
            public string           DisplayName { get; set; }
            public DateTimeOffset   DateOfBirth { get; set; }
            public string           Title { get; set; }
            public string           Gender { get; set; }
            public string           Email { get; set; }
            public string           PhoneNumber { get; set; }
            public string           CountryName { get; set; }
            public string[]         AddressLines { get; set; }
            public string           ZipCode { get; set; }
            public bool             AccountAlertEmail { get; set; }
            public bool             AccountAlertSms { get; set; }
            public bool             MarketingAlertEmail { get; set; }
            public bool             MarketingAlertSms { get; set; }
            public bool             MarketingAlertPhone { get; set; }
        }
    }
}