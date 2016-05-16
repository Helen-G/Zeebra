using System;
using System.Linq;
using AFT.RegoV2.BoundedContexts.Report;
using AFT.RegoV2.Core.Brand.Events;
using AFT.RegoV2.Core.Common.Brand.Events;
using AFT.RegoV2.Core.Common.Events.Brand;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Report.Data.Admin;
using AFT.RegoV2.Core.Report.Events;
using AFT.RegoV2.Core.Services.Security;
using AFT.RegoV2.Domain.Brand.Events;
using AFT.RegoV2.Domain.Security.Events;
using AFT.RegoV2.Domain.Security.Interfaces;
using AFT.RegoV2.Shared;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Helpers;
using AFT.RegoV2.WinService.Workers;
using Microsoft.Practices.Unity;
using NUnit.Framework;
using BrandLanguagesAssigned = AFT.RegoV2.Core.Common.Events.Brand.BrandLanguagesAssigned;

namespace AFT.RegoV2.Tests.Unit.AdminActivityLog
{
    internal class AdminActivityLogTest : AdminWebsiteUnitTestsBase
    {
        private const string PerformedBy = "testuser";
        private IReportRepository _reportRepository;
        private IServiceBus _serviceBus;

        public override void BeforeEach()
        {
            base.BeforeEach();
            Container.Resolve<SecurityTestHelper>().SignInUser();
            _reportRepository = Container.Resolve<IReportRepository>();
            _serviceBus = Container.Resolve<IServiceBus>();

            Container.Resolve<AdminActivityLogWorker>().Start();
        }

        [Test]
        public void Can_log_Brand_Registered()
        {
            var @event = new BrandRegistered { CreatedBy = PerformedBy };
            _serviceBus.PublishMessage(@event);
            AssertAdminActivityLog(@event, AdminActivityLogCategory.Brand, PerformedBy);
        }

        [Test]
        public void Can_log_Brand_Activated()
        {
            var @event = new BrandActivated { ActivatedBy = PerformedBy };
            _serviceBus.PublishMessage(@event);
            AssertAdminActivityLog(@event, AdminActivityLogCategory.Brand, PerformedBy);
        }

        [Test]
        public void Can_log_Brand_Deactivated()
        {
            var @event = new BrandDeactivated { DeactivatedBy = PerformedBy };
            _serviceBus.PublishMessage(@event);
            AssertAdminActivityLog(@event, AdminActivityLogCategory.Brand, PerformedBy);
        }

        [Test]
        public void Can_log_Brand_Countries_Assigned()
        {
            var @event = new BrandCountriesAssigned();
            _serviceBus.PublishMessage(@event);
            AssertAdminActivityLog(@event, AdminActivityLogCategory.Brand);
        }

        [Test]
        public void Can_log_Brand_Currencies_Assigned()
        {
            var @event = new BrandCurrenciesAssigned();
            _serviceBus.PublishMessage(@event);
            AssertAdminActivityLog(@event, AdminActivityLogCategory.Brand);
        }

        [Test]
        public void Can_log_Brand_Languages_Assigned()
        {
            var @event = new BrandLanguagesAssigned();
            _serviceBus.PublishMessage(@event);
            AssertAdminActivityLog(@event, AdminActivityLogCategory.Brand);
        }

        [Test]
        public void Can_log_Brand_Products_Assigned()
        {
            var @event = new BrandProductsAssigned();
            _serviceBus.PublishMessage(@event);
            AssertAdminActivityLog(@event, AdminActivityLogCategory.Brand);
        }

        [Test]
        public void Can_log_Licensee_Created()
        {
            var @event = new LicenseeCreated { CreatedBy = PerformedBy };
            _serviceBus.PublishMessage(@event);
            AssertAdminActivityLog(@event, AdminActivityLogCategory.Licensee, PerformedBy);
        }

        [Test]
        public void Can_log_Licensee_Updated()
        {
            var @event = new LicenseeUpdated { UpdatedBy = PerformedBy };
            _serviceBus.PublishMessage(@event);
            AssertAdminActivityLog(@event, AdminActivityLogCategory.Licensee, PerformedBy);
        }

        [Test]
        public void Can_log_Licensee_Activated()
        {
            var @event = new LicenseeActivated { ActivatedBy = PerformedBy };
            _serviceBus.PublishMessage(@event);
            AssertAdminActivityLog(@event, AdminActivityLogCategory.Licensee, PerformedBy);
        }

        [Test]
        public void Can_log_Licensee_Deactivated()
        {
            var @event = new LicenseeDeactivated { DeactivatedBy = PerformedBy };
            _serviceBus.PublishMessage(@event);
            AssertAdminActivityLog(@event, AdminActivityLogCategory.Licensee, PerformedBy);
        }

        [Test]
        public void Can_log_Language_Created()
        {
            var @event = new LanguageCreated { CreatedBy = PerformedBy };
            _serviceBus.PublishMessage(@event);
            AssertAdminActivityLog(@event, AdminActivityLogCategory.Language, PerformedBy);
        }

