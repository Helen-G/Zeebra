using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web.Configuration;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Game.Data;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.Infrastructure.DataAccess.Game.Mappings;
using AFT.RegoV2.Shared;

namespace AFT.RegoV2.Infrastructure.DataAccess.Game
{
    public class GameRepository : DbContext, IGameRepository, ISeedable
    {
        private const string Schema = "game";
        private readonly Guid _brand138Id = new Guid("00000000-0000-0000-0000-000000000138");

        public virtual IDbSet<Round>                      Rounds { get; set; }
        public virtual IDbSet<GameAction>                 GameActions { get; set; }
        public virtual IDbSet<Core.Game.Data.Player>      Players { get; set; }
        public virtual IDbSet<GameProviderConfiguration>  GameProviderConfigurations { get; set; }
        public virtual IDbSet<BrandGameProviderConfiguration> BrandGameProviderConfigurations { get; set; }
        public virtual IDbSet<GameProvider>               GameProviders { get; set; }
        public virtual IDbSet<Core.Game.Data.Game> Games { get; set; }

        public virtual IDbSet<Core.Game.Data.Brand>       Brands { get; set; }
        public virtual IDbSet<Licensee>                   Licensees { get; set; }

        public virtual IDbSet<GameProviderBetLimit>       BetLimits { get; set; }
        public virtual IDbSet<VipLevel>                   VipLevels { get; set; }
        public virtual IDbSet<VipLevelGameProviderBetLimit> VipLevelBetLimits { get; set; }

        public virtual IDbSet<Wallet> Wallets { get; set; }
        public virtual IDbSet<WalletTemplate> WalletTemplates { get; set; }
        public virtual IDbSet<WalletTemplateGameProvider> WalletTemplateGameProviders { get; set; }

        public virtual IDbSet<GameCulture>                Cultures { get; set; }
        public virtual IDbSet<GameCurrency>               Currencies { get; set; }

        static GameRepository()
        {
            Database.SetInitializer(new GameRepositoryInitializer());
        }

        public GameRepository() : base("name=Default")
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
            modelBuilder.Conventions.Remove<ManyToManyCascadeDeleteConvention>();
            modelBuilder.Configurations.Add(new RoundMap(Schema));
            modelBuilder.Configurations.Add(new GameActionMap(Schema));
            modelBuilder.Configurations.Add(new PlayerMap(Schema));
            modelBuilder.Configurations.Add(new GameProviderConfigurationsMap(Schema));
            modelBuilder.Configurations.Add(new GameProviderMap(Schema));

            modelBuilder.Configurations.Add(new BrandGameProviderConfigurationMap(Schema));

            modelBuilder.Entity<Core.Game.Data.Game>()
                .HasKey(x => x.Id)
                .ToTable("Games", Schema);

            modelBuilder.Configurations.Add(new BrandMap(Schema));
            modelBuilder.Configurations.Add(new BetLimitMap(Schema));

            modelBuilder.Configurations.Add(new TransactionMap(Schema));
            modelBuilder.Configurations.Add(new WalletMap(Schema));

            modelBuilder.Entity<WalletTemplateGameProvider>()
                .HasKey(x => x.Id)
                .ToTable("WalletTemplateGameProviders", Schema);

            modelBuilder.Configurations.Add(new LockMap(Schema));
            modelBuilder.Entity<WalletTemplate>()
                .ToTable("WalletTemplates", Schema);

            modelBuilder.Entity<VipLevel>()
                .ToTable("VipLevels", Schema);

            modelBuilder.Entity<Licensee>()
                .ToTable("Licensees", Schema);

            modelBuilder.Entity<VipLevelGameProviderBetLimit>()
                .HasKey(x => new { x.VipLevelId, x.BetLimitId })
                .ToTable("xref_VipLevelBetLimits", Schema);

            modelBuilder.Entity<GameCulture>()
                .HasKey(x => x.Code)
                .ToTable("Cultures", Schema);

            modelBuilder.Entity<GameCurrency>()
                .HasKey(x => x.Code)
                .ToTable("Currencies", Schema);
        }


