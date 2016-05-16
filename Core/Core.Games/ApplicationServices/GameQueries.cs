using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.UI;
using AFT.RegoV2.ApplicationServices.Security;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Exceptions;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Game.Data;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Shared;
using Round = AFT.RegoV2.Core.Game.Entities.Round;

namespace AFT.RegoV2.Core.Game.ApplicationServices
{
    public sealed class GameQueries : MarshalByRefObject, IGameQueries
    {
        private readonly IGameRepository _gameRepository;
        private readonly IWalletQueries _walletQueries;

        public GameQueries(
            IGameRepository gameRepository,
            IWalletQueries walletQueries)
        {
            _gameRepository = gameRepository;
            _walletQueries = walletQueries;
        }

        decimal IGameQueries.GetPlayableBalance(Guid playerId, Guid gameId)
        {
            var walletTemplateId = GetWalletTemplateId(playerId, gameId);

            return _walletQueries.GetPlayerBalance(playerId, walletTemplateId).Playable;
        }

        List<Round> IGameQueries.GetRoundHistory(TokenData token, int recordCount)
        {
            return _gameRepository
                .GetPlayerRounds(token.PlayerId)
                .Where(round => round.Data.GameId == token.GameId)
                .OrderByDescending(x => x.Data.CreatedOn)
                .Take(recordCount)
                .ToList();
        }

        Round IGameQueries.GetRoundByGameActionId(Guid gameActionId)
        {
            return
                new Round(
                    _gameRepository.Rounds.Single(
                        round => round.Id == _gameRepository.GameActions.FirstOrDefault(tx => tx.Id == gameActionId).Round.Id));
        }

        Player IGameQueries.GetPlayerData(Guid playerId)
        {
            return _gameRepository.Players.Single(player => player.Id == playerId);
        }

        Data.Brand IGameQueries.GetBrand(string brandCode)
        {
            return _gameRepository.Brands.Single(x => x.Code == brandCode);
        }

        Data.Brand IGameQueries.GetBrand(Guid brandId)
        {
            return _gameRepository.Brands.Single(x => x.Id == brandId);
        }

        void IGameQueries.ValidateBatchIsUnique(string batchId, Guid gameProviderId)
        {
            if (_gameRepository.DoesBatchIdExist(batchId, gameProviderId))
            {
                throw new DuplicateBatchException();
            }
        }


        IEnumerable<GameProviderConfiguration> IGameQueries.GetGameProviderConfigurations()
        {
            return _gameRepository.GameProviderConfigurations;
        }

        async Task<GameProviderConfiguration> IGameQueries.GetGameProviderConfigurationAsync(Guid brandId, Guid gameProviderId)
        {
            var bgpc = await
                _gameRepository.BrandGameProviderConfigurations
                    .Include(x => x.GameProviderConfiguration)
                    .SingleAsync(
                    x => x.BrandId == brandId && x.GameProviderId == gameProviderId);
            
            return bgpc.GameProviderConfiguration;
        }

        [Permission(Permissions.View, Module = Modules.GameManager)]
        IEnumerable<GameDTO> IGameQueries.GetGameDtos()
        {
            return _gameRepository.Games.Select(x => new GameDTO()
            {
                Id = x.Id,
                Name = x.Name,
                Type = x.Type,
                Url = x.EndpointPath,
                Status = x.IsActive ? "Active" : "Inactive",
                ProductId = x.GameProviderId,
                CreatedBy = x.CreatedBy,
                CreatedDate = x.CreatedDate,
                UpdatedBy = x.UpdatedBy,
                UpdatedDate = x.UpdatedDate
            });
        }

        GameDTO IGameQueries.GetGameDto(Guid gameId)
        {
            return _gameRepository
                .Games
                .Where(x => x.Id == gameId)
                .Select(x => new GameDTO()
            {
                CreatedBy = x.CreatedBy,
                CreatedDate = x.CreatedDate,
                Id = x.Id,
                Name = x.Name,
                UpdatedBy = x.UpdatedBy,
                UpdatedDate = x.UpdatedDate,
                Type = x.Type,
                ProductId = x.GameProviderId,
                Status = x.IsActive ? "Active" : "Inactive",
                Url = x.EndpointPath
            }).FirstOrDefault();
        }

