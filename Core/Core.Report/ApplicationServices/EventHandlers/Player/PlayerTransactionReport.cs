using System;
using System.Linq;
using AFT.RegoV2.ApplicationServices.Player;
using AFT.RegoV2.BoundedContexts.Report;
using AFT.RegoV2.BoundedContexts.Report.Data;
using AFT.RegoV2.Core.Common.Data.Wallet;
using AFT.RegoV2.Core.Common.Events.Wallet;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Game.ApplicationServices;
using AFT.RegoV2.Core.Game.Events;
using AFT.RegoV2.Core.Security.ApplicationServices;
using AFT.RegoV2.Shared;

namespace AFT.RegoV2.ApplicationServices.Report.EventHandlers
{
    public class PlayerTransactionReportEventHandlers : MarshalByRefObject
    {
        private readonly Func<BrandQueries> _brandQueriesFactory;
        private readonly Func<PlayerQueries> _playerQueriesFactory;
        private readonly Func<UserService> _userServiceFactory;
        private readonly Func<GameQueries> _gameQueriesFactory;
        private readonly Func<IReportRepository> _reportRepositoryFactory;
        
        public PlayerTransactionReportEventHandlers(
            Func<BrandQueries> brandQueriesFactory,
            Func<PlayerQueries> playerQueriesFactory,
            Func<UserService> userServiceFactory,
            Func<GameQueries> gameQueriesFactory,
            Func<IReportRepository> reportRepositoryFactory)
        {
            _brandQueriesFactory = brandQueriesFactory;
            _playerQueriesFactory = playerQueriesFactory;
            _userServiceFactory = userServiceFactory;
            _gameQueriesFactory = gameQueriesFactory;
            _reportRepositoryFactory = reportRepositoryFactory;
        }
        
        public void Handle(TransactionProcessed processedEvent)
        {
            var reportRepository = _reportRepositoryFactory();
            var record = reportRepository.PlayerTransactionRecords.SingleOrDefault(r => r.TransactionId == processedEvent.TransactionId);
            
            if (record != null)
                throw new RegoException(string.Format("Player transaction record {0} already exists", processedEvent.TransactionId));

            var player = _playerQueriesFactory().GetPlayer(processedEvent.Wallet.PlayerId);
            
            record = new PlayerTransactionRecord
            {
                TransactionId = processedEvent.TransactionId,
                PlayerId = processedEvent.Wallet.PlayerId,
                CreatedOn = processedEvent.CreatedOn,
                Wallet = _brandQueriesFactory().GetWalletTemplate(processedEvent.Wallet.WalletTemplateId).Name,
                //todo: until DomainEventBase changed
                PerformedBy = GetNamePerformedBy(processedEvent.PerformedBy),
                RoundId = processedEvent.RoundId,
                GameId = processedEvent.GameId,
                Type = Enum.GetName(typeof(TransactionType), processedEvent.PaymentType),
                MainBalanceAmount = processedEvent.MainBalanceAmount,
                MainBalance = processedEvent.Wallet.Main,
                BonusBalanceAmount = processedEvent.BonusBalanceAmount,
                BonusBalance = processedEvent.Wallet.Bonus,
                TemporaryBalanceAmount = processedEvent.TemporaryBalanceAmount,
                TemporaryBalance = processedEvent.Wallet.Temporary,
                LockBonus = processedEvent.Wallet.BonusLock,
                LockFraud = processedEvent.Wallet.FraudLock,
                LockWithdrawal = processedEvent.Wallet.WithdrawalLock,
                //todo: while currency not added to wallet
                //CurrencyCode = processedEvent.Wallet.CurrencyCode,
                CurrencyCode = player.CurrencyCode,
                Description = GetDescription(processedEvent, player.CurrencyCode),
                TransactionNumber = processedEvent.TransactionNumber,
                RelatedTransactionId = processedEvent.RelatedTransactionId
            };

            reportRepository.PlayerTransactionRecords.Add(record);
            reportRepository.SaveChanges();
        }

        public void Handle(LockApplied processedEvent)
        {
            var reportRepository = _reportRepositoryFactory();
            var record = reportRepository.PlayerTransactionRecords.SingleOrDefault(r => r.TransactionId == processedEvent.LockId);

            if (record != null)
                throw new RegoException(string.Format("Player transaction record {0} already exists", processedEvent.LockId));

            var player = _playerQueriesFactory().GetPlayer(processedEvent.Wallet.PlayerId);

            record = new PlayerTransactionRecord
            {
                TransactionId = processedEvent.LockId,
                PlayerId = processedEvent.Wallet.PlayerId,
                CreatedOn = processedEvent.EventCreated,
                Wallet = _brandQueriesFactory().GetWalletTemplate(processedEvent.Wallet.WalletTemplateId).Name,
                //todo: until DomainEventBase changed
                PerformedBy = GetNamePerformedBy(processedEvent.PerformedBy),
                Type = Enum.GetName(typeof(LockType), processedEvent.LockType),
                IsInternal = true,
                MainBalance = processedEvent.Wallet.Main,
                BonusBalance = processedEvent.Wallet.Bonus,
                LockBonus = processedEvent.Wallet.BonusLock,
                LockFraud = processedEvent.Wallet.FraudLock,
                LockWithdrawal = processedEvent.Wallet.WithdrawalLock,
                //todo: while currency not added to wallet
                //CurrencyCode = processedEvent.Wallet.CurrencyCode,
                CurrencyCode = player.CurrencyCode,
                Description = processedEvent.Description,
            };

            reportRepository.PlayerTransactionRecords.Add(record);
            reportRepository.SaveChanges();
        }

        private string GetNamePerformedBy(Guid? id)
        {
            //todo: until DomainEventBase changed
            if (!id.HasValue)
                return "System";

            var player = _playerQueriesFactory().GetPlayer(id.Value);
            if (player != null)
                return "Player";

            var user = _userServiceFactory().GetUserById(id.Value);
            if (user != null)
                return "Admin: " + user.Username;

            return id.ToString();
        }

        private string GetGameNameOrTransactionNumber(TransactionProcessed processedEvent)
        {
            if (processedEvent.GameId.HasValue)
            {
                var gameName =
                    _gameQueriesFactory().GetProductName(processedEvent.GameId.Value);

                return gameName ?? processedEvent.GameId.ToString();
            }

            return processedEvent.TransactionNumber;
        }

        private string GetDescription(TransactionProcessed processedEvent, string currencyCode)
        {
            //todo: for the future use and format Descrition field
            var description = processedEvent.Description + currencyCode + ", " + GetGameNameOrTransactionNumber(processedEvent);
            
            return description;
        }
    }
}