        public Core.Game.Entities.Round GetRound(string roundId)
        {
            return GetRound(b => b.ExternalRoundId == roundId);
        }

        public Core.Game.Entities.Round GetRound(Expression<Func<Round, bool>> condition)
        {
            var round = Rounds.Include(x => x.GameActions).SingleOrDefault(condition);

            return round == null ? null : new Core.Game.Entities.Round(round);
        }

        public Task<Guid> GetGameProviderIdByGameIdAsync(Guid gameId)
        {
            return GameProviderConfigurations.Where(ge => ge.Id == gameId).Select(ge => ge.GameProviderId).FirstOrDefaultAsync();
        }

        public GameAction GetGameActionByExternalTransactionId(string externalTransactionId, Guid gameProviderId)
        {
            var round = GetRound(x => x.GameActions.Any(t => t.ExternalTransactionId == externalTransactionId));

            return (round == null) ? null :
                round.Data.GameActions.Single(t => t.ExternalTransactionId == externalTransactionId);
        }

        public Core.Game.Entities.Round GetOrCreateRound(string roundId, TokenData tokenData)
        {
            if (Rounds.Any(x => x.ExternalRoundId == roundId))
                return GetRound(roundId);

            if (tokenData == null)
                throw new RegoException("Cannot create a game round without a token.");

            return new Core.Game.Entities.Round(roundId, tokenData);
        }

        public List<Core.Game.Entities.Round> GetPlayerRounds(Guid playerId)
        {
            var rounds = Rounds
                .Include(x => x.GameActions)
                .Where(x => x.PlayerId == playerId)
                .OrderBy(x => x.CreatedOn)
                .ToList();

            return rounds.Select(x => new Core.Game.Entities.Round(x)).ToList();
        }

        public List<GameProvider> GetGameProviderList()
        {
            return GameProviders.Include(g => g.GameProviderConfigurations).ToList();
        }

        
        public override int SaveChanges()
        {
            try
            {
                return base.SaveChanges();
            }
            catch (DbEntityValidationException e)
            {
                var dbValidationErrorMessages = e.EntityValidationErrors.ToArray();
                Trace.WriteLine(dbValidationErrorMessages.ToString());
                throw;
            }
        }

        public override Task<int> SaveChangesAsync()
        {
            try
            {
                return base.SaveChangesAsync();
            }
            catch (DbEntityValidationException e)
            {
                var dbValidationErrorMessages = e.EntityValidationErrors.ToArray();
                Trace.WriteLine(dbValidationErrorMessages.ToString());
                throw;
            }
        }

        public bool DoesBatchIdExist(string batchId, Guid gameProviderId)
        {
            return (batchId != null) && GetRound(b => b.GameActions.Any(tx => tx.ExternalBatchId == batchId)) != null;
        }

        public bool DoesGameActionExist(string externTransactionId, Guid gameProviderId)
        {
            return GameActions.Any(x => x.ExternalTransactionId == externTransactionId);
        }

        /// <summary>
        /// This method returns player wallet and locks the record with UPD (update) lock for the scope of the transaction
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="walletTemplateId">this parameter is required in order to select specific user wallet as other systems may not know user wallet id</param>
        /// <returns></returns>
        public Core.Game.Entities.Wallet GetWalletWithUPDLock(Guid playerId, Guid? walletTemplateId = null)
        {
            if (walletTemplateId.HasValue == false)
            {
                //querying main wallet structure id
                walletTemplateId =
                    Wallets.Where(w => w.PlayerId == playerId && w.Template.IsMain).Select(x => x.Template.Id).Single();
            }

            LockWallet(playerId, walletTemplateId.Value);
            var wallet = Wallets
                .Include(w => w.Template)
                .Include(w => w.Brand)
                .SingleOrDefault(x => x.PlayerId == playerId && x.Template.Id == walletTemplateId.Value);

            if (wallet == null)
                throw new RegoException("Wallet does not exist.");

            return new Core.Game.Entities.Wallet(wallet);
        }

