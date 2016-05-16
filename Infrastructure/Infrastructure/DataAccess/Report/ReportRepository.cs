using System;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;
using AFT.RegoV2.Core.Brand.Data;
using AFT.RegoV2.BoundedContexts.Report;
using AFT.RegoV2.BoundedContexts.Report.Data;
using AFT.RegoV2.Core.Common.Data.Brand;
using AFT.RegoV2.Core.Common.Events.Brand;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Player.Data;
using AFT.RegoV2.Core.Report.Data.Admin;
using AFT.RegoV2.Core.Report.Data.Brand;

namespace AFT.RegoV2.Infrastructure.DataAccess.Report
{
    public class ReportRepository : DbContext, IReportRepository, ISeedable
    {
        public const string Schema = "report";

        static ReportRepository()
        {
            Database.SetInitializer(new ReportRepositoryInitializer());
        }

        public ReportRepository()
            : base("name=Default")
        {
        }

        public void Initialize()
        {
            Database.Initialize(false);
        }

        // Admin
        public IDbSet<AdminActivityLog> AdminActivityLog { get; set; }
        public IDbSet<AdminAuthenticationLog> AdminAuthenticationLog { get; set; }
        public IDbSet<MemberAuthenticationLog> MemberAuthenticationLog { get; set; }

        // Player Reports
        public IDbSet<PlayerRecord> PlayerRecords { get; set; }
        public IDbSet<PlayerBetHistoryRecord> PlayerBetHistoryRecords { get; set; }

        // Payment Reports
        public IDbSet<DepositRecord> DepositRecords { get; set; }

        // Brand Reports
        public IDbSet<BrandRecord> BrandRecords { get; set; }
        public IDbSet<LicenseeRecord> LicenseeRecords { get; set; }
        public IDbSet<LanguageRecord> LanguageRecords { get; set; }
        public IDbSet<VipLevelRecord> VipLevelRecords { get; set; }

