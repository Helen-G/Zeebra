using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Game;
using AFT.RegoV2.Core.Game.Data;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.Shared;
using Player = AFT.RegoV2.Core.Game.Data.Player;
using VipLevel = AFT.RegoV2.Core.Game.Data.VipLevel;

namespace AFT.RegoV2.Tests.Common.TestDoubles
{
    public class FakeGameRepository : IGameRepository
    {
        private readonly FakeDbSet<Round> _bets = new FakeDbSet<Round>();
        private readonly FakeDbSet<GameAction> _betTransactions = new FakeDbSet<GameAction>();
        private readonly FakeDbSet<Player> _players = new FakeDbSet<Player>();
        private readonly FakeDbSet<GameProviderConfiguration> _configurations = new FakeDbSet<GameProviderConfiguration>();
        private readonly FakeDbSet<BrandGameProviderConfiguration> _brandGameProviderConfigurations = new FakeDbSet<BrandGameProviderConfiguration>();
        private readonly FakeDbSet<GameProvider> _gameProviders = new FakeDbSet<GameProvider>();
        private readonly FakeDbSet<Game> _games = new FakeDbSet<Game>();
        private readonly FakeDbSet<Brand> _brands = new FakeDbSet<Brand>();
        private readonly FakeDbSet<GameProviderBetLimit> _betLimits = new FakeDbSet<GameProviderBetLimit>();
        private readonly FakeDbSet<VipLevel> _vipLevels = new FakeDbSet<VipLevel>();
        private readonly FakeDbSet<VipLevelGameProviderBetLimit> _vipLevelBetLimit = new FakeDbSet<VipLevelGameProviderBetLimit>();
        private readonly FakeDbSet<Licensee> _licensees = new FakeDbSet<Licensee>();
        private readonly FakeDbSet<Wallet> _wallets = new FakeDbSet<Wallet>();
        private readonly FakeDbSet<GameCulture> _cultures = new FakeDbSet<GameCulture>();
        private readonly FakeDbSet<WalletTemplate> _templates = new FakeDbSet<WalletTemplate>();
        private readonly FakeDbSet<WalletTemplateGameProvider> _walletTemplateGameProviders = new FakeDbSet<WalletTemplateGameProvider>();
        private readonly FakeDbSet<GameCurrency> _currencies = new FakeDbSet<GameCurrency>();

        public EventHandler SavedChanges;

        public IDbSet<Round> Rounds { get { return _bets; } }
        public IDbSet<GameAction> GameActions { get { return _betTransactions; } }
        public IDbSet<Player> Players { get { return _players; } }

        
        public IDbSet<GameProviderConfiguration>     GameProviderConfigurations { get { return _configurations; } }
        public IDbSet<BrandGameProviderConfiguration> BrandGameProviderConfigurations { get { return _brandGameProviderConfigurations; } }
        public IDbSet<Game> Games { get { return _games; } }
        public IDbSet<GameProvider> GameProviders { get { return _gameProviders; } }
        public IDbSet<GameProviderBetLimit> BetLimits { get { return _betLimits; } }

        public IDbSet<VipLevel> VipLevels { get { return _vipLevels; } }
        public IDbSet<VipLevelGameProviderBetLimit> VipLevelBetLimits { get { return _vipLevelBetLimit; } }
        public IDbSet<Brand> Brands { get { return _brands; } }
        public IDbSet<Licensee> Licensees { get { return _licensees; } }
        public IDbSet<GameCulture> Cultures { get { return _cultures; } }
        public IDbSet<GameCurrency> Currencies { get { return _currencies; } }
        public IDbSet<Wallet> Wallets
        {
            get { return _wallets; }
        }
        public IDbSet<WalletTemplate> WalletTemplates
        {
            get { return _templates; }
        }

        public IDbSet<WalletTemplateGameProvider> WalletTemplateGameProviders
        {
            get { return _walletTemplateGameProviders; }
        }

        public int SaveChanges()
        {
            if (_wallets.Any() && !_wallets.Any(x => x.Transactions.AllElementsAreUnique()))
            {
                throw new RegoException("Transactions with duplicate Ids were found");
            }

            var handler = SavedChanges;
            if (handler != null)
                handler(this, EventArgs.Empty);

            return 0;
        }