        /// <summary>
        /// This method returns player wallet and locks the record with UPD (update) lock for the scope of the transaction
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="walletTemplateId">this parameter is required in order to select specific user wallet as other systems may not know user wallet id</param>
        /// <returns></returns>
        public async Task<Core.Game.Entities.Wallet> GetWalletWithUPDLockAsync(Guid playerId, Guid? walletTemplateId = null)
        {
            if (walletTemplateId.HasValue == false)
            {
                //querying main wallet structure id
                walletTemplateId =
                    Wallets.Where(w => w.PlayerId == playerId && w.Template.IsMain).Select(x => x.Template.Id).Single();
            }

            LockWallet(playerId, walletTemplateId.Value);
            var wallet = await Wallets
                .Include(w => w.Template)
                .Include(w => w.Brand)
                .SingleOrDefaultAsync(x => x.PlayerId == playerId && x.Template.Id == walletTemplateId.Value);

            if (wallet == null)
                throw new RegoException("Wallet does not exist.");

            return new Core.Game.Entities.Wallet(wallet);
        }

        protected virtual void LockWallet(Guid playerId, Guid walletTemplateId)
        {
            Database.ExecuteSqlCommand("SELECT * FROM game.Wallets WITH (ROWLOCK, UPDLOCK) WHERE PlayerId = @p0 AND Template_Id = @p1", playerId, walletTemplateId);
        }

        #region Seeding...

        public void Seed()
        {
            var mockGameWebsite = WebConfigurationManager.AppSettings["MockGameWebsite"];
            var brand138Id = new Guid("00000000-0000-0000-0000-000000000138");

            if (mockGameWebsite == null)
            {
                throw new RegoException("Setting MockGameWebsite is missing.");
            }

            SeedBrands();

            SeedGameProviders(mockGameWebsite);
            
            AddBrandProduct(brand138Id, new Guid("1E6001D6-722F-4774-B59C-05EDE2A74DB9")); // MOCK GP
            AddBrandProduct(brand138Id, new Guid("9FC056A8-D516-4864-B86F-77C5764749A5")); // MOCK GP 2
            AddBrandProduct(brand138Id, new Guid("7A058B86-4E73-43C1-931F-13ECFE6C2DBD")); // secure website
            AddBrandProduct(brand138Id, new Guid("A368EAFF-1A85-4DB3-B611-B410E6F0D0EF")); // ACS

            SeedPlayers();
            SeedBetLimits();

            SeedWallet();

            SaveChanges();
        }

        private void SeedWallet()
        {
            CreateTemplates();

            CreateWallet(new Guid("9A28104C-30E8-490F-B9DC-520CE5E4A0D0"), new Guid("315E8BBA-41AC-41A0-A3C1-C60E5A9CE960"));
            CreateWallet(new Guid("1AA1AE4D-D6DA-42D6-8FB2-1C481684E1F1"), new Guid("66AF937B-63D3-4CD4-89C4-7EECAD4FBB05"));
            CreateWallet(new Guid("65169985-4223-4A83-A5A9-E7ED1D36C7B7"), new Guid("D568A5E8-56AA-4A0E-96BC-D2FBCBFB5D34"));
            CreateWallet(new Guid("61D96ECB-363C-4B39-A886-9A7B9B0DFC3C"), new Guid("8D1E5228-CA52-4503-89B8-E65B720C82DE"));

            CreateWallet(new Guid("61D96ECB-363C-4B39-A886-9A7B9B0DFC2E"), new Guid("315E8BBA-41AC-41A0-A3C1-C60E5A9CE960"), false);
            CreateWallet(new Guid("61D96ECB-363C-4B39-A886-9A7B9B0DFC3E"), new Guid("66AF937B-63D3-4CD4-89C4-7EECAD4FBB05"), false);
            CreateWallet(new Guid("61D96ECB-363C-4B39-A886-9A7B9B0DFC4E"), new Guid("D568A5E8-56AA-4A0E-96BC-D2FBCBFB5D34"), false);
            CreateWallet(new Guid("61D96ECB-363C-4B39-A886-9A7B9B0DFC5E"), new Guid("8D1E5228-CA52-4503-89B8-E65B720C82DE"), false);

        }

        private enum SeededGameProviders
        {
            Mock = 0, Cowsino, Acs, Dopamine
        }

