using System;
using System.Linq;
using AFT.RegoV2.Core.Common.Events.Payment;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Player;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Helpers;
using AFT.RegoV2.WinService.Workers;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Player
{
    internal class PlayerActivityLogTest : AdminWebsiteUnitTestsBase
    {
        private IServiceBus _serviceBus;
        private IPlayerRepository _playerRepository;


        public override void BeforeEach()
        {
            base.BeforeEach();
            Container.Resolve<SecurityTestHelper>().SignInUser();
            _serviceBus = Container.Resolve<IServiceBus>();

            _playerRepository = Container.Resolve<IPlayerRepository>();

            Container.Resolve<PlayerActivityLogWorker>().Start();
        }

        [Test]
        public void Can_log_TransferFundCreated()
        {
            // Arrange
            var @event = new TransferFundCreated
            {
                PlayerId = new Guid(),
                Amount = 1,
                TransactionNumber = "TF001",
                Remarks = "remark"
            };

            //Act
            _serviceBus.PublishMessage(@event);

            //Assert
            Assert.AreEqual(1, _playerRepository.PlayerActivityLog.Count());
            var record = _playerRepository.PlayerActivityLog.Single();
            Assert.AreEqual("Player", record.Category);
            Assert.AreEqual(new Guid(), record.PlayerId);
            Assert.AreEqual(string.Empty, record.PerformedBy);
            Assert.AreEqual(@event.EventCreated.Date, record.DatePerformed.Date);
            Assert.AreEqual("Transfer Fund created. Amount: 1. Transaction Number: TF001", record.ActivityDone);
            Assert.AreEqual("remark", record.Remarks);
        }
    }
}