        public Task<int> SaveChangesAsync()
        {
            SaveChanges();

            return Task.FromResult(0);
        }



        public Core.Game.Entities.Round GetRound(string roundId)
        {
            return GetRound(b => b.ExternalRoundId == roundId);
        }
        public Core.Game.Entities.Round GetRound(System.Linq.Expressions.Expression<Func<Round, bool>> condition)
        {
            var roundDto = Rounds.Include(x => x.GameActions).SingleOrDefault(condition);

            return (roundDto != null) ? new Core.Game.Entities.Round(roundDto) : null;
        }


        public Core.Game.Entities.Round GetOrCreateRound(string roundId, TokenData tokenData)
        {
            // if the bet exists, get t
            var round = GetRound(roundId);

            if (round == null)
            {
                if (tokenData == null)
                {
                    throw new RegoException("Cannot create a bet without a token.");
                }

                round = new Core.Game.Entities.Round(roundId, tokenData);
            }

            return round;
        }

        public List<Core.Game.Entities.Round> GetPlayerRounds(Guid playerId)
        {
            return Rounds.Where(x => x.PlayerId == playerId).Select(x => new Core.Game.Entities.Round(x)).ToList();
        }

        public List<GameProvider> GetGameProviderList()
        {
            return GameProviders.Include(x => x.GameProviderConfigurations).ToList();
        }

        public Task<Guid> GetGameProviderIdByGameIdAsync(Guid gameId)
        {
            return GameProviderConfigurations.Where(x => x.Id == gameId).Select(ge => ge.GameProviderId).FirstOrDefaultAsync();
        }

        public GameAction GetGameActionByExternalTransactionId(string externalTransactionId, Guid gameProviderId)
        {
            var round = GetRound(x => x.GameActions.Any(t => t.ExternalTransactionId == externalTransactionId));

            return (round == null) ? null :
                round.Data.GameActions.Single(t => t.ExternalTransactionId == externalTransactionId);
        }


        public bool DoesBatchIdExist(string batchId, Guid gameProviderId)
        {
            return (batchId != null) && Rounds.Include(x => x.GameActions).Any(bet => bet.GameActions.Any(tx => tx.ExternalBatchId == batchId));
        }

        bool IGameRepository.DoesGameActionExist(string externTransactionId, Guid gameProviderId)
        {
            return GetRound(b => b.GameActions.Any(x => x.ExternalTransactionId == externTransactionId)) != null;
        }




        public Core.Game.Entities.Wallet GetWalletWithUPDLock(Guid playerId, Guid? walletTemplateId = null)
        {
            if (walletTemplateId.HasValue == false || walletTemplateId.Value == Guid.Empty)
            {
                //querying main wallet structure id
                walletTemplateId =
                    Wallets.Where(w => w.PlayerId == playerId && w.Template.IsMain).Select(x => x.Template.Id).Single();
            }
            var wallet = Wallets
                .Include(w => w.Template)
                .SingleOrDefault(x => x.PlayerId == playerId && x.Template.Id == walletTemplateId.Value);

            if (wallet == null)
                throw new RegoException("Wallet does not exist.");

            return new Core.Game.Entities.Wallet(wallet);
        }

        public async Task<Core.Game.Entities.Wallet> GetWalletWithUPDLockAsync(Guid playerId, Guid? walletTemplateId = null)
        {
            if (walletTemplateId.HasValue == false)
            {
                //querying main wallet structure id
                walletTemplateId =
                    Wallets.Where(w => w.PlayerId == playerId && w.Template.IsMain).Select(x => x.Template.Id).Single();
            }

            var wallet = await Wallets
                .Include(w => w.Template)
                .SingleOrDefaultAsync(x => x.PlayerId == playerId && x.Template.Id == walletTemplateId.Value);

            if (wallet == null)
                throw new RegoException("Wallet does not exist.");

            return new Core.Game.Entities.Wallet(wallet);
        }
    }
}