        IEnumerable<BetLimitDTO> IGameQueries.GetBetLimits(Guid gameProviderId, Guid brandId)
        {
            return _gameRepository
                .BetLimits
                .Where(x => x.GameProviderId == gameProviderId && x.BrandId == brandId)
                .ToArray()
                .Select(x => GetBetLimitDto(x.Id));
        }

        public BetLimitDTO GetBetLimitDto(Guid id)
        {
            var betLimit = _gameRepository.BetLimits.SingleOrDefault(x => x.Id == id);

            if (betLimit == null) return null;

            var betLimitDto = new BetLimitDTO
            {
                Id = betLimit.Id,
                GameProviderId = betLimit.GameProviderId,
                LimitId = betLimit.Code,
                Name = betLimit.Name,
                FullName = betLimit.Code + " - " + betLimit.Name,
                Description = betLimit.Description
            };

            return betLimitDto;
        }

        IEnumerable<GameProviderDTO> IGameQueries.GetGameProviders(Guid brandId)
        {
            var brand =
                _gameRepository.Brands
                    .Include(x => x.BrandGameProviderConfigurations.Select(y => y.GameProvider))
                    .Single(x => x.Id == brandId);

            return brand
                .BrandGameProviderConfigurations
                .Select(x => new GameProviderDTO
                {
                    Id = x.GameProviderId,
                    Name = x.GameProvider.Name
                });
        }

        [Permission(Permissions.View, Module = Modules.ProductManager)]
        IEnumerable<GameProviderDTO> IGameQueries.GetGameProviderDtos()
        {
            return _gameRepository.GameProviders
                .Select(x => new GameProviderDTO
                {
                    Id = x.Id,
                    Name = x.Name,
                    Category = x.Category,
                    CreatedBy = x.CreatedBy,
                    UpdatedBy = x.UpdatedBy,
                    CreatedDate = x.CreatedDate,
                    UpdatedDate = x.UpdatedDate
                })
                .ToArray();
        }

        IEnumerable<GameProvider> IGameQueries.GetGameProviders()
        {
            return _gameRepository.GameProviders;
        }

        IEnumerable<GameProvider> IGameQueries.GetGameProvidersWithGames(Guid brandId)
        {
            var configurations = _gameRepository
                .BrandGameProviderConfigurations
                .Include(x => x.GameProvider)
                .Include(x => x.GameProvider.Games)
                .Where(x => x.BrandId == brandId);

            return configurations.Select(x => x.GameProvider).Include(x => x.Games);
        }

        Task<Data.Game> IGameQueries.GetGameByIdAsync(Guid gameId)
        {
            return _gameRepository.Games.SingleAsync(x => x.Id == gameId);
        }

        bool IGameQueries.DoesPlayerIdExist(Guid playerId)
        {
            return _gameRepository.Players.Any(p => p.Id == playerId);
        }
        async Task<decimal> IGameQueries.GetPlayerBalanceAsync(Guid playerId, Guid gameId)
        {
            var walletTemplateId = GetWalletTemplateId(playerId, gameId);
            var playerBalance = await _walletQueries.GetPlayerBalanceAsync(playerId, walletTemplateId);
            return playerBalance.Playable;
        }
        Task<Player> IGameQueries.GetPlayerDataAsync(Guid playerId)
        {
            return _gameRepository.Players.FirstOrDefaultAsync(x => x.Id == playerId);
        }
        async Task<string> IGameQueries.GetPlayerBetLimitCodeOrNullAsync(Guid vipLevelId, Guid gameProviderId, string currency)
        {
            return await GetPlayerBetLimitCodeQuery(vipLevelId, gameProviderId, currency).SingleOrDefaultAsync();
        }