        private void SeedGameProviders(string mockGameWebsite)
        {
            var gameProviderIds = new[]
            {
                new Guid("1814418D-BC00-43B4-AD18-BEBEF6501D7F"),
                new Guid("9955C272-CC22-409A-B012-C5D37BF1242A"), 
                new Guid("321B0909-1768-42E2-8BD4-11A28CDAD039"),
                new Guid("A36D60C8-CF92-4620-AF4C-184EE010DE8C"),
                new Guid("2D6FAADD-9B88-41F8-8858-D07E4F3E3324")
            };

            const string script = @"
                IF (NOT EXISTS (SELECT * 
                                 FROM INFORMATION_SCHEMA.TABLES 
                                 WHERE TABLE_SCHEMA = '{0}' 
                                 AND  TABLE_NAME = 'GameApiLog4Net'))
                BEGIN
                    CREATE TABLE [{0}].[GameApiLog4Net] (
                        [Id] [int] IDENTITY (1, 1) NOT NULL,
                        [Date] [datetime] NOT NULL,
                        [Thread] [varchar] (255) NOT NULL,
                        [Level] [varchar] (50) NOT NULL,
                        [Logger] [varchar] (255) NOT NULL,
                        [Message] [varchar] (4000) NOT NULL,
                        [Exception] [varchar] (4000) NULL,
                        [Application] [varchar] (200) NULL
                    )
                END";
            Database.ExecuteSqlCommand(string.Format(script, Schema));

            GameProviders.AddOrUpdate(f => f.Name,
                new GameProvider
                {
                    Name = "Mock Casino",
                    Id = gameProviderIds[(int)SeededGameProviders.Mock],
                    GameProviderConfigurations = new List<GameProviderConfiguration>
                    {
                        // regular, unsecure mock configuration
                        new GameProviderConfiguration
                        {
                            Id = new Guid("1E6001D6-722F-4774-B59C-05EDE2A74DB9"), // This has to be hardcoded as it is used in jMeter tests
                            Name = "Regular", 
                            Endpoint = mockGameWebsite + "/Game/Index?",
                            Type = "Casino",
                            CreatedBy = "SuperAdmin",
                            CreatedDate = DateTime.UtcNow,
                            GameProviderId = gameProviderIds[(int)SeededGameProviders.Mock]
                        }
                    },
                    Games = new List<Core.Game.Data.Game> 
                    {
                        new Core.Game.Data.Game
                        {
                            Id = new Guid("67277E31-800C-4793-B029-AE8231E4B0FA"),
                            Name = "Slots", 
                            EndpointPath = "gameName={GameName}",
                            GameProviderId = gameProviderIds[(int)SeededGameProviders.Mock]
                        },
                        new Core.Game.Data.Game
                        {
                            Id = new Guid("C17F4D3F-2F99-42A4-A766-4493EFF6DB9F"),
                            Name = "Roulette", 
                            EndpointPath = "gameName={GameName}",
                            GameProviderId = gameProviderIds[(int)SeededGameProviders.Mock]
                        },
                        new Core.Game.Data.Game
                        {
                            Id = new Guid("BD8BF6F5-BC5D-4BC6-BD8E-C48E45DD0977"),
                            Name = "Blackjack",
                            EndpointPath = "gameName={GameName}",
                            GameProviderId = gameProviderIds[(int)SeededGameProviders.Mock]
                        },
                        new Core.Game.Data.Game
                        {
                            Id = new Guid("B641B4E9-CA08-4443-8FD3-8D1A43727C3E"),
                            Name = "Poker",
                            EndpointPath = "gameName={GameName}",
                            GameProviderId = gameProviderIds[(int)SeededGameProviders.Mock]
                        }
                    }
                },
                new GameProvider
                {
                    Name = "Mock Sportsbetting",
                    Id = new Guid("18FB823B-435D-42DF-867E-3BA38ED92060"),
                    GameProviderConfigurations = new List<GameProviderConfiguration>
                    {
                        new GameProviderConfiguration
                        {
                            Id = new Guid("9FC056A8-D516-4864-B86F-77C5764749A5"),
                            Name = "Regular", 
                            Endpoint = mockGameWebsite + "/Game/Index?",
                            Type = "Sports",
                            CreatedBy = "SuperAdmin",
                            CreatedDate = DateTime.UtcNow,
                            GameProviderId = new Guid("18FB823B-435D-42DF-867E-3BA38ED92060")
                        }
                    },
                    Games = new List<Core.Game.Data.Game> 
                    {
                        new Core.Game.Data.Game
                        {
                            Id = new Guid("C18CEBA7-77D8-4E5E-8E1F-F046B7F7544F"),
                            Name = "Horses",
                            EndpointPath = "gameName={GameName}",
                            GameProviderId = new Guid("18FB823B-435D-42DF-867E-3BA38ED92060")
                        },
                        new Core.Game.Data.Game
                        {
                            Id = new Guid("BDCD4277-4FF7-46EF-B8ED-F4192E51F03C"),
                            Name = "Football",
                            EndpointPath = "gameName={GameName}",
                            GameProviderId = new Guid("18FB823B-435D-42DF-867E-3BA38ED92060")
                        },
                    }
                },
                new GameProvider
                {
                    Name = "Mock Secure",
                    Id = new Guid("794B0C51-7493-4C2C-9EBA-F0BEE8BAD761"),
                    AuthorizationClientId = "MOCK_CLIENT_ID",
                    AuthorizationSecret = "MOCK_CLIENT_SECRET",
                    // secure mock configuration
                    GameProviderConfigurations = new List<GameProviderConfiguration> 
                    {
                        new GameProviderConfiguration
                        {
                            Id = new Guid("7A058B86-4E73-43C1-931F-13ECFE6C2DBD"),
                            Name = "Secure",
                            Endpoint = mockGameWebsite + "/Game/Index?isoauth=true&",
                            Type = "Casino",
                            CreatedBy = "SuperAdmin",
                            CreatedDate = DateTime.UtcNow,
                            GameProviderId = new Guid("794B0C51-7493-4C2C-9EBA-F0BEE8BAD761")
                        }
                    },
                    Games =  new List<Core.Game.Data.Game>()
                    {
                        new Core.Game.Data.Game()
                        {
                            Id = new Guid("486FB3E3-FC88-461B-AECD-A8F4C504182D"),
                            Name = "Secure game",
                            EndpointPath = "gameName={GameName}",
                            GameProviderId = new Guid("794B0C51-7493-4C2C-9EBA-F0BEE8BAD761")
                        }
                    }
                },
                new GameProvider
                {
                    Id = gameProviderIds[(int)SeededGameProviders.Cowsino],
                    Name = "Cowsino",
                    GameProviderConfigurations = new List<GameProviderConfiguration>
                    {
                        new GameProviderConfiguration
                        {
                            Id = new Guid("D368EAFF-1A85-4DB3-B611-B410E6F0D0EF"),
                            Name = "Mock Lobby",
                            Endpoint = "http://cowsino-mock-regov2.flycowdev.com/mocklobby",
                            GameProviderId = gameProviderIds[(int)SeededGameProviders.Cowsino]
                        }
                    },
                    Games = new List<Core.Game.Data.Game>
                    {
                        new Core.Game.Data.Game
                        {
                            Id = new Guid("D3D43F80-F7A3-4CC4-9625-4DD442BEDF90"),
                            EndpointPath = "",
                            Name = "Cowsino",
                            GameProviderId = gameProviderIds[(int)SeededGameProviders.Cowsino]
                        }
                    }
                },
                new GameProvider
                {
                    Id = gameProviderIds[(int)SeededGameProviders.Acs],
                    Name = "ACS dev",
                    AuthorizationClientId = "ACS_DEV_CLIENT_ID",
                    AuthorizationSecret = "ACS_DEV_CLIENT_SECRET",
                    GameProviderConfigurations = new List<GameProviderConfiguration>
                    {
                        new GameProviderConfiguration
                        {
                            Id = new Guid("A368EAFF-1A85-4DB3-B611-B410E6F0D0EF"),
                            Name = "Soleil dev",
                            Endpoint = "http://ph-sun-web01.acsdev.net",
                            GameProviderId = gameProviderIds[(int)SeededGameProviders.Acs]
                        }
                    },
                    Games = new List<Core.Game.Data.Game>
                    {
                        new Core.Game.Data.Game
                        {
                            Id = Guid.NewGuid(),
                            EndpointPath = "/soleilmem/GameRego.aspx",
                            Name = "Soleil",
                            GameProviderId = gameProviderIds[(int)SeededGameProviders.Acs]
                        }
                    }

                },
                new GameProvider
                {
                    Id = gameProviderIds[(int)SeededGameProviders.Dopamine],
                    Name = "Dopamine games",
                    GameProviderConfigurations = new List<GameProviderConfiguration>
                    {
                        new GameProviderConfiguration
                        {
                            Id = Guid.NewGuid(),
                            Name = "Dopamine dev",
                            Endpoint = "http://gameserver.dopamine-gaming.com",
                            GameProviderId = gameProviderIds[(int)SeededGameProviders.Dopamine]
                        }
                    },
                    Games = new List<Core.Game.Data.Game>
                    {
                        new Core.Game.Data.Game
                        {
                            Id = Guid.NewGuid(),
                            EndpointPath = "/platform/",
                            Name = "Dopamine",
                            GameProviderId = gameProviderIds[(int)SeededGameProviders.Dopamine]
                        }
                    }
                });

            SaveChanges();
        }

