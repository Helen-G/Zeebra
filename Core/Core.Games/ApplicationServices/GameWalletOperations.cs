using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Game.Data;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.Core.Security.Data;
using AFT.RegoV2.Shared;

namespace AFT.RegoV2.Core.Game.ApplicationServices
{
    public class GameWalletOperations : IGameWalletOperations
    {
        private readonly IGameRepository _repository;
        private readonly IEventBus _eventBus;

        public GameWalletOperations(IEventBus eventBus, IGameRepository repository)
        {
            _repository = repository;
            _eventBus = eventBus;
        }

        public Guid PlaceBet(Guid playerId, Guid gameId, Guid roundId, decimal amount)
        {
            var walletTemplateId = GetWalletTemplateId(playerId, gameId);
            var wallet = _repository.GetWalletWithUPDLock(playerId, walletTemplateId);
            
            var transaction = wallet.PlaceBet(amount, roundId, gameId);
            wallet.Events.ForEach(ev => _eventBus.Publish(ev));

            return transaction.Id;
        }

        public Guid CancelBet(Guid playerId, Guid gameId, Guid transactionId)
        {
            var walletTemplateId = GetWalletTemplateId(playerId, gameId);
            var wallet = _repository.GetWalletWithUPDLock(playerId, walletTemplateId);

            var transaction = wallet.CancelBet(transactionId);
            wallet.Events.ForEach(ev => _eventBus.Publish(ev));

            return transaction.Id;
        }

        public Guid WinBet(Guid playerId, Guid gameId, Guid roundId, decimal amount)
        {
            var walletTemplateId = GetWalletTemplateId(playerId, gameId);
            var wallet = _repository.GetWalletWithUPDLock(playerId, walletTemplateId);

            var transaction = wallet.WinBet(roundId, amount);
            wallet.Events.ForEach(ev => _eventBus.Publish(ev));

            return transaction.Id;
        }

        public async Task<Guid> WinBetAsync(Guid playerId, Guid gameId, Guid roundId, decimal amount)
        {
            var walletTemplateId = GetWalletTemplateId(playerId, gameId);
            var wallet = await _repository.GetWalletWithUPDLockAsync(playerId, walletTemplateId);

            var transaction = wallet.WinBet(roundId, amount);
            wallet.Events.ForEach(ev => _eventBus.Publish(ev));

            return transaction.Id;
        }

        public Guid LoseBet(Guid playerId, Guid gameId, Guid roundId)
        {
            var walletTemplateId = GetWalletTemplateId(playerId, gameId);
            var wallet = _repository.GetWalletWithUPDLock(playerId, walletTemplateId);

            var transaction = wallet.LoseBet(roundId);
            wallet.Events.ForEach(ev => _eventBus.Publish(ev));

            return transaction.Id;
        }

        public Guid FreeBet(Guid playerId, Guid gameId, decimal amount)
        {
            var walletTemplateId = GetWalletTemplateId(playerId, gameId);
            var wallet = _repository.GetWalletWithUPDLock(playerId, walletTemplateId);
            var transaction = wallet.Deposit(amount, "FreeBet");
            wallet.Events.ForEach(ev => _eventBus.Publish(ev));

            return transaction.Id;
        }

        public Guid AdjustBetTransaction(Guid playerId, Guid gameId, Guid transactionId, decimal newAmount)
        {
            //TODO: Adjust bet call is not implemented in the wallet.
            return Guid.NewGuid();
        }

        private Guid GetWalletTemplateId(Guid playerId, Guid gameId)
        {
            var brandId = _repository.Players.Single(x => x.Id == playerId).BrandId;
            var gameProviderId = _repository.Games.Single(x => x.Id == gameId).GameProviderId;


            var walletTemplate = _repository.WalletTemplates.Single(
                x => x.BrandId == brandId && x.WalletTemplateGameProviders.Any(y => y.GameProviderId == gameProviderId));

            return walletTemplate.Id;
        }
    }
}