        // Transaction Reports
        public IDbSet<PlayerTransactionRecord> PlayerTransactionRecords { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Admin
            modelBuilder.Entity<AdminActivityLog>().ToTable("AdminActivityLogs", Schema);
            modelBuilder.Entity<AdminAuthenticationLog>().ToTable("AdminAuthenticationLog", Schema);
            modelBuilder.Entity<MemberAuthenticationLog>().ToTable("MemberAuthenticationLog", Schema);

            // Player Reports
            modelBuilder.Entity<PlayerRecord>().ToTable("Players", Schema);
            modelBuilder.Entity<PlayerBetHistoryRecord>().ToTable("Bets", Schema);

            // Payment Reports
            modelBuilder.Entity<DepositRecord>().ToTable("Deposits", Schema);

            // Transaction Reports
            modelBuilder.Entity<PlayerTransactionRecord>().ToTable("PlayerTransactions", Schema);

            // Brand Reports
            modelBuilder.Entity<BrandRecord>().ToTable("Brands", Schema);
            modelBuilder.Entity<LicenseeRecord>().ToTable("Licensees", Schema);
            modelBuilder.Entity<LanguageRecord>().ToTable("Languages", Schema);
            modelBuilder.Entity<VipLevelRecord>().ToTable("VipLevels", Schema);
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

        public void Seed()
        {
            const string licensee = "Flycow";
            var brand138Id = new Guid("00000000-0000-0000-0000-000000000138");
            const string brand138 = "138";

            var brand831Id = new Guid("D2CFDF01-FB79-4365-9E47-FC8E4DC1349C");
            const string brand831 = "831";


            var vipLevelId831Default = new Guid("30E9988C-AFED-49A0-BE6B-AD60F7A50BEB");
            var vipLevelId138Default = new Guid("0447E567-BDC6-4330-979C-5E0984BFB626");

            AddBrand(licensee, brand138, brand138Id, vipLevelId138Default);
            AddBrand(licensee, brand831, brand831Id, vipLevelId831Default);


            AddVipLevel("G", 0, licensee, brand831, vipLevelId831Default);
            //var brand831Record = BrandRecords.Local.Single(o => o.BrandId == brand831Id);
            //brand831Record.DefaultVipLevelId = vipLevelId831Default;

            AddVipLevel("S", 1, licensee, brand138, vipLevelId138Default);
            //var brand138Record = BrandRecords.Local.Single(o => o.BrandId == brand138Id);
            //brand138Record.DefaultVipLevelId = vipLevelId138Default;

            AddVipLevel("B", 2, licensee, brand138, new Guid("541F60EF-AEE7-408B-9B39-90289D49F6AD"));

            AddPlayer(new Guid("315E8BBA-41AC-41A0-A3C1-C60E5A9CE960"), "testplayer", "Test Player", "testplayer@gmail.com", AccountStatus.Active, licensee, brand138);
            AddPlayer(new Guid("66AF937B-63D3-4CD4-89C4-7EECAD4FBB05"), "testuser", "Test User", "testuser@gmail.com", AccountStatus.Active, licensee, brand138);
            AddPlayer(new Guid("D568A5E8-56AA-4A0E-96BC-D2FBCBFB5D34"), "lockeduser", "Locked User", "lockeduser@gmail.com", AccountStatus.Locked, licensee, brand138);
            AddPlayer(new Guid("8D1E5228-CA52-4503-89B8-E65B720C82DE"), "inactiveuser", "Inactive User", "inactiveuser@gmail.com", AccountStatus.Inactive, licensee, brand138);

            SaveChanges();
        }

        private void AddBrand(string licensee, string brand, Guid brandId, Guid defaultVipLevelId)
        {
            if (BrandRecords.Any(r => r.BrandId == brandId))
            {
                return;
            }
            BrandRecords.Add(new BrandRecord
            {
                BrandId = brandId,
                Licensee = licensee,
                BrandCode = brand,
                Brand = brand,
                BrandType = BrandType.Deposit.ToString(),
                BrandStatus = BrandStatus.Active.ToString(),
                BrandTimeZone = "(UTC-08:00) Pacific Time (US & Canada)",
                Created = new DateTimeOffset(),
                Activated = new DateTimeOffset(),
                DefaultVipLevelId = defaultVipLevelId
            });
        }

        private void AddVipLevel(string code, int rank, string licensee, string brand, Guid vipLevelId)
        {
            if (VipLevelRecords.Any(r => r.VipLevelId == vipLevelId))
            {
                return;
            }
            VipLevelRecords.Add(new VipLevelRecord
            {
                Id = Guid.NewGuid(),
                VipLevelId = vipLevelId,
                Licensee = licensee,
                Brand = brand,
                Code = code,
                Rank = rank,
                Status = VipLevelStatus.Active.ToString(),
                Created = new DateTimeOffset(),
                Activated = new DateTimeOffset()
            });
        }

        private void AddPlayer(
            Guid id,
            string username,
            string name,
            string email,
            AccountStatus status,
            string licensee,
            string brand)
        {
            PlayerRecords.AddOrUpdate(new PlayerRecord
            {
                PlayerId = id,
                Licensee = licensee,
                Brand = brand,
                Username = username,
                Mobile = "123456789",
                Email = email,
                Birthday = DateTimeOffset.Parse("1990-01-01 00:00:00.0000000 +02:00"),
                RegistrationDate = DateTimeOffset.UtcNow,
                PlayerStatus = status.ToString(),
                Language = "English US",
                Currency = "CAD",
                VipLevel = "S",
                Country = "Canada",
                PlayerName = name,
                Title = "Mr",
                Gender = "Male",
                StreetAddress = "305-1250 Homer Street",
                PostCode = "V6B1C6",
                SignUpIP = "127.0.0.1",
                Created = DateTimeOffset.UtcNow,
                Activated = DateTimeOffset.UtcNow
            });
        }
    }
}
