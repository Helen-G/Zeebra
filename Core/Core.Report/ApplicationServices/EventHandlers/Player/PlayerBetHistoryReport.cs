using System;
using System.Linq;
using System.Threading;
using AFT.RegoV2.ApplicationServices.Player;
using AFT.RegoV2.BoundedContexts.Report;
using AFT.RegoV2.BoundedContexts.Report.Data;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Common.Events.Games;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.Shared;

namespace AFT.RegoV2.ApplicationServices.Report.EventHandlers
{
    public class PlayerBetHistoryReportEventHandlers : MarshalByRefObject
    {
        private readonly Func<BrandQueries> _brandQueriesFactory;
        private readonly Func<PlayerQueries> _playerQueriesFactory;
        private readonly Func<IGameQueries> _gameQueriesFactory;
        private readonly Func<IReportRepository> _reportRepositoryFactory;

        private const string BetHistoryRecordNotFoundMessage = "PlayerBetHistoryRecord with Id '{0}' was not found";

        public PlayerBetHistoryReportEventHandlers(
            Func<BrandQueries> brandQueriesFactory, 
            Func<PlayerQueries> playerQueriesFactory, 
            Func<IGameQueries> gameQueriesFactory,
            Func<IReportRepository> reportRepositoryFactory)
        {
            _brandQueriesFactory = brandQueriesFactory;
            _playerQueriesFactory = playerQueriesFactory;
            _gameQueriesFactory = gameQueriesFactory;
            _reportRepositoryFactory = reportRepositoryFactory;
        }

        public void Handle(BetPlaced placedEvent)
        {
            var reportRepository = _reportRepositoryFactory();
            var record = reportRepository.PlayerBetHistoryRecords.SingleOrDefault(r => r.GameActionId == placedEvent.GameActionId);
            if (record != null)
                throw new RegoException(string.Format("Player bet history record {0} already exists", placedEvent.RoundId));

            record = reportRepository.PlayerBetHistoryRecords.SingleOrDefault(r => r.RoundId == placedEvent.RoundId);
            if (record != null)
            {
                record.BetAmount += placedEvent.Amount;
                record.TotalWinLoss -= placedEvent.Amount;
                reportRepository.SaveChanges();
                return;
            }

            var brand = _brandQueriesFactory().GetBrandOrNull(placedEvent.BrandId);
            var player = _playerQueriesFactory().GetPlayer(placedEvent.PlayerId);
            var game = _gameQueriesFactory().GetGameDtos().SingleOrDefault(g => g.Id == placedEvent.GameId);
            record = new PlayerBetHistoryRecord
            {
                GameActionId = placedEvent.GameActionId,
                RoundId = placedEvent.RoundId,
                Brand = brand != null ? brand.Name : null,
                Licensee = brand != null && brand.Licensee != null ? brand.Licensee.Name : null,
                Currency = player != null ? player.CurrencyCode : null,
                GameName = game != null ? game.Name : null,
                LoginName = player != null ? player.Username : null,
                UserIP = player != null ? player.IpAddress ?? "127.0.0.1" : null,
                DateBet = placedEvent.CreatedOn,
                BetAmount = placedEvent.Amount,
                TotalWinLoss = -placedEvent.Amount
            };
            reportRepository.PlayerBetHistoryRecords.Add(record);
            reportRepository.SaveChanges();
        }

        public void Handle(BetWon wonEvent)
        {
            var reportRepository = _reportRepositoryFactory();
            var record = GetBetHistoryRecordOrNull(reportRepository, wonEvent.RoundId);
            if (record == null)
                throw new RegoException(string.Format(BetHistoryRecordNotFoundMessage, wonEvent.RoundId));

            record.TotalWinLoss = GetTotalWinLoss(wonEvent);
            reportRepository.SaveChanges();
        }

        public void Handle(BetAdjusted adjustedEvent)
        {
            var reportRepository = _reportRepositoryFactory();
            var record = GetBetHistoryRecordOrNull(reportRepository, adjustedEvent.RoundId);
            if (record == null)
            {
                throw new RegoException(string.Format(BetHistoryRecordNotFoundMessage, adjustedEvent.RoundId));
            }
            record.TotalWinLoss = GetTotalWinLoss(adjustedEvent);
            reportRepository.SaveChanges();
        }

        public void Handle(BetCancelled cancelledEvent)
        {
            var reportRepository = _reportRepositoryFactory();
            var record = GetBetHistoryRecordOrNull(reportRepository, cancelledEvent.RoundId);
            if (record == null)
            {
                throw new RegoException(string.Format(BetHistoryRecordNotFoundMessage, cancelledEvent.RoundId));
            }

            record.TotalWinLoss = GetTotalWinLoss(cancelledEvent);
            reportRepository.SaveChanges();
        }

        PlayerBetHistoryRecord GetBetHistoryRecordOrNull(IReportRepository reportRepository, Guid roundId, int attemts = 3, int intervalMSec = 100)
        {
            //multiple attempts are required to avoid situation when BetPlaced handlers were not committed transaction yet
            for (var i = 0; i < attemts; i++)
            {
                var record = reportRepository.PlayerBetHistoryRecords.SingleOrDefault(r => r.RoundId == roundId);
                if (record != null) 
                    return record;
                Thread.Sleep(intervalMSec);
            }
            return null;
        }

        public void Handle(BetPlacedFree freeBetEvent)
        {
            var reportRepository = _reportRepositoryFactory();
            var record = reportRepository.PlayerBetHistoryRecords.SingleOrDefault(r => r.RoundId == freeBetEvent.RoundId);
            if (record == null)
                throw new RegoException(string.Format(BetHistoryRecordNotFoundMessage, freeBetEvent.RoundId));

            record.TotalWinLoss = GetTotalWinLoss(freeBetEvent);
            reportRepository.SaveChanges();
        }

        private decimal GetTotalWinLoss(GameActionEventBase bet)
        {
            return bet.WonAmount - bet.Amount + bet.AdjustedAmount;
        }
    }
}