        [Test]
        public void Can_log_Language_Updated()
        {
            var @event = new LanguageUpdated { UpdatedBy = PerformedBy };
            _serviceBus.PublishMessage(@event);
            AssertAdminActivityLog(@event, AdminActivityLogCategory.Language, PerformedBy);
        }

        [Test]
        public void Can_log_Language_Status_Changed()
        {
            var @event = new LanguageStatusChanged { StatusChangedBy = PerformedBy };
            _serviceBus.PublishMessage(@event);
            AssertAdminActivityLog(@event, AdminActivityLogCategory.Language, PerformedBy);
        }

        [Test]
        public void Can_log_Country_Created()
        {
            var @event = new CountryCreated();
            _serviceBus.PublishMessage(@event);
            AssertAdminActivityLog(@event, AdminActivityLogCategory.Country);
        }

        [Test]
        public void Can_log_Country_Updated()
        {
            var @event = new CountryUpdated();
            _serviceBus.PublishMessage(@event);
            AssertAdminActivityLog(@event, AdminActivityLogCategory.Country);
        }

        [Test]
        public void Can_log_Country_Removed()
        {
            var @event = new CountryRemoved();
            _serviceBus.PublishMessage(@event);
            AssertAdminActivityLog(@event, AdminActivityLogCategory.Country);
        }

        [Test]
        public void Can_log_VIP_Level_Registered()
        {
            var @event = new VipLevelRegistered { CreatedBy = PerformedBy };
            _serviceBus.PublishMessage(@event);
            AssertAdminActivityLog(@event, AdminActivityLogCategory.VipLevel, PerformedBy);
        }

        [Test]
        public void Can_log_VIP_Level_Updated()
        {
            var @event = new VipLevelUpdated { UpdatedBy = PerformedBy };
            _serviceBus.PublishMessage(@event);
            AssertAdminActivityLog(@event, AdminActivityLogCategory.VipLevel, PerformedBy);
        }

        [Test]
        public void Can_log_VIP_Level_Status_Changed()
        {
            var @event = new VipLevelActivated();
            _serviceBus.PublishMessage(@event);
            AssertAdminActivityLog(@event, AdminActivityLogCategory.VipLevel);
        }

        [Test]
        public void Can_log_Admin_IP_Regulation_Created()
        {
            var @event = new AdminIpRegulationCreated();
            _serviceBus.PublishMessage(@event);
            AssertAdminActivityLog(@event, AdminActivityLogCategory.BackendIPRegulation);
        }

        [Test]
        public void Can_log_Admin_IP_Regulation_Updated()
        {
            var @event = new AdminIpRegulationUpdated();
            _serviceBus.PublishMessage(@event);
            AssertAdminActivityLog(@event, AdminActivityLogCategory.BackendIPRegulation);
        }

        [Test]
        public void Can_log_Admin_IP_Regulation_Deleted()
        {
            var @event = new AdminIpRegulationDeleted();
            _serviceBus.PublishMessage(@event);
            AssertAdminActivityLog(@event, AdminActivityLogCategory.BackendIPRegulation);
        }

        [Test]
        public void Can_log_Brand_IP_Regulation_Created()
        {
            var @event = new BrandIpRegulationCreated();
            _serviceBus.PublishMessage(@event);
            AssertAdminActivityLog(@event, AdminActivityLogCategory.PlayerIPRegulation);
        }

        [Test]
        public void Can_log_Brand_IP_Regulation_Updated()
        {
            var @event = new BrandIpRegulationUpdated();
            _serviceBus.PublishMessage(@event);
            AssertAdminActivityLog(@event, AdminActivityLogCategory.PlayerIPRegulation);
        }

        [Test]
        public void Can_log_Brand_IP_Regulation_Deleted()
        {
            var @event = new BrandIpRegulationDeleted();
            _serviceBus.PublishMessage(@event);
            AssertAdminActivityLog(@event, AdminActivityLogCategory.PlayerIPRegulation);
        }

        [Test]
        public void Can_log_Report_Exported()
        {
            var @event = new ReportExported();
            _serviceBus.PublishMessage(@event);
            AssertAdminActivityLog(@event, AdminActivityLogCategory.Report);
        }

        private void AssertAdminActivityLog(IDomainEvent @event, AdminActivityLogCategory category, string performedBy = "System")
        {
            Assert.AreEqual(1, _reportRepository.AdminActivityLog.Count());
            var record = _reportRepository.AdminActivityLog.Single();
            Assert.AreEqual(category, record.Category);
            Assert.AreEqual(performedBy, record.PerformedBy);
            Assert.AreEqual(@event.EventCreated.Date, record.DatePerformed.Date);
            Assert.AreEqual(@event.GetType().Name.SeparateWords(), record.ActivityDone);
        }
    }
}
