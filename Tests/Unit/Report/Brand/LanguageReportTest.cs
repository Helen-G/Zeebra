using System;
using System.Linq;
using System.Text;
using AFT.RegoV2.AdminWebsite.Controllers;
using AFT.RegoV2.ApplicationServices.Report;
using AFT.RegoV2.Core.Brand.Data;
using AFT.RegoV2.BoundedContexts.Report;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Brand.Events;
using AFT.RegoV2.Core.Common.Brand.Data;
using AFT.RegoV2.Core.Common.Brand.Events;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Report.Data.Brand;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.WinService.Workers;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Report.Brand
{
    internal class LanguageReportTest : ReportsTestsBase
    {
        private IReportRepository _reportRepository;
        private ReportQueries _reportQueries;
        private BrandCommands _brandCommands;
        private IServiceBus _serviceBus;

        public override void BeforeEach()
        {
            base.BeforeEach();
            _reportRepository = Container.Resolve<IReportRepository>();
            _reportQueries = Container.Resolve<ReportQueries>();
            _brandCommands = Container.Resolve<BrandCommands>();
            _serviceBus = Container.Resolve<IServiceBus>();
        }

        protected override void StartWorkers()
        {
            Container.Resolve<LanguageReportWorker>().Start();
        }

        [Test]
        public void Can_process_Language_created()
        {
            // Act
            var language = BrandTestHelper.CreateCulture("en-TS", "English (Test)");

            // Assert
            Assert.AreEqual(2, _reportRepository.LanguageRecords.Count());
            var record = _reportRepository.LanguageRecords.Last();
            Assert.AreEqual(language.Code, record.Code);
            Assert.AreEqual(language.Name, record.Name);
            Assert.AreEqual(language.NativeName, record.NativeName);
            Assert.AreEqual(language.DateCreated, record.Created);
            Assert.AreEqual(language.CreatedBy, record.CreatedBy);
        }

        [Test]
        public void Can_process_Language_updated()
        {
            // Arrange
            var language = BrandTestHelper.CreateCulture("en-TS", "English (Test)");
            language.Name = "Changed Name";
            language.NativeName = "Changed Native Name";
            language.UpdatedBy = "TestUser";
            language.DateUpdated = DateTimeOffset.UtcNow;

            // Act
            _serviceBus.PublishMessage(new LanguageUpdated(language));

            // Assert
            Assert.AreEqual(2, _reportRepository.LanguageRecords.Count());
            var record = _reportRepository.LanguageRecords.Last();
            Assert.AreEqual(language.Code, record.Code);
            Assert.AreEqual(language.Name, record.Name);
            Assert.AreEqual(language.NativeName, record.NativeName);
            Assert.AreEqual(language.DateUpdated, record.Updated);
            Assert.AreEqual(language.UpdatedBy, record.UpdatedBy);
        }

        [Test]
        public void Can_process_Language_activated()
        {
            // Arrange
            var language = BrandTestHelper.CreateCulture("en-TS", "English (Test)");
            if (language.Status == CultureStatus.Active)
            {
                _brandCommands.DeactivateCulture(language.Code, "remark");
            }

            // Act
            _brandCommands.ActivateCulture(language.Code, "remark");

            // Assert
            Assert.AreEqual(2, _reportRepository.LanguageRecords.Count());
            var record = _reportRepository.LanguageRecords.Last();
            Assert.AreEqual(language.Code, record.Code);
            Assert.AreEqual(language.Status.ToString(), record.Status);
            Assert.AreEqual(language.DateActivated, record.Activated);
            Assert.AreEqual(language.ActivatedBy, record.ActivatedBy);
        }

        [Test]
        public void Can_process_Language_deactivated()
        {
            // Arrange
            var language = BrandTestHelper.CreateCulture("en-TS", "English (Test)");
            if (language.Status == CultureStatus.Inactive)
            {
                _brandCommands.ActivateCulture(language.Code, "remark");
            }

            // Act
            _brandCommands.DeactivateCulture(language.Code, "remark");

            // Assert
            Assert.AreEqual(2, _reportRepository.LanguageRecords.Count());
            var record = _reportRepository.LanguageRecords.Last();
            Assert.AreEqual(language.Code, record.Code);
            Assert.AreEqual(language.Status.ToString(), record.Status);
            Assert.AreEqual(language.DateDeactivated, record.Deactivated);
            Assert.AreEqual(language.DeactivatedBy, record.DeactivatedBy);
        }

        [Test]
        public void Can_export_report_data()
        {
            // Arrange
            BrandTestHelper.CreateCulture("en-TS", "English (Test)");

            var filteredRecords = ReportController.FilterAndOrder(
                _reportQueries.GetLanguageRecordsForExport(),
                new LanguageRecord(), 
                "Created", "asc");

            // Act
            var content = Encoding.Unicode.GetString(ReportController.ExportToExcel(filteredRecords));

            // Assert
            Assert.AreNotEqual(content.IndexOf("<table"), -1);
        }
    }
}