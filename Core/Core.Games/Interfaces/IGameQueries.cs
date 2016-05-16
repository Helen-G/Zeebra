using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Game.Data;
using Round = AFT.RegoV2.Core.Game.Entities.Round;

namespace AFT.RegoV2.Core.Game.Interfaces
{
    public interface IGameQueries : IApplicationService
    {
        IEnumerable<GameProviderConfiguration>           GetGameProviderConfigurations();

        IEnumerable<GameDTO>        GetGameDtos();
        GameDTO                     GetGameDto(Guid gameId);

        IEnumerable<BetLimitDTO>    GetBetLimits(Guid gameProviderId, Guid gameId);
        BetLimitDTO                 GetBetLimitDto(Guid id);
        Round              GetRoundByGameActionId(Guid gameActiondId);
        List<Round>        GetRoundHistory(TokenData token, int recordCount);

        string                      GetPlayerBetLimitCodeOrNull(Guid vipLevelId, Guid gameProviderId, string currency);
        Task<string>                GetPlayerBetLimitCodeOrNullAsync(Guid vipLevelId, Guid gameProviderId, string currency);
        Task<bool>                  DoesPlayerIdExistAsync(Guid playerId);
        Task<Player>                GetPlayerDataAsync(Guid playerId);
        Task<decimal>               GetPlayerBalanceAsync(Guid playerId, Guid gameId);
        

        IEnumerable<GameProvider>     GetGameProviders();
        IEnumerable<GameProviderDTO>  GetGameProviderDtos();
        IEnumerable<GameProviderDTO>  GetGameProviders(Guid brandId);
        Task<Guid>                  GetGameProviderIdByGameIdAsync(Guid gameId);
        
        decimal                     GetPlayableBalance(Guid playerId, Guid gameId);
        Player                      GetPlayerData(Guid playerId);
        bool                        DoesPlayerIdExist(Guid playerId);
        IEnumerable<object>         GetAssignedProductsData(IEnumerable<Guid> productIds);
        

        Data.Brand             GetBrand(string brandCode);
        Data.Brand             GetBrand(Guid brandId);
        Task<string>                GetBrandCodeAsync(Guid brandId);
        IEnumerable<BrandProductData> GetBrandProducts();

        void                        ValidateBatchIsUnique(string batchId, Guid gameProviderId);
        IEnumerable<GameAction>     GetWinLossGameActions(Guid gameProviderId);

        IEnumerable<GameProvider> GetGameProvidersWithGames(Guid brandId);

        Task<Data.Game> GetGameByIdAsync(Guid gameId);

        Task<GameProviderConfiguration> GetGameProviderConfigurationAsync(Guid brandId, Guid gameProviderId);
    }
}