        private IQueryable<string> GetPlayerBetLimitCodeQuery(Guid vipLevelId, Guid gameProviderId, string currency)
        {
            return _gameRepository.VipLevelBetLimits
                .Where(x => x.VipLevelId == vipLevelId 
                            &&  x.GameProviderId == gameProviderId 
                            &&  x.CurrencyCode == currency)
                .Select(x =>x.BetLimit.Code);
        }

        Task<bool> IGameQueries.DoesPlayerIdExistAsync(Guid playerId)
        {
            return _gameRepository.Players.AnyAsync(p => p.Id == playerId);
        }

        Task<string> IGameQueries.GetBrandCodeAsync(Guid brandId)
        {
            return _gameRepository.Brands.Where(x => x.Id == brandId).Select(b => b.Code).SingleOrDefaultAsync();
        }

        Task<Guid> IGameQueries.GetGameProviderIdByGameIdAsync(Guid gameId)
        {
            return _gameRepository.GameProviderConfigurations.Where(x => x.Id == gameId).Select(ge => ge.GameProviderId).SingleOrDefaultAsync();
        }

        string IGameQueries.GetPlayerBetLimitCodeOrNull(Guid vipLevelId, Guid gameProviderId, string currency)
        {
            return GetPlayerBetLimitCodeQuery(vipLevelId, gameProviderId, currency).SingleOrDefault();
        }

        public IEnumerable<GameAction> GetWinLossGameActions(Guid gameProviderId)
        {
            var gameActions = _gameRepository.GameActions
                .Include(o => o.Round)
                .Include(o => o.Round.Game)
                .Where(o => o.Round.Game.GameProviderId == gameProviderId)
                .Where(o => o.GameActionType == GameActionType.Won
                    || o.GameActionType==GameActionType.Lost);

            return gameActions;
        }

        [Permission(Permissions.View, Module = Modules.BetLevels)]
        public IEnumerable<BrandProductData> GetBrandProducts()
        {
            return _gameRepository.VipLevelBetLimits
                    .Select(x => new BrandProductData
                    {
                        BrandId = x.VipLevel.BrandId,
                        GameProviderId = x.GameProviderId,
                        BrandName = x.VipLevel.Brand.Name,
                        LicenseeId = x.VipLevel.Brand.Licensee.Id,
                        GameProviderName = x.GameProvider.Name,
                        CreatedBy = x.GameProvider.CreatedBy,
                        DateCreated = x.GameProvider.CreatedDate,
                        DateUpdated = x.GameProvider.UpdatedDate,
                        UpdatedBy = x.GameProvider.UpdatedBy
                    });
        }

        public IEnumerable<object> GetAssignedProductsData(IEnumerable<Guid> productIds)
        {
            return _gameRepository
                .GameProviders
                .Where(x => productIds.Contains(x.Id))
                .Select(x => new { x.Id, x.Name });
        }

        private Guid GetWalletTemplateId(Guid playerId, Guid gameId)
        {
            var query = from wtgp in _gameRepository.WalletTemplateGameProviders
                        join wt in _gameRepository.WalletTemplates on wtgp.WalletTemplateId equals wt.Id
                        join w in _gameRepository.Wallets on wt.Id equals w.Template.Id
                        join gp in _gameRepository.GameProviders on wtgp.GameProviderId equals gp.Id
                        join g in _gameRepository.Games on gp.Id equals g.GameProviderId
                        where gameId == g.Id && w.PlayerId == playerId
                        select wtgp.WalletTemplateId;

            return query.Single();
        }

        public string GetProductName(Guid gameId)
        {
            var product = _gameRepository.GameProviderConfigurations.FirstOrDefault(x => x.Id == gameId);
            if (product != null)
                return product.Name;

            return string.Empty;
        }
    }

    public class PlayerNotFoundException : RegoException
    {
        public PlayerNotFoundException(string message) : base(message) { }
    }
}