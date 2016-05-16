using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.BoundedContexts.Report;
using AFT.RegoV2.Core.Common.Brand.Data;
using AFT.RegoV2.Core.Common.Brand.Events;
using AFT.RegoV2.Core.Report.Data.Brand;
using AFT.RegoV2.Core.Brand.Events;
using AFT.RegoV2.Core.Common.Events.Brand;
using AFT.RegoV2.Shared;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.ApplicationServices.Report.EventHandlers
{
    public class LanguageReportEventHandlers : MarshalByRefObject
    {
        private readonly IUnityContainer _container;
        private const string LanguageRecordAlreadyExistsMessage = "Language record with code '{0}' already exists";
        private const string LanguageRecordNotFoundMessage = "Language record with code '{0}' was not found";
        private const string LanguageRecordsWereNotFoundMessage = "Language records were not found";
        private const string LanguageRecordsCountIsIncorrectMessage = "Language records count is incorrect";

        public LanguageReportEventHandlers(IUnityContainer container)
        {
            _container = container;
        }

        public void Handle(LanguageCreated createdEvent)
        {
            var repository = _container.Resolve<IReportRepository>();
            var record = repository.LanguageRecords.SingleOrDefault(r => r.Code == createdEvent.Code);
            if (record != null)
                return; //it may be the case that language record was already created by other report

            record = new LanguageRecord
            {
                Id = Guid.NewGuid(),
                Code = createdEvent.Code,
                Name = createdEvent.Name,
                NativeName = createdEvent.NativeName,
                Status = createdEvent.Status.ToString(),
                Created = createdEvent.DateCreated,
                CreatedBy = createdEvent.CreatedBy
            };
            if (createdEvent.Status == CultureStatus.Active)
            {
                record.Activated = createdEvent.DateCreated;
                record.ActivatedBy = createdEvent.CreatedBy;
            }
            repository.LanguageRecords.Add(record);
            repository.SaveChanges();
        }

        public void Handle(LanguageUpdated updatedEvent)
        {
            var repository = _container.Resolve<IReportRepository>();
            var records = repository.LanguageRecords.Where(r => r.Code == updatedEvent.Code);
            if (!records.Any())
                throw new RegoException(string.Format(LanguageRecordNotFoundMessage, updatedEvent.Code));

            records.ForEach(record =>
            {
                record.Code = updatedEvent.Code;
                record.Name = updatedEvent.Name;
                record.NativeName = updatedEvent.NativeName;
                record.Updated = updatedEvent.DateUpdated;
                record.UpdatedBy = updatedEvent.UpdatedBy;
            });
            repository.SaveChanges();
        }

        public void Handle(LanguageStatusChanged statusChangedEvent)
        {
            var repository = _container.Resolve<IReportRepository>();
            var record = repository.LanguageRecords.SingleOrDefault(r => r.Code == statusChangedEvent.Code);
            if (record == null)
                throw new RegoException(string.Format(LanguageRecordNotFoundMessage, statusChangedEvent.Code));

            record.Status = statusChangedEvent.Status.ToString();
            switch (statusChangedEvent.Status)
            {
                case CultureStatus.Active:
                    record.Activated = statusChangedEvent.DateStatusChanged;
                    record.ActivatedBy = statusChangedEvent.StatusChangedBy;
                    break;
                case CultureStatus.Inactive:
                    record.Deactivated = statusChangedEvent.DateStatusChanged;
                    record.DeactivatedBy = statusChangedEvent.StatusChangedBy;
                    break;
            }
            repository.SaveChanges();
        }

        public void Handle(BrandLanguagesAssigned brandLanguagesAssignedEvent)
        {
            var repository = _container.Resolve<IReportRepository>();
            if (brandLanguagesAssignedEvent.Languages.Any(l =>
                repository.LanguageRecords.All(lr => lr.Code != l.Code)))
            {
                //todo: create more detailed message
                throw new RegoException(LanguageRecordsWereNotFoundMessage);
            }

            UpdateAssignedInBrand(repository,
                brandLanguagesAssignedEvent.Languages.Select(c => c.Code),
                _container.Resolve<BrandQueries>().GetBrandOrNull(brandLanguagesAssignedEvent.BrandId).Licensee.Name,
                brandLanguagesAssignedEvent.BrandName);
        }

        public void Handle(LicenseeCreated licenseeLanguagesAssignedEvent)
        {
            var repository = _container.Resolve<IReportRepository>();
            if (licenseeLanguagesAssignedEvent.Languages.Any(l =>
                repository.LanguageRecords.All(lr => lr.Code != l)))
            {
                //todo: create more detailed message
                throw new RegoException(LanguageRecordsWereNotFoundMessage);
            }

            UpdateAssignedInLicensee(repository,
                licenseeLanguagesAssignedEvent.Languages,
                licenseeLanguagesAssignedEvent.Name);
        }

        public void Handle(LicenseeUpdated licenseeLanguagesAssignedEvent)
        {
            var repository = _container.Resolve<IReportRepository>();
            if (licenseeLanguagesAssignedEvent.Languages.Any(l =>
                repository.LanguageRecords.All(lr => lr.Code != l)))
            {
                //todo: create more detailed message
                throw new RegoException(LanguageRecordsWereNotFoundMessage);
            }

            UpdateAssignedInLicensee(repository,
                licenseeLanguagesAssignedEvent.Languages,
                licenseeLanguagesAssignedEvent.Name);
        }

        private static void UpdateAssignedInLicensee(IReportRepository repository,
            IEnumerable<string> languages, string licenseeName)
        {
            var licenseeLanguages = languages.ToList();
            var languageRecords = repository.LanguageRecords
                .Where(lr => licenseeLanguages.Contains(lr.Code))
                .GroupBy(lr => lr.Code)
                .Select(lrr => lrr.OrderBy(lr => lr.Licensee == null || lr.Licensee == licenseeName ? 0 : 1).FirstOrDefault())
                .ToList();
            if (languageRecords.Count() != licenseeLanguages.Count())
            {
                //todo: create more detailed message
                throw new RegoException(LanguageRecordsCountIsIncorrectMessage);
            }

            // add new
            languageRecords.Where(lr => lr.Licensee != licenseeName).ForEach(record =>
            {
                if (record.Licensee == null)
                {
                    record.Licensee = licenseeName;
                    return;
                }
                var newRecord = new LanguageRecord
                {
                    Id = Guid.NewGuid(),
                    Licensee = licenseeName
                };
                var detailFields = new[] { "Licensee", "Brand" };
                typeof(LanguageRecord).GetProperties()
                    .Where(pi => pi.Name != "Id" && !detailFields.Contains(pi.Name))
                    .ForEach(pi => pi.SetValue(newRecord, pi.GetValue(record)));
                repository.LanguageRecords.Add(newRecord);
            });

            // remove old
            repository.LanguageRecords
                .Where(lr => !licenseeLanguages.Contains(lr.Code) && lr.Licensee == licenseeName)
                .ToList()
                .ForEach(record =>
                {
                    if (repository.LanguageRecords.Any(lr => lr.Code == record.Code && lr.Id != record.Id))
                    {
                        repository.LanguageRecords.Remove(record);
                    }
                    else
                    {
                        record.Licensee = null;
                    }
                });

            repository.SaveChanges();
        }

        private void UpdateAssignedInBrand(IReportRepository repository,
            IEnumerable<string> languages, string licenseeName, string brandName)
        {
            var brandLanguages = languages.ToList();
            var languageRecords = repository.LanguageRecords
                .Where(lr => brandLanguages.Contains(lr.Code) && lr.Licensee == licenseeName)
                .GroupBy(lr => lr.Code)
                .Select(lrr => lrr.OrderBy(lr => lr.Brand == null || lr.Brand == brandName ? 0 : 1).FirstOrDefault())
                .ToList();
            if (languageRecords.Count() != brandLanguages.Count())
            {
                //todo: create more detailed message
                throw new RegoException(LanguageRecordsCountIsIncorrectMessage);
            }

            // add new
            languageRecords.Where(lr => lr.Brand != brandName).ForEach(record =>
            {
                if (record.Brand == null)
                {
                    record.Brand = brandName;
                    return;
                }
                var newRecord = new LanguageRecord
                {
                    Id = Guid.NewGuid(),
                    Licensee = licenseeName,
                    Brand = brandName
                };
                var detailFields = new[] { "Licensee", "Brand" };
                typeof(LanguageRecord).GetProperties()
                    .Where(pi => pi.Name != "Id" && !detailFields.Contains(pi.Name))
                    .ForEach(pi => pi.SetValue(newRecord, pi.GetValue(record)));
                repository.LanguageRecords.Add(newRecord);
            });

            // remove old
            repository.LanguageRecords
                .Where(lr => !brandLanguages.Contains(lr.Code) && lr.Licensee == licenseeName && lr.Brand == brandName)
                .ToList()
                .ForEach(record =>
                {
                    if (repository.LanguageRecords.Any(lr => lr.Code == record.Code && lr.Licensee == licenseeName && lr.Id != record.Id))
                    {
                        repository.LanguageRecords.Remove(record);
                    }
                    else
                    {
                        record.Brand = null;
                    }
                });

            repository.SaveChanges();
        }

    }
}
