using System;
using System.Linq;
using System.Text;
using AFT.RegoV2.AdminWebsite.Controllers;
using AFT.RegoV2.ApplicationServices.Report;
using AFT.RegoV2.BoundedContexts.Report;
using AFT.RegoV2.BoundedContexts.Report.Data;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Game.Data;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Helpers;
using AFT.RegoV2.WinService.Workers;
using Microsoft.Practices.Unity;
using NUnit.Framework;
using PlayerData = AFT.RegoV2.Core.Player.Data.Player;

namespace AFT.RegoV2.Tests.Unit.Report.Player
{
    internal class PlayerBetHistoryReportTest : ReportsTestsBase
    {
        private IReportRepository _reportRepository;
        private GamesTestHelper _gamesTestHelper;
        private ReportQueries _reportQueries;
        private Random _random;

        private PlayerData _player;
        private Core.Game.Data.Game _game;

        public override void BeforeEach()
        {
            base.BeforeEach();
            _reportRepository = Container.Resolve<IReportRepository>();
            _gamesTestHelper = Container.Resolve<GamesTestHelper>();
            _reportQueries = Container.Resolve<ReportQueries>();
            _random = new Random();

            _player = Container.Resolve<PlayerTestHelper>().CreatePlayer();
            PaymentTestHelper.MakeDeposit(_player.Id, 1000000);

            _game = BrandTestHelper.GetMainWalletGame(_player.Id);
        }

        protected override void StartWorkers()
        {
            Container.Resolve<BonusWorker>().Start();
            Container.Resolve<PlayerBetHistoryReportWorker>().Start();
        }

        [Test]
        public void Can_process_win_bet()
        {
            // Arrange
            var betAmount = RandomBetAmount();

            // Act
            _gamesTestHelper.PlaceAndWinBet(betAmount, betAmount, _player.Id, _game.Id);

            // Assert
            Assert.AreEqual(1, _reportRepository.PlayerBetHistoryRecords.Count());
            var record = _reportRepository.PlayerBetHistoryRecords.Single();
            Assert.AreEqual(betAmount, record.BetAmount);
            Assert.AreEqual(CurrentBrand.Name, record.Brand);
            Assert.Less(DateTimeOffset.Now.AddDays(-2), record.DateBet);
            Assert.AreEqual(_game.Name, record.GameName);
            Assert.AreEqual(_player.Username, record.LoginName);
            Assert.AreEqual(_player.IpAddress ?? LocalIPAddress, record.UserIP);
            Assert.AreEqual(0, record.TotalWinLoss);
        }

        [Test]
        public void Can_process_lose_bet()
        {
            // Arrange
            var betAmount = RandomBetAmount();

            // Act
            _gamesTestHelper.PlaceAndLoseBet(betAmount, _player.Id, _game.Id);

            // Assert
            Assert.AreEqual(1, _reportRepository.PlayerBetHistoryRecords.Count());
            var record = _reportRepository.PlayerBetHistoryRecords.Single();
            Assert.AreEqual(betAmount, record.BetAmount);
            Assert.AreEqual(CurrentBrand.Name, record.Brand);
            Assert.Less(DateTimeOffset.Now.AddDays(-2), record.DateBet);
            Assert.AreEqual(_game.Name, record.GameName);
            Assert.AreEqual(_player.Username, record.LoginName);
            Assert.AreEqual(_player.IpAddress ?? LocalIPAddress, record.UserIP);
            Assert.AreEqual(-betAmount, record.TotalWinLoss);
        }

        [Test]
        public void Can_process_cancelled_bet()
        {
            // Arrange
            var betAmount = RandomBetAmount();

            // Act
            _gamesTestHelper.PlaceAndCancelBet(betAmount, _player.Id, _game.Id);

            // Assert
            Assert.AreEqual(1, _reportRepository.PlayerBetHistoryRecords.Count());
            var record = _reportRepository.PlayerBetHistoryRecords.Single();
            Assert.AreEqual(betAmount, record.BetAmount);
            Assert.AreEqual(CurrentBrand.Name, record.Brand);
            Assert.Less(DateTimeOffset.Now.AddDays(-2), record.DateBet);
            Assert.AreEqual(_game.Name, record.GameName);
            Assert.AreEqual(_player.Username, record.LoginName);
            Assert.AreEqual(_player.IpAddress ?? LocalIPAddress, record.UserIP);
            Assert.AreEqual(0, record.TotalWinLoss);
        }

        [Test]
        public void Can_export_report_data()
        {
            // Arrange
            _gamesTestHelper.PlaceBet(RandomBetAmount(), _player.Id, _game.Id);

            var filteredRecords = ReportController.FilterAndOrder(
                _reportQueries.GetPlayerBetHistoryRecordsForExport(),
                new PlayerBetHistoryRecord(),
                "DateBet", "asc");

            // Act
            var content = Encoding.Unicode.GetString(ReportController.ExportToExcel(filteredRecords));

            // verify data
            Assert.AreNotEqual(content.IndexOf("<table"), -1);
        }

        private int RandomBetAmount()
        {
            return _random.Next(100, 1000);
        }
    }
}