        private void SeedBrands()
        {
            var licenseeId = new Guid("4A557EA9-E6B7-4F1F-AEE5-49E170ADB7E0");
           
            Licensees.AddOrUpdate(x => x.Id, new Licensee { Id = licenseeId });

            Brands.AddOrUpdate(x => x.Id,
                new Core.Game.Data.Brand
                {
                    Id = new Guid("00000000-0000-0000-0000-000000000138"), 
                    Code = "138",
                    LicenseeId = licenseeId,
                    TimezoneId = "Pacific Standard Time"
                });

            Brands.AddOrUpdate(x => x.Id,
                new Core.Game.Data.Brand
                {
                    Id = new Guid("D2CFDF01-FB79-4365-9E47-FC8E4DC1349C"),
                    Code = "831",
                    LicenseeId = licenseeId,
                    TimezoneId = "Pacific Standard Time"
                });
        }

        private void AddBrandProduct(Guid brandId, Guid productConfigurationId)
        {

            var gameProviderId = GameProviderConfigurations.Single(x => x.Id == productConfigurationId).GameProviderId;

            var brand = Brands.Include(x => x.BrandGameProviderConfigurations).Single(x => x.Id == brandId);
            if (brand.BrandGameProviderConfigurations.Any(x => x.BrandId == brandId && x.GameProviderId == gameProviderId)) 
                return;

            brand.BrandGameProviderConfigurations.Add(new BrandGameProviderConfiguration
            {
                Id = Guid.NewGuid(),
                BrandId = brand.Id,
                GameProviderConfigurationId = productConfigurationId,
                GameProviderId = gameProviderId
            });
        }

