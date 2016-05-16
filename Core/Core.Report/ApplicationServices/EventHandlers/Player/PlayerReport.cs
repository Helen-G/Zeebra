using System;
using System.Linq;
using AFT.RegoV2.BoundedContexts.Report;
using AFT.RegoV2.BoundedContexts.Report.Data;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Common.Events;
using AFT.RegoV2.Core.Domain.Player.Events;
using AFT.RegoV2.Core.Player.Data;
using AFT.RegoV2.Core.Player.Events;
using AFT.RegoV2.Shared;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.ApplicationServices.Report.EventHandlers
{
    public class PlayerReportEventHandlers : MarshalByRefObject
    {
        private readonly IUnityContainer _container;
        private const string PlayerRecordWasNotFoundMessage = "Player record with Id '{0}' was not found";

        public PlayerReportEventHandlers(IUnityContainer container)
        {
            _container = container;
        }

        public void Handle(PlayerRegistered registeredEvent)
        {
            var playerRegisteredEvent = registeredEvent;
            var repository = _container.Resolve<IReportRepository>();
            var record = repository.PlayerRecords.SingleOrDefault(r => r.PlayerId == playerRegisteredEvent.PlayerId);
            if (record != null)
                throw new RegoException(string.Format("Player record with Id '{0}' already exists", playerRegisteredEvent.PlayerId));

            var brand = _container.Resolve<BrandQueries>().GetBrandOrNull(playerRegisteredEvent.BrandId);
            var streetAddress = String.Join("\n", playerRegisteredEvent.AddressLines).Trim('\n');
            record = new PlayerRecord
            {
                PlayerId = registeredEvent.PlayerId,
                Username = registeredEvent.UserName,
                Brand = brand != null ? brand.Name : null,
                Licensee = brand != null && brand.Licensee != null ? brand.Licensee.Name : null,
                IsInternalAccount = false, // TODO: implement HousePlayer when it's passing in PlayerRegistered event,
                RegistrationDate = playerRegisteredEvent.DateRegistered,
                SignUpIP = playerRegisteredEvent.IPAddress ?? "127.0.0.1",
                PlayerName = playerRegisteredEvent.DisplayName,
                Birthday = playerRegisteredEvent.DateOfBirth,
                Title = playerRegisteredEvent.Title,
                Gender = playerRegisteredEvent.Gender,
                Email = playerRegisteredEvent.Email,
                Mobile = playerRegisteredEvent.PhoneNumber,
                Language = playerRegisteredEvent.Language,
                Currency = playerRegisteredEvent.CurrencyCode,
                Country = playerRegisteredEvent.CountryName,
                PlayerStatus = playerRegisteredEvent.Status,
                VipLevel = playerRegisteredEvent.VipLevel,
                //[KB] TODO: Taking only first 100 characters so that EF will not throw exception during SaveChanges.
                StreetAddress = streetAddress.Length > 100 ? streetAddress.Substring(0, 100): streetAddress,
                PostCode = playerRegisteredEvent.ZipCode,
                Created = registeredEvent.EventCreated,
                CreatedBy = registeredEvent.EventCreatedBy
            };
            if (playerRegisteredEvent.Status == AccountStatus.Active.ToString())
            {
                record.Activated = registeredEvent.EventCreated;
                record.ActivatedBy = registeredEvent.EventCreatedBy;
            }
            repository.PlayerRecords.Add(record);
            repository.SaveChanges();
        }

        public void Handle(PlayerUpdated updatedEvent)
        {
            var player = updatedEvent.Player;
            var repository = _container.Resolve<IReportRepository>();
            var record = repository.PlayerRecords.SingleOrDefault(r => r.PlayerId == player.Id);
            if (record == null)
                throw new RegoException(string.Format(PlayerRecordWasNotFoundMessage, player.Id));

            record.PlayerName = player.DisplayName;
            record.Birthday = player.DateOfBirth;
            record.Title = player.Title;
            record.Gender = player.Gender;
            record.Email = player.Email;
            record.Mobile = player.PhoneNumber;
            record.Country = player.CountryName;
            record.StreetAddress = string.Join("\n", player.AddressLines).Trim('\n');
            record.PostCode = player.ZipCode;
            record.VipLevel = player.VipLevel;
            record.Updated = updatedEvent.EventCreated;
            record.UpdatedBy = updatedEvent.EventCreatedBy;
            repository.SaveChanges();
        }

        public void Handle(PlayerStatusChanged statusChangedEvent)
        {
            var repository = _container.Resolve<IReportRepository>();
            var record = repository.PlayerRecords.SingleOrDefault(r => r.PlayerId == statusChangedEvent.PlayerId);
            if (record == null)
                throw new RegoException(string.Format(PlayerRecordWasNotFoundMessage, statusChangedEvent.PlayerId));

            record.PlayerStatus = statusChangedEvent.AccountStatus.ToString();
            switch (statusChangedEvent.AccountStatus)
            {
                case AccountStatus.Active:
                    record.Activated = statusChangedEvent.EventCreated;
                    record.ActivatedBy = statusChangedEvent.EventCreatedBy;
                    break;
                case AccountStatus.Inactive:
                    record.Deactivated = statusChangedEvent.EventCreated;
                    record.DeactivatedBy = statusChangedEvent.EventCreatedBy;
                    break;
            }
            repository.SaveChanges();
        }
    }
}
