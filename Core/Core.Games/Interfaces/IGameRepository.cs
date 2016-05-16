using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Game.Data;

namespace AFT.RegoV2.Core.Game.Interfaces
{
    public interface IGameRepository
    {
        IDbSet<Round>               Rounds { get; }
        IDbSet<GameAction>          GameActions { get; }
        IDbSet<Player>              Players { get; }
        IDbSet<GameProviderConfiguration>        GameProviderConfigurations { get; }
        IDbSet<BrandGameProviderConfiguration> BrandGameProviderConfigurations { get; }
        IDbSet<GameProvider>          GameProviders { get; }
        IDbSet<Core.Game.Data.Game> Games { get; }

        IDbSet<Data.Brand>          Brands { get; }
        IDbSet<Licensee>            Licensees { get; }

        IDbSet<GameProviderBetLimit>            BetLimits { get; }
        IDbSet<VipLevel>            VipLevels { get; }
        IDbSet<VipLevelGameProviderBetLimit>    VipLevelBetLimits { get; }

        IDbSet<GameCulture> Cultures { get; }
        IDbSet<GameCurrency>            Currencies { get;  }
            
        Entities.Round             GetRound(string roundId);
        Entities.Round             GetRound(Expression<Func<Round, bool>> condition);
        Entities.Round             GetOrCreateRound(string roundId, TokenData token);
        List<Entities.Round>       GetPlayerRounds(Guid playerId);

        GameAction GetGameActionByExternalTransactionId(string externalTransactionId, Guid gameProviderId);
        bool DoesBatchIdExist(string batchId, Guid gameProviderId);
        bool DoesGameActionExist(string externalTransactionId, Guid gameProviderId);

        List<GameProvider> GetGameProviderList();

        Task<Guid>      GetGameProviderIdByGameIdAsync(Guid gameId);

        IDbSet<Wallet> Wallets { get; }
        IDbSet<Data.WalletTemplate> WalletTemplates { get; }
        IDbSet<Data.WalletTemplateGameProvider> WalletTemplateGameProviders { get; }

        Entities.Wallet GetWalletWithUPDLock(Guid playerId, Guid? walletTemplateId);
        Task<Entities.Wallet> GetWalletWithUPDLockAsync(Guid playerId, Guid? walletTemplateId);

        int SaveChanges();
        Task<int> SaveChangesAsync();
    }
}