        private void SeedPlayers()
        {
            SeedPlayer(new Guid("315E8BBA-41AC-41A0-A3C1-C60E5A9CE960"), "testplayer", new Guid("9A28104C-30E8-490F-B9DC-520CE5E4A0D0"));
            SeedPlayer(new Guid("66AF937B-63D3-4CD4-89C4-7EECAD4FBB05"), "testuser", new Guid("1AA1AE4D-D6DA-42D6-8FB2-1C481684E1F1"));
            SeedPlayer(new Guid("D568A5E8-56AA-4A0E-96BC-D2FBCBFB5D34"), "lockeduser", new Guid("65169985-4223-4A83-A5A9-E7ED1D36C7B7"));
            SeedPlayer(new Guid("8D1E5228-CA52-4503-89B8-E65B720C82DE"), "inactiveuser", new Guid("61D96ECB-363C-4B39-A886-9A7B9B0DFC3C"));
            SaveChanges();
        }

        private void SeedPlayer(Guid id, string name, Guid walletId)
        {
            Players.AddOrUpdate(new Core.Game.Data.Player
            {
                Id = id,
                Name = name,
                VipLevelId = new Guid("0447E567-BDC6-4330-979C-5E0984BFB626"),
                CultureCode = "en-US",
                CurrencyCode = "CAD",
                BrandId = new Guid("00000000-0000-0000-0000-000000000138"),
                WalletId = walletId
            });
        }

