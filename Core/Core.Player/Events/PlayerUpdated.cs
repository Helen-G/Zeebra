using System;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Domain.Player.Events
{
    public class PlayerUpdated : DomainEventBase
    {
        public PlayerUpdated() { } // default constructor is required for publishing event to MQ

        public PlayerUpdated(Core.Player.Data.Player player, Guid? paymentLevelId, string countryName)
        {
            Player = new PlayerData
            {
                Id = player.Id,
                PaymentLevelId = paymentLevelId,
                VipLevel = player.VipLevel != null ? player.VipLevel.Name : null,
                VipLevelId = player.VipLevel != null ? player.VipLevel.Id : Guid.Empty,
                DisplayName = player.GetFullName(),
                DateOfBirth = player.DateOfBirth,
                Title = player.Title.ToString(),
                Gender = player.Gender.ToString(),
                Email = player.Email,
                PhoneNumber = player.PhoneNumber,
                CountryName = countryName,
                AddressLines = new[]
                {
                    player.MailingAddressLine1,
                    player.MailingAddressLine2,
                    player.MailingAddressLine3,
                    player.MailingAddressLine4
                },
                ZipCode = player.MailingAddressPostalCode
            };
        }

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
        }
    }
}