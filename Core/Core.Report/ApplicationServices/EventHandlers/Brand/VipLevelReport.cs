using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.BoundedContexts.Report;
using AFT.RegoV2.BoundedContexts.Report.Data;
using AFT.RegoV2.Core.Common.Events.Brand;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.Shared;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.ApplicationServices.Report.EventHandlers
{
    public class VipLevelReportEventHandlers : MarshalByRefObject
    {
        private readonly IUnityContainer _container;

        public VipLevelReportEventHandlers(IUnityContainer container)
        {
            _container = container;
        }

        public void Handle(VipLevelRegistered registeredEvent)
        {
            var repository = _container.Resolve<IReportRepository>();
            var record = repository.VipLevelRecords.FirstOrDefault(r => r.VipLevelId == registeredEvent.Id);
            if (record != null)
            {
                return;
            }
            var brand = _container.Resolve<BrandQueries>().GetBrandOrNull(registeredEvent.BrandId);
            record = new VipLevelRecord
            {
                Id = Guid.NewGuid(),
                VipLevelId = registeredEvent.Id,
                Licensee = brand.Licensee.Name,
                Brand = brand.Name,
                Code = registeredEvent.Code,
                Rank = registeredEvent.Rank,
                Status = registeredEvent.Status.ToString(),
                Created = registeredEvent.DateCreated,
                CreatedBy = registeredEvent.CreatedBy
            };
            if (registeredEvent.Status == VipLevelStatus.Active)
            {
                record.Activated = registeredEvent.DateCreated;
                record.ActivatedBy = registeredEvent.CreatedBy;
            }
            repository.VipLevelRecords.Add(record);
            UpdateReportBetLimits(repository, record, registeredEvent.VipLevelLimits, registeredEvent.BrandId);
            repository.SaveChanges();
        }

        public void Handle(VipLevelUpdated updatedEvent)
        {
            //todo: refactor updatedEvent.Id 
            var repository = _container.Resolve<IReportRepository>();
            var records = repository.VipLevelRecords.Where(r => r.VipLevelId == updatedEvent.Id);
            if (!records.Any())
                throw new RegoException(string.Format("Vip level record with id '{0}' was not found", updatedEvent.Id));

            var brand = _container.Resolve<BrandQueries>().GetBrandOrNull(updatedEvent.BrandId);
            foreach (var record in records)
            {
                record.Licensee = brand.Licensee.Name;
                record.Brand = brand.Name;
                record.Code = updatedEvent.Code;
                record.Rank = updatedEvent.Rank;
                record.Updated = updatedEvent.DateUpdated;
                record.UpdatedBy = updatedEvent.UpdatedBy;
            }
            UpdateReportBetLimits(repository, records.First(), updatedEvent.VipLevelLimits, updatedEvent.BrandId);
            repository.SaveChanges();
        }

        private void UpdateReportBetLimits(IReportRepository repository,
            VipLevelRecord record, ICollection<VipLevelLimitData> vipLevelLimits, Guid brandId)
        {
            var vipLevelRecords = repository.VipLevelRecords.Where(r => r.VipLevelId == record.VipLevelId).ToList();

            var gamesQueries = _container.Resolve<IGameQueries>();
            var gameProviders = gamesQueries.GetGameProviderDtos().ToList();

            // Remove old records
            vipLevelRecords
                .Where(r => vipLevelLimits.All(l => l.Id != r.VipLevelLimitId) && r.Id != record.Id)
                .ToArray()
                .ForEach(r => repository.VipLevelRecords.Remove(r));

            // Update record
            var vipLevelLimit =
                vipLevelLimits.FirstOrDefault(l => l.Id == record.VipLevelLimitId) ??
                vipLevelLimits.FirstOrDefault();
            var vipLevelLimitId = vipLevelLimit != null ? vipLevelLimit.Id : null as Guid?;
            if (record.VipLevelLimitId != vipLevelLimitId)
            {
                record.VipLevelLimitId = vipLevelLimitId;
                if (vipLevelLimit != null)
                {
                    record.GameProvider = gameProviders.Single(s => s.Id == vipLevelLimit.GameProviderId).Name;
                    record.Currency = vipLevelLimit.CurrencyCode;
                    var betLimits = gamesQueries.GetBetLimits(vipLevelLimit.GameProviderId, brandId).ToList();
                    record.BetLevel = betLimits.Single(l => l.Id == vipLevelLimit.BetLimitId).LimitId;
                }
                else
                {
                    record.GameProvider = record.Currency = record.BetLevel = null;
                }
            }

            // add new records
            vipLevelLimits
                .Where(l => l.Id != vipLevelLimit.Id)
                .ForEach(limit =>
                {
                    var betLimits = gamesQueries.GetBetLimits(limit.GameProviderId, brandId).ToList();
                    var newRecord = new VipLevelRecord
                    {
                        Id = Guid.NewGuid(),
                        VipLevelLimitId = limit.Id,
                        GameProvider = gameProviders.Single(s => s.Id == limit.GameProviderId).Name,
                        Currency = limit.CurrencyCode,
                        BetLevel = betLimits.Single(l => l.Id == limit.BetLimitId).LimitId
                    };
                    var vipLevelFields = new[] { "VipLevelLimitId", "GameProvider", "Currency", "BetLevel" };
                    typeof (VipLevelRecord).GetProperties()
                        .Where(pi => pi.Name != "Id" && !vipLevelFields.Contains(pi.Name))
                        .ForEach(pi => pi.SetValue(newRecord, pi.GetValue(record)));
                    repository.VipLevelRecords.Add(newRecord);
                });
        }

        public void Handle(VipLevelActivated @event)
        {
            var repository = _container.Resolve<IReportRepository>();
            var record = GetVipLevelRecord(repository, @event.VipLevelId);
            record.Status = VipLevelStatus.Active.ToString();
            record.Activated = @event.EventCreated;
            record.ActivatedBy = @event.EventCreatedBy;
            repository.SaveChanges();
        }

        public void Handle(VipLevelDeactivated @event)
        {
            var repository = _container.Resolve<IReportRepository>();
            var record = GetVipLevelRecord(repository, @event.VipLevelId);
            record.Status = VipLevelStatus.Inactive.ToString();
            record.Deactivated = @event.EventCreated;
            record.DeactivatedBy = @event.EventCreatedBy;
            repository.SaveChanges();
        }

        VipLevelRecord GetVipLevelRecord(IReportRepository repository, Guid vipLevelId)
        {
            var record = repository.VipLevelRecords.FirstOrDefault(r => r.VipLevelId == vipLevelId);
            if (record == null)
                throw new RegoException(string.Format("Vip level record with id '{0}' was not found", vipLevelId));
            return record;
        }
    }
}