        private void SeedBetLimits()
        {
            var betLevelId1 = new Guid("5526DF3C-9167-48D8-BBBC-4FB21501D793");
            var betLevelId2 = new Guid("AE101BE6-AE08-4C9B-BBF1-810505B452D7");
            var betLevelId3 = new Guid("A59F67D0-2D68-4E63-903C-7073DD50D92A");

            BetLimits.AddOrUpdate(x => x.Id, new GameProviderBetLimit
            {
                Id = betLevelId1,
                BrandId = new Guid("00000000-0000-0000-0000-000000000138"),
                Code = "10",
                GameProviderId = new Guid("1814418D-BC00-43B4-AD18-BEBEF6501D7F"),
                Name = "BetLevel1"
            });
            BetLimits.AddOrUpdate(x => x.Id, new GameProviderBetLimit
            {
                Id = betLevelId2,
                BrandId = new Guid("00000000-0000-0000-0000-000000000138"),
                Code = "20",
                GameProviderId = new Guid("1814418D-BC00-43B4-AD18-BEBEF6501D7F"),
                Name = "BetLevel2"
            });
            BetLimits.AddOrUpdate(x => x.Id, new GameProviderBetLimit
            {
                Id = betLevelId3,
                BrandId = new Guid("00000000-0000-0000-0000-000000000138"),
                Code = "30",
                GameProviderId = new Guid("1814418D-BC00-43B4-AD18-BEBEF6501D7F"),
                Name = "BetLevel3"
            });
        }

        private void CreateTemplates()
        {
            var mainTemplate = new WalletTemplate
            {
                Id = new Guid("9D366EF4-7AEF-4DFE-80E1-045909DC8EFD"),
                IsMain = true,
                CurrencyCode = "CAD",
                BrandId = _brand138Id,
                WalletTemplateGameProviders = new List<WalletTemplateGameProvider>
                {
                    new WalletTemplateGameProvider
                    {
                        Id = Guid.NewGuid(),
                        WalletTemplateId = new Guid("9D366EF4-7AEF-4DFE-80E1-045909DC8EFD"),
                        GameProviderId = new Guid("18FB823B-435D-42DF-867E-3BA38ED92060")
                    },
                    new WalletTemplateGameProvider
                    {
                        Id = Guid.NewGuid(),
                        WalletTemplateId = new Guid("9D366EF4-7AEF-4DFE-80E1-045909DC8EFD"),
                        GameProviderId = new Guid("794B0C51-7493-4C2C-9EBA-F0BEE8BAD761")
                    }
                }
            };
            var productTemplate = new WalletTemplate
            {
                Id = new Guid("5855385F-8013-48E1-A3E4-B0BC1A49EF33"),
                IsMain = false,
                CurrencyCode = "CAD",
                BrandId = _brand138Id,
                WalletTemplateGameProviders = new List<WalletTemplateGameProvider>
                {
                    new WalletTemplateGameProvider
                    {
                        Id = Guid.NewGuid(),
                        WalletTemplateId = new Guid("5855385F-8013-48E1-A3E4-B0BC1A49EF33"),
                        GameProviderId = new Guid("1814418D-BC00-43B4-AD18-BEBEF6501D7F")
                    }
                }
            };

            WalletTemplates.AddOrUpdate(t => t.Id, mainTemplate);
            WalletTemplates.AddOrUpdate(t => t.Id, productTemplate);

            SaveChanges();
        }

        private void CreateWallet(Guid id, Guid playerId, bool isMain = true)
        {
            var brand = Brands.Single(b => b.Id == _brand138Id);
            Wallets.AddOrUpdate(x => x.Id, new Wallet
            {
                Id = id,
                PlayerId = playerId,
                Brand = brand,
                Template = WalletTemplates.Single(t => t.IsMain == isMain && t.BrandId == _brand138Id)
            });
        }

        #endregion
    }
}
