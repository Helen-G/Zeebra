using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using AFT.RegoV2.Core.Common.Data.Wallet;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Game.Data;
using AFT.RegoV2.Core.Game.Interfaces;
using AutoMapper;

namespace AFT.RegoV2.Core.Game.ApplicationServices
{
    public class WalletQueries : IWalletQueries
    {
        private readonly IGameRepository _repository;

        public WalletQueries(IGameRepository repository)
        {
            _repository = repository;
        }

        public PlayerBalance GetPlayerBalance(Guid playerId, Guid? walletTemplateId = null)
        {
            var wallet = _repository.GetWalletWithUPDLock(playerId, walletTemplateId);

            return Mapper.DynamicMap<PlayerBalance>(wallet.Data);
        }

        public async Task<PlayerBalance> GetPlayerBalanceAsync(Guid playerId, Guid? walletTemplateId = null)
        {
            Wallet wallet;

            if ( walletTemplateId.HasValue)
                wallet = await _repository.Wallets.SingleAsync(x => x.PlayerId == playerId && x.Template.Id == walletTemplateId);
            else
                wallet = await _repository.Wallets.SingleAsync(x => x.PlayerId == playerId && x.Template.IsMain);

            return Mapper.DynamicMap<PlayerBalance>(wallet);
        }

        public decimal GetWageringLockedBalanceOfPlayer(Guid playerId)
        {
            return _repository.Wallets.Single(x => x.PlayerId == playerId && x.Template.IsMain).BonusLock;
        }

        public IQueryable<Wallet> GetProductWalletsOfPlayer(Guid playerId)
        {
            return _repository.Wallets.Where(x => x.PlayerId == playerId);
        }

        public bool PlayerHasWageringRequirement(Guid playerId, Guid? walletStructureId = null)
        {
            return _repository.GetWalletWithUPDLock(playerId, walletStructureId).Data.HasWageringRequirement;
        }
    }
}