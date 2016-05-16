using System;
using System.Linq;
using System.Text;
using AFT.RegoV2.AdminWebsite.Controllers;
using AFT.RegoV2.ApplicationServices.Report;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Brand.Data;
using AFT.RegoV2.BoundedContexts.Report;
using AFT.RegoV2.BoundedContexts.Report.Data;
using AFT.RegoV2.Core.Brand.Events;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.TestDoubles;
using AFT.RegoV2.WinService.Workers;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Report.Brand
{
    internal class LicenseeReportTest : ReportsTestsBase
    {
        private IReportRepository _reportRepository;
        private FakeServiceBus _serviceBus;
        private ReportQueries _reportQueries;
        private LicenseeCommands _licenseeCommands;

        public override void BeforeEach()
        {
            base.BeforeEach();
            _reportRepository = Container.Resolve<IReportRepository>();
            _serviceBus = Container.Resolve<FakeServiceBus>();
            _reportQueries = Container.Resolve<ReportQueries>();
            _licenseeCommands = Container.Resolve<LicenseeCommands>();
        }

        protected override void StartWorkers()
        {
            Container.Resolve<LicenseeReportWorker>().Start();
        }

        [Test]
        public void Can_process_licensee_created()
        {
            // Act
            var licensee = BrandTestHelper.CreateLicensee(false);

            // Assert
            Assert.AreEqual(2, _reportRepository.LicenseeRecords.Count());
            var record = _reportRepository.LicenseeRecords.Last();
            Assert.AreEqual(licensee.Id, record.LicenseeId);
            Assert.AreEqual(licensee.Name, record.Name);
            Assert.AreEqual(licensee.Email, record.EmailAddress);
            Assert.AreEqual(licensee.CompanyName, record.CompanyName);
            Assert.AreEqual(licensee.AffiliateSystem, record.AffiliateSystem);
            Assert.AreEqual(licensee.Status.ToString(), record.Status);
            Assert.AreEqual(licensee.ContractStart, record.ContractStart);
            Assert.AreEqual(licensee.ContractEnd, record.ContractEnd);
            Assert.AreEqual(Enum.GetName(typeof(LicenseeStatus), licensee.Status), record.Status);
            Assert.AreEqual(licensee.DateCreated, record.Created);
            Assert.AreEqual(licensee.CreatedBy, record.CreatedBy);
        }

        [Test]
        public void Can_process_licensee_updated()
        {
            // Arrange
            var licensee = BrandTestHelper.CreateLicensee(false);
            var newName = TestDataGenerator.GetRandomString(5);
            var newCompanyName = newName + " Inc.";
            var newEmail = TestDataGenerator.GetRandomEmail();
            var newAffiliateSystem = TestDataGenerator.GetRandomNumber(2) > 1;
            var newContractStart = DateTimeOffset.UtcNow.Date.AddDays(-TestDataGenerator.GetRandomNumber(30));
            var newContractEnd = newContractStart.AddMonths(3);
            var updated = DateTimeOffset.UtcNow;
            const string updatedBy = "me";

            // Act
            _serviceBus.PublishMessage(new LicenseeUpdated(new Licensee
            {
                Id = licensee.Id,
                Name = newName,
                CompanyName = newCompanyName,
                Email = newEmail,
                AffiliateSystem = newAffiliateSystem,
                ContractStart = newContractStart,
                ContractEnd = newContractEnd,
                DateUpdated = updated,
                UpdatedBy = updatedBy
            }));

            // Assert
            Assert.AreEqual(2, _reportRepository.LicenseeRecords.Count());
            var record = _reportRepository.LicenseeRecords.Last();
            Assert.AreEqual(licensee.Id, record.LicenseeId);
            Assert.AreEqual(newName, record.Name);
            Assert.AreEqual(newCompanyName, record.CompanyName);
            Assert.AreEqual(newEmail, record.EmailAddress);
            Assert.AreEqual(newAffiliateSystem, record.AffiliateSystem);
            Assert.AreEqual(newContractStart.Date, record.ContractStart.Date);
            Assert.AreEqual(newContractEnd.Date, record.ContractEnd.Value.Date);
            Assert.AreEqual(licensee.Status.ToString(), record.Status);
            Assert.AreEqual(updated, record.Updated);
            Assert.AreEqual(updatedBy, record.UpdatedBy);
        }

        [Test]
        public void Can_process_licensee_activated()
        {
            // Arrange
            var licensee = BrandTestHelper.CreateLicensee(false);

            // Act
            _licenseeCommands.Activate(licensee.Id, "test");

            // Assert
            Assert.AreEqual(2, _reportRepository.LicenseeRecords.Count());
            var record = _reportRepository.LicenseeRecords.Last();
            Assert.AreEqual(licensee.Id, record.LicenseeId);
            Assert.AreEqual(licensee.Status.ToString(), record.Status);
            Assert.AreEqual(licensee.DateActivated, record.Activated);
            Assert.AreEqual(licensee.ActivatedBy, record.ActivatedBy);
        }

        [Test]
        public void Can_process_licensee_deactivated()
        {
            // Arrange
            var licensee = BrandTestHelper.CreateLicensee();

            // Act
            _licenseeCommands.Deactivate(licensee.Id, "test");

            // Assert
            Assert.AreEqual(2, _reportRepository.LicenseeRecords.Count());
            var record = _reportRepository.LicenseeRecords.Last();
            Assert.AreEqual(licensee.Id, record.LicenseeId);
            Assert.AreEqual(licensee.Status.ToString(), record.Status);
            Assert.AreEqual(licensee.DateDeactivated, record.Deactivated);
            Assert.AreEqual(licensee.DeactivatedBy, record.DeactivatedBy);
        }

        [Test]
        public void Can_export_report_data()
        {
            // Arrange
            BrandTestHelper.CreateLicensee();

            var filteredRecords = ReportController.FilterAndOrder(
                _reportQueries.GetLicenseeRecordsForExport(),
                new LicenseeRecord(),
                "Created", "asc");

            // Act
            var content = Encoding.Unicode.GetString(ReportController.ExportToExcel(filteredRecords));

            // Assert
            Assert.AreNotEqual(content.IndexOf("<table"), -1);
        }
    }
}