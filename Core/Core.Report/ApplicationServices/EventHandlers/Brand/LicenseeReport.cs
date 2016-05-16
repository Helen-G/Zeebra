using System;
using System.Linq;
using AFT.RegoV2.Core.Brand.Data;
using AFT.RegoV2.BoundedContexts.Report;
using AFT.RegoV2.Core.Brand.Events;
using AFT.RegoV2.BoundedContexts.Report.Data;
using AFT.RegoV2.Shared;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.ApplicationServices.Report.EventHandlers
{
    public class LicenseeReportEventHandlers : MarshalByRefObject
    {
        private readonly IUnityContainer _container;
        private const string LicenseeRecordWasNotFoundMessage = "Licensee record with id '{0}' was not found";

        public LicenseeReportEventHandlers(IUnityContainer container)
        {
            _container = container;
        }

        public void Handle(LicenseeCreated createdEvent)
        {
            var repository = _container.Resolve<IReportRepository>();
            var record = repository.LicenseeRecords.SingleOrDefault(r => r.LicenseeId == createdEvent.Id);
            if (record != null)
            {
                return;
            }
            record = new LicenseeRecord
            {
                LicenseeId = createdEvent.Id,
                Name = createdEvent.Name,
                CompanyName = createdEvent.CompanyName,
                EmailAddress = createdEvent.Email,
                AffiliateSystem = createdEvent.AffiliateSystem,
                Status = LicenseeStatus.Inactive.ToString(),
                ContractStart = createdEvent.ContractStart,
                ContractEnd = createdEvent.ContractEnd,
                Created = createdEvent.DateCreated,
                CreatedBy = createdEvent.CreatedBy
            };
            repository.LicenseeRecords.Add(record);
            repository.SaveChanges();
        }

        public void Handle(LicenseeUpdated updatedEvent)
        {
            var repository = _container.Resolve<IReportRepository>();
            var record = repository.LicenseeRecords.SingleOrDefault(r => r.LicenseeId == updatedEvent.Id);
            if (record == null)
                throw new RegoException(string.Format(LicenseeRecordWasNotFoundMessage, updatedEvent.Id));

            record.Name = updatedEvent.Name;
            record.CompanyName = updatedEvent.CompanyName;
            record.EmailAddress = updatedEvent.Email;
            record.AffiliateSystem = updatedEvent.AffiliateSystem;
            record.ContractStart = updatedEvent.ContractStart;
            record.ContractEnd = updatedEvent.ContractEnd;
            record.Updated = updatedEvent.DateUpdated;
            record.UpdatedBy = updatedEvent.UpdatedBy;
            repository.SaveChanges();
        }

        public void Handle(LicenseeActivated activatedEvent)
        {
            var repository = _container.Resolve<IReportRepository>();
            var record = repository.LicenseeRecords.SingleOrDefault(r => r.LicenseeId == activatedEvent.Id);
            if (record == null)
                throw new RegoException(string.Format(LicenseeRecordWasNotFoundMessage, activatedEvent.Id));

            record.Status = LicenseeStatus.Active.ToString();
            record.Activated = activatedEvent.DateActivated;
            record.ActivatedBy = activatedEvent.ActivatedBy;
            repository.SaveChanges();
        }

        public void Handle(LicenseeDeactivated deactivatedEvent)
        {
            var repository = _container.Resolve<IReportRepository>();
            var record = repository.LicenseeRecords.SingleOrDefault(r => r.LicenseeId == deactivatedEvent.Id);
            if (record == null)
                throw new RegoException(string.Format(LicenseeRecordWasNotFoundMessage, deactivatedEvent.Id));

            record.Status = LicenseeStatus.Deactivated.ToString();
            record.Deactivated = deactivatedEvent.DateDeactivated;
            record.DeactivatedBy = deactivatedEvent.DeactivatedBy;
            repository.SaveChanges();
        }
    }
}
