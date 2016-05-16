using System;
using System.Linq;
using AFT.RegoV2.BoundedContexts.Report;
using AFT.RegoV2.Core.Brand.Events;
using AFT.RegoV2.Core.Common.Events.Brand;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Payment.Events;
using AFT.RegoV2.Core.Player;
using AFT.RegoV2.Core.Report.Data.Admin;
using AFT.RegoV2.Core.Report.Events;
using AFT.RegoV2.Core.Services.Security;
using AFT.RegoV2.Domain.Brand.Events;
using AFT.RegoV2.Domain.Payment.Data;
using AFT.RegoV2.Domain.Security.Events;
using AFT.RegoV2.Domain.Security.Interfaces;
using AFT.RegoV2.Shared;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Helpers;
using AFT.RegoV2.WinService.Workers;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Report.Payment
{
    public class WithdrawalActivityLogTests : AdminWebsiteUnitTestsBase
    {
        private string PerformedBy;
        private IPlayerRepository _playerRepository;
        private IServiceBus _serviceBus;

        public override void BeforeEach()
        {
            base.BeforeEach();
            Container.Resolve<SecurityTestHelper>().SignInUser();
            var licensee = Container.Resolve<BrandTestHelper>().CreateLicensee();
            var brand = Container.Resolve<BrandTestHelper>().CreateBrand(licensee);
            var user = Container.Resolve<SecurityTestHelper>().CreateUser(licensee.Id, new [] {brand}, new [] {"CAD"});
            PerformedBy = user.Username;
            _playerRepository = Container.Resolve<IPlayerRepository>();
            _serviceBus = Container.Resolve<IServiceBus>();

            Container.Resolve<PlayerActivityLogWorker>().Start();
        }

        [Test]
        public void Can_log_withdrawal_wager_checked()
        {
            var @event = new WithdrawalWagerChecked ();
            @event.EventCreatedBy = PerformedBy;
            _serviceBus.PublishMessage(@event);
            AssertAdminActivityLog(@event, AdminActivityLogCategory.Brand, PerformedBy);
        }

        [Test]
        public void Can_log_withdrawal_created()
        {
            var @event = new WithdrawalCreated();
            @event.EventCreatedBy = PerformedBy;
            _serviceBus.PublishMessage(@event);
            AssertAdminActivityLog(@event, AdminActivityLogCategory.Brand, PerformedBy);
        }
        [Test]
        public void Can_log_withdrawal_investigated()
        {
            var @event = new WithdrawalInvestigated();
            @event.EventCreatedBy = PerformedBy;
            _serviceBus.PublishMessage(@event);
            AssertAdminActivityLog(@event, AdminActivityLogCategory.Brand, PerformedBy);
        }

        private void AssertAdminActivityLog(IDomainEvent @event, AdminActivityLogCategory category, string performedBy = "System")
        {
            Assert.AreEqual(1, _playerRepository.PlayerActivityLog.Count());
//            var record = _reportRepository.AdminActivityLog.Single();
//            Assert.AreEqual(category, record.Category);
//            Assert.AreEqual(performedBy, record.PerformedBy);
//            Assert.AreEqual(@event.EventCreated.Date, record.DatePerformed.Date);
//            Assert.AreEqual(@event.GetType().Name.SeparateWords(), record.ActivityDone);
        }
    }
}
