using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using AFT.RegoV2.Core.Bonus;
using AFT.RegoV2.Core.Bonus.Data;
using AFT.RegoV2.Core.Bonus.Data.ViewModels;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Infrastructure.DataAccess.Bonus.Migrations;
using AFT.RegoV2.Shared;
using BonusRedemption = AFT.RegoV2.Core.Bonus.Entities.BonusRedemption;
using Currency = AFT.RegoV2.Core.Bonus.Data.Currency;
using RiskLevel = AFT.RegoV2.Core.Bonus.Data.RiskLevel;
using VipLevel = AFT.RegoV2.Core.Bonus.Data.VipLevel;
using WalletTemplate = AFT.RegoV2.Core.Bonus.Data.WalletTemplate;

namespace AFT.RegoV2.Infrastructure.DataAccess.Bonus
{
    public class BonusRepository : DbContext, IBonusRepository, ISeedable
    {
        static BonusRepository()
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<BonusRepository, Configuration>());
        }

        private readonly Guid _brand138Id = new Guid("00000000-0000-0000-0000-000000000138");
        private readonly Guid _brand831Id = new Guid("D2CFDF01-FB79-4365-9E47-FC8E4DC1349C");

        public BonusRepository() : base("name=Default") { }

        public virtual IDbSet<Template> Templates { get; set; }
        public virtual IDbSet<Core.Bonus.Data.Bonus> Bonuses { get; set; }
        public virtual IDbSet<Core.Bonus.Data.Player> Players { get; set; }
        public virtual IDbSet<Core.Bonus.Data.Brand> Brands { get; set; }
        public virtual IDbSet<Core.Bonus.Data.Game> Games { get; set; }

        public Core.Bonus.Entities.Player GetLockedPlayer(Guid playerId)
        {
            LockPlayer(playerId);
            var data = Players
                .Include(p => p.Wallets)
                .Include(p => p.RiskLevels)
                .Include(p => p.Wallets.Select(w => w.BonusesRedeemed))
                .Include(p => p.ReferredWith)
                .SingleOrDefault(p => p.Id == playerId);
            if (data == null)
                throw new RegoException(string.Format("Player with Id '{0}' was not found", playerId));

            return new Core.Bonus.Entities.Player(data);
        }

        public Core.Bonus.Entities.Bonus GetLockedBonus(Guid bonusId)
        {
            LockBonus(bonusId);
            var bonuses = Bonuses.Where(bonus => bonus.Id == bonusId);
            if (bonuses.Any() == false)
                throw new RegoException("Bonus id is not valid.");
            var currentVersion = bonuses.Max(bonus => bonus.Version);
            var bonusData = Bonuses
                .Include(bonus => bonus.Statistic)
                .Single(x => x.Id == bonusId && x.Version == currentVersion);

            return new Core.Bonus.Entities.Bonus(bonusData);
        }

        public Core.Bonus.Entities.Bonus GetLockedBonus(string bonusCode)
        {
            var bonus = GetLockedBonusOrNull(bonusCode);
            if (bonus == null)
                throw new RegoException("Bonus code is not valid.");

            return bonus;
        }

        public Core.Bonus.Entities.Bonus GetLockedBonusOrNull(string bonusCode)
        {
            var bonusData = GetCurrentVersionBonuses()
                .Where(b => b.Template != null && b.Template.Info.Mode == IssuanceMode.AutomaticWithCode)
                .SingleOrDefault(b => b.Code == bonusCode);

            if (bonusData == null)
                return null;

            LockBonus(bonusData.Id);
            return new Core.Bonus.Entities.Bonus(bonusData);
        }

        public BonusRedemption GetBonusRedemption(Guid playerId, Guid redemptionId)
        {
            var player = GetLockedPlayer(playerId);
            var bonusRedemption = player.BonusesRedeemed.SingleOrDefault(redemption => redemption.Id == redemptionId);
            if (bonusRedemption == null)
                throw new RegoException(string.Format("Redemption not found with id: {0}", redemptionId));

            return new BonusRedemption(bonusRedemption);
        }

        public IQueryable<Core.Bonus.Data.Bonus> GetCurrentVersionBonuses()
        {
            var currentIdVersion = Bonuses
                .GroupBy(bonus => bonus.Id)
                .Select(group => new { Id = group.Key, Version = group.Max(obj => obj.Version) });

            return Bonuses
                    .Include(bonus => bonus.Statistic)
                    .Where(bonus => currentIdVersion.Contains(new { bonus.Id, bonus.Version }));
        }

        public void RemoveGameContributionsForGame(Guid gameId)
        {
            var gameContributions = Templates
                .SelectMany(template => template.Wagering.GameContributions)
                .Where(contribution => contribution.GameId == gameId);
            foreach (var gameContribution in gameContributions)
            {
                Entry(gameContribution).State = EntityState.Deleted;
            }
        }

        protected virtual void LockBonus(Guid bonusId)
        {
            Database.ExecuteSqlCommand("SELECT * FROM bonus.Bonuses WITH (ROWLOCK, UPDLOCK) WHERE Id = @p0", bonusId);
        }

        protected virtual void LockPlayer(Guid playerId)
        {
            Database.ExecuteSqlCommand("SELECT * FROM bonus.Players WITH (ROWLOCK, UPDLOCK) WHERE Id = @p0", playerId);
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("bonus");
            modelBuilder.Entity<Core.Bonus.Data.Bonus>()
                .ToTable("Bonuses")
                .HasKey(p => new { p.Id, p.Version })
                .Property(p => p.StatisticId).HasColumnName("Statistic_Id");

            modelBuilder.Entity<Currency>()
                .HasKey(c => new { c.Id, c.BrandId });
            modelBuilder.Entity<Product>()
                .HasKey(c => new { c.Id, c.WalletTemplateId });

            modelBuilder.Entity<Template>()
                .HasKey(p => new { p.Id, p.Version });
            modelBuilder.Entity<Template>()
                .HasOptional(p => p.Info)
                .WithOptionalDependent();
            modelBuilder.Entity<Template>()
                .HasOptional(p => p.Availability)
                .WithOptionalDependent();
            modelBuilder.Entity<Template>()
                .HasOptional(p => p.Rules)
                .WithOptionalDependent();
            modelBuilder.Entity<Template>()
                .HasOptional(p => p.Wagering)
                .WithOptionalDependent();
            modelBuilder.Entity<Template>()
                .HasOptional(p => p.Notification)
                .WithOptionalDependent();

            modelBuilder.Entity<TierBase>()
                .ToTable("TemplateTiers")
                .Property(p => p.Reward).HasPrecision(16, 4);
            modelBuilder.Entity<RewardTier>()
                .Ignore(p => p.Tiers)
                .Ignore(p => p.HighDepositTiers);
            modelBuilder.Entity<HighDepositTier>()
                .Property(p => p.NotificationPercentThreshold).HasPrecision(3, 2);
            modelBuilder.ComplexType<RedemptionParams>();
            base.OnModelCreating(modelBuilder);
        }

        #region Seeding

        public void Seed()
        {
            AddBrands();
            SaveChanges();

            AddPlayer(new Guid("315E8BBA-41AC-41A0-A3C1-C60E5A9CE960"), "testplayer", "testplayer@gmail.com",
                new Guid("AA477D19-9955-49FB-9E56-CFCD5DA8118F"));
            AddPlayer(new Guid("66AF937B-63D3-4CD4-89C4-7EECAD4FBB05"), "testuser", "testuser@gmail.com",
                new Guid("A5DE54B9-FACD-441D-BA68-972B9F8F5AB2"));
            AddPlayer(new Guid("D568A5E8-56AA-4A0E-96BC-D2FBCBFB5D34"), "lockeduser", "lockeduser@gmail.com",
                new Guid("B2424F62-EF12-4166-808F-ED238B55502A"));
            AddPlayer(new Guid("8D1E5228-CA52-4503-89B8-E65B720C82DE"), "inactiveuser", "inactiveuser@gmail.com",
                new Guid("88CDE313-FFC9-4D5A-AF7D-80B9F3F65DAB"));

            AddGames();
            SaveChanges();
        }

        private void AddGames()
        {
            var gameIdsMock1 = new[]
            {
                new Guid("67277E31-800C-4793-B029-AE8231E4B0FA"),
                new Guid("C17F4D3F-2F99-42A4-A766-4493EFF6DB9F"),
                new Guid("BD8BF6F5-BC5D-4BC6-BD8E-C48E45DD0977"),
                new Guid("B641B4E9-CA08-4443-8FD3-8D1A43727C3E")
            };

            var gameIdsMock2 = new [] 
            {
                new Guid("C18CEBA7-77D8-4E5E-8E1F-F046B7F7544F"),
                new Guid("BDCD4277-4FF7-46EF-B8ED-F4192E51F03C")
            };
            foreach (var gameId in gameIdsMock1)
            {
                Games.AddOrUpdate(game => game.Id, new Core.Bonus.Data.Game
                {
                    Id = gameId,
                    ProductId = new Guid("1814418D-BC00-43B4-AD18-BEBEF6501D7F")
                });
            }

            foreach (var gameId in gameIdsMock2)
            {
                Games.AddOrUpdate(game => game.Id, new Core.Bonus.Data.Game
                {
                    Id = gameId,
                    ProductId = new Guid("18FB823B-435D-42DF-867E-3BA38ED92060")
                });
            }

            Games.AddOrUpdate(game => game.Id, new Core.Bonus.Data.Game
            {
                Id = new Guid("D3D43F80-F7A3-4CC4-9625-4DD442BEDF90"),
                ProductId = new Guid("321B0909-1768-42E2-8BD4-11A28CDAD039")
            });
        }

        private void AddBrands()
        {
            var licenseeId = new Guid("4A557EA9-E6B7-4F1F-AEE5-49E170ADB7E0");
            Brands.AddOrUpdate(brand => brand.Name, new Core.Bonus.Data.Brand
            {
                Id = _brand138Id,
                Name = "138",
                LicenseeId = licenseeId,
                LicenseeName = "Flycow",
                Currencies = new List<Currency>
                {
                    new Currency {Code = "CAD"},
                    new Currency {Code = "CNY"}
                },
                Vips = new List<VipLevel>
                {
                    new VipLevel {Code = "S"},
                    new VipLevel {Code = "B"}
                },
                TimezoneId = "Pacific Standard Time",
                WalletTemplates = new List<WalletTemplate>
                {
                    new WalletTemplate {Id = new Guid("9D366EF4-7AEF-4DFE-80E1-045909DC8EFD")},
                    new WalletTemplate {Id = new Guid("5855385F-8013-48E1-A3E4-B0BC1A49EF33")}
                },
                RiskLevels = new List<RiskLevel>
                {
                    new RiskLevel{ Id = new Guid("5B6EA085-9661-4FA9-8391-54704040FE91"), IsActive = true},
                    new RiskLevel{ Id = new Guid("5B6EA085-9661-4FA9-8391-54704040FE93"), IsActive = false},
                    new RiskLevel{ Id = new Guid("5B6EA085-9661-4FA9-8391-54704040FE95"), IsActive = true}
                }
            });
            Brands.AddOrUpdate(brand => brand.Name, new Core.Bonus.Data.Brand
            {
                Id = _brand831Id,
                Name = "831",
                LicenseeId = licenseeId,
                LicenseeName = "Flycow",
                Currencies = new List<Currency>
                {
                    new Currency {Code = "CAD"},
                    new Currency {Code = "CNY"}
                },
                Vips = new List<VipLevel>
                {
                    new VipLevel {Code = "G"}
                },
                TimezoneId = "Pacific Standard Time",
                WalletTemplates = new List<WalletTemplate>
                {
                    new WalletTemplate {Id = new Guid("237D0F30-C605-4214-A37C-061AEE5FA0F4")}
                },
                RiskLevels = new List<RiskLevel>
                {
                    new RiskLevel{ Id = new Guid("5B6EA085-9661-4FA9-8391-54704040FE92"), IsActive = true},
                    new RiskLevel{ Id = new Guid("5B6EA085-9661-4FA9-8391-54704040FE94"), IsActive = false}
                }
            });
        }

        private void AddPlayer(Guid id, string name, string email, Guid referralId)
        {
            Players.AddOrUpdate(new Core.Bonus.Data.Player
            {
                Id = id,
                Name = name,
                VipLevel = "S",
                CurrencyCode = "CAD",
                DateRegistered = DateTimeOffset.UtcNow,
                PhoneNumber = "123456789",
                Email = email,
                ReferralId = referralId,
                Brand = Brands.Single(b => b.Id == new Guid("00000000-0000-0000-0000-000000000138")),
                Wallets = new List<Wallet>
                {
                    new Wallet {TemplateId = new Guid("9D366EF4-7AEF-4DFE-80E1-045909DC8EFD")},
                    new Wallet {TemplateId = new Guid("5855385F-8013-48E1-A3E4-B0BC1A49EF33")}
                }
            });
        }

        #endregion
    }
}