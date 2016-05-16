using System;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Common.Repository.Mappings;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Player;
using AFT.RegoV2.Core.Player.Data;
using AFT.RegoV2.Infrastructure.DataAccess.Player.Repository;

namespace AFT.RegoV2.Infrastructure.DataAccess.Player
{
    public class PlayerRepository : DbContext, IPlayerRepository, ISeedable
    {
        public const string Schema = "player";

        static PlayerRepository()
        {
            Database.SetInitializer(new PlayerRepositoryInitializer());
        }

        public PlayerRepository()
            : base("name=Default")
        {
        }

        public IDbSet<Core.Player.Data.Player> Players { get; set; }
        public IDbSet<PlayerBetStatistics> PlayerBetStatistics { get; set; }
        public IDbSet<VipLevel> VipLevels { get; set; }
        public IDbSet<SecurityQuestion> SecurityQuestions { get; set; }
        public IDbSet<PlayerActivityLog> PlayerActivityLog { get; set; }
        public IDbSet<PlayerInfoLog> PlayerInfoLog { get; set; }
        public IDbSet<Core.Player.Data.Brand> Brands { get; set; }
        public IDbSet<IdentificationDocumentSettings> IdentificationDocumentSettings { get; set; }
        public IDbSet<IdentityVerification> IdentityVerifications { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Configurations.Add(new PlayerMap(Schema));
            modelBuilder.Configurations.Add(new VipLevelMap(Schema));
            modelBuilder.Configurations.Add(new PlayerBetStatisticsMap(Schema));
            modelBuilder.Configurations.Add(new IdentityVerificationMap(Schema));
            modelBuilder.Configurations.Add(new IdentificationDocumentSettingsMap(Schema));
            modelBuilder.Entity<PlayerActivityLog>().ToTable("PlayerActivityLog", Schema);
            modelBuilder.Entity<PlayerInfoLog>().ToTable("PlayerInfoLog", Schema);
            modelBuilder.Entity<SecurityQuestion>().ToTable("SecurityQuestions", Schema);
            modelBuilder.Entity<Core.Player.Data.Brand>().ToTable("Brands", Schema);
            modelBuilder.Entity<IdentificationDocumentSettings>().ToTable("IdentificationDocumentSettings", Schema);
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
            var brand138Id = new Guid("00000000-0000-0000-0000-000000000138");
            var brand831Id = new Guid("D2CFDF01-FB79-4365-9E47-FC8E4DC1349C");

            var brand138 = new Core.Player.Data.Brand { Id = brand138Id, Name = "138" };
            Brands.AddOrUpdate(brand138);
            Brands.AddOrUpdate(new Core.Player.Data.Brand { Id = brand831Id, Name = "831" });
            SaveChanges();

            AddVipLevel(new Guid("30e9988c-afed-49a0-be6b-ad60f7a50beb"), brand831Id, "G", "Gold", "High Roller", "#fad165");
            AddVipLevel(new Guid("0447e567-bdc6-4330-979c-5e0984bfb626"), brand138Id, "S", "Silver", "Baller", "#cabdbf");
            AddVipLevel(new Guid("541F60EF-AEE7-408B-9B39-90289D49F6AD"), brand138Id, "B", "Bronze", "Some description", "#d06b64");

            brand138 = Brands.Single(x => x.Id == brand138.Id);
            brand138.DefaultVipLevelId = new Guid("0447e567-bdc6-4330-979c-5e0984bfb626");
            SaveChanges();

            AddPlayer(new Guid("315E8BBA-41AC-41A0-A3C1-C60E5A9CE960"), "testplayer", "Test", "Player", "testplayer@gmail.com", AccountStatus.Active, "CD-C4-1F-93-34-4A-6E-86-D5-D9-03-91-06-78-C9-D4");
            AddPlayer(new Guid("66AF937B-63D3-4CD4-89C4-7EECAD4FBB05"), "testuser", "Test", "User", "testuser@gmail.com", AccountStatus.Active, "6F-DF-EB-60-CD-D2-00-CA-95-EB-9B-FF-2A-61-99-F7");
            AddPlayer(new Guid("D568A5E8-56AA-4A0E-96BC-D2FBCBFB5D34"), "lockeduser", "Locked", "User", "lockeduser@gmail.com", AccountStatus.Locked, "21-FD-65-C4-F8-09-DA-BF-2B-8F-DB-51-A1-16-C9-12");
            AddPlayer(new Guid("8D1E5228-CA52-4503-89B8-E65B720C82DE"), "inactiveuser", "Inactive", "User", "inactiveuser@gmail.com", AccountStatus.Inactive, "D6-B1-0C-DA-36-03-E4-79-96-34-A0-F2-F8-BE-29-1A");

            InitSecurityQuestionSeedData();
        }


        private void AddPlayer(
            Guid id,
            string username,
            string firstName,
            string lastName,
            string email,
            AccountStatus accountStatus,
            string password)
        {
            if (Players.Any(x => x.Id == id))
                return;

            Players.AddOrUpdate(new Core.Player.Data.Player
            {
                Id = id,
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                PhoneNumber = "123456789",
                MailingAddressLine1 = "305-1250 Homer Street",
                MailingAddressCity = "Vancouver",
                MailingAddressPostalCode = "V6B1C6",
                PhysicalAddressLine1 = "305-1250 Homer Street",
                PhysicalAddressCity = "Vancouver",
                PhysicalAddressPostalCode = "V6B1C6",
                CountryCode = "CA",
                CurrencyCode = "CAD",
                CultureCode = "en-US",
                Username = username,
                PasswordEncrypted = password, //123456
                DateOfBirth = new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                DateRegistered = DateTimeOffset.UtcNow,
                BrandId = new Guid("00000000-0000-0000-0000-000000000138"),
                PaymentLevelId = new Guid("E1E600D4-0729-4D5C-B93E-085A94B55B33"),
                VipLevel = VipLevels.Single(x => x.Code == "S"),
                AccountStatus = accountStatus,
                AccountAlertEmail = true,
                AccountAlertSms = true,
                MarketingAlertEmail = true,
                MarketingAlertPhone = true,
                MarketingAlertSms = true
            });

            SaveChanges();
        }

        private void InitSecurityQuestionSeedData()
        {
            SecurityQuestions.AddOrUpdate(a => a.Id, new SecurityQuestion { Id = new Guid("96569808-4744-4bf2-952c-86b1a634bb67"), Question = "What is your First Pet" });
            SecurityQuestions.AddOrUpdate(a => a.Id, new SecurityQuestion { Id = new Guid("46eb056d-72ae-4b89-bccb-f4ddf893c535"), Question = "What is First Street Name" });
            SecurityQuestions.AddOrUpdate(a => a.Id, new SecurityQuestion { Id = new Guid("b355b02c-4de4-4981-9a09-5de74bfc5765"), Question = "What is your Mother's Maiden Name" });
            SecurityQuestions.AddOrUpdate(a => a.Id, new SecurityQuestion { Id = new Guid("ff621262-172b-40b7-831b-56dffe66af0b"), Question = "What is your Sister in law's Name" });
            SecurityQuestions.AddOrUpdate(a => a.Id, new SecurityQuestion { Id = new Guid("3be83483-8345-44bf-a0d6-7bebd61daf8d"), Question = "What is the Colour of your first car" });
            SecurityQuestions.AddOrUpdate(a => a.Id, new SecurityQuestion { Id = new Guid("a59635c7-523d-4c74-b456-483eeb458b6d"), Question = "What was your childhood nickname?" });
            /*
            SecurityQuestions.AddOrUpdate(a => a.Id, new SecurityQuestion { Id = new Guid("d4c53657-c4f9-4786-b7a2-8c841286555f"), Question = "In what city did you meet your spouse/significant other?" });
            SecurityQuestions.AddOrUpdate(a => a.Id, new SecurityQuestion { Id = new Guid("2d42e434-8578-4118-b4aa-c17e921812d4"), Question = "What is the name of your favorite childhood friend?" });
            SecurityQuestions.AddOrUpdate(a => a.Id, new SecurityQuestion { Id = new Guid("36a53000-6b96-4fa3-b11c-35eff2bc4d20"), Question = "What street did you live on in third grade?" });
            SecurityQuestions.AddOrUpdate(a => a.Id, new SecurityQuestion { Id = new Guid("0b6b89ea-ef52-4f9a-982e-fbbd7b1fb6f9"), Question = "What is your oldest sibling¡¯s birthday month and year? (e.g., January 1900)" });
            SecurityQuestions.AddOrUpdate(a => a.Id, new SecurityQuestion { Id = new Guid("baba9c92-7a1c-4185-85b6-0956a8f4ac61"), Question = "What is the middle name of your oldest child?" });
            SecurityQuestions.AddOrUpdate(a => a.Id, new SecurityQuestion { Id = new Guid("b7f32b57-c933-4931-b88e-9e96e4506f13"), Question = "What is your oldest sibling's middle name?" });
            SecurityQuestions.AddOrUpdate(a => a.Id, new SecurityQuestion { Id = new Guid("c1e23243-301f-4e92-973e-c2398816899c"), Question = "What school did you attend for sixth grade?" });
            SecurityQuestions.AddOrUpdate(a => a.Id, new SecurityQuestion { Id = new Guid("31f26887-290d-45ed-a55c-c63594b2a241"), Question = "What was your childhood phone number including area code? (e.g., 000-000-0000)" });
            SecurityQuestions.AddOrUpdate(a => a.Id, new SecurityQuestion { Id = new Guid("30c6232a-fb05-493b-9267-f0d633bae2d2"), Question = "What is your oldest cousin's first and last name?" });
            SecurityQuestions.AddOrUpdate(a => a.Id, new SecurityQuestion { Id = new Guid("616b7039-23b5-4f50-a4a0-01160d842ff3"), Question = "What was the name of your first stuffed animal?" });
            SecurityQuestions.AddOrUpdate(a => a.Id, new SecurityQuestion { Id = new Guid("bd979574-0664-4c59-9c1c-31dd1bfae0ff"), Question = "In what city or town did your mother and father meet?" });
            SecurityQuestions.AddOrUpdate(a => a.Id, new SecurityQuestion { Id = new Guid("e8c940a0-26a7-4687-a099-0e753a68160c"), Question = "Where were you when you had your first kiss?" });
            SecurityQuestions.AddOrUpdate(a => a.Id, new SecurityQuestion { Id = new Guid("6995910f-2e08-4fe6-b15e-7dab6df730dd"), Question = "What is the first name of the boy or girl that you first kissed?" });
            SecurityQuestions.AddOrUpdate(a => a.Id, new SecurityQuestion { Id = new Guid("c5a4d536-1a11-4873-b98d-35904d525127"), Question = "What was the last name of your third grade teacher?" });
            SecurityQuestions.AddOrUpdate(a => a.Id, new SecurityQuestion { Id = new Guid("7d4e2916-1af0-4459-9951-908e2b8c672c"), Question = "In what city does your nearest sibling live?" });
            SecurityQuestions.AddOrUpdate(a => a.Id, new SecurityQuestion { Id = new Guid("af00d333-9461-48e5-bd2e-156060ebdb2b"), Question = "What is your oldest brother¡¯s birthday month and year? (e.g., January 1900)" });
            SecurityQuestions.AddOrUpdate(a => a.Id, new SecurityQuestion { Id = new Guid("0c899f88-de29-450c-bc17-40649f7e37f8"), Question = "What is your maternal grandmother's maiden name?" });
            SecurityQuestions.AddOrUpdate(a => a.Id, new SecurityQuestion { Id = new Guid("c81c43ff-f576-4a22-9451-b52e79010d86"), Question = "In what city or town was your first job?" });
            SecurityQuestions.AddOrUpdate(a => a.Id, new SecurityQuestion { Id = new Guid("1c848cff-9822-45d0-861c-cb62e0785fcf"), Question = "What is the name of the place your wedding reception was held?" });
            SecurityQuestions.AddOrUpdate(a => a.Id, new SecurityQuestion { Id = new Guid("cffd92f3-b904-4514-8a49-778019f22a63"), Question = "What is the name of a college you applied to but didn't attend?" });
            SecurityQuestions.AddOrUpdate(a => a.Id, new SecurityQuestion { Id = new Guid("7ce19eb6-9019-4787-aafe-0b7ffe15cd3b"), Question = "Where were you when you first heard about 9/11?" });
            */
            SaveChanges();
        }



        private void AddVipLevel(Guid id, Guid brandId, string code, string name, string description, string colorCode)
        {
            if (VipLevels.Any(v => v.Id == id))
                return;

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                VipLevels.AddOrUpdate(x => x.Id, new VipLevel
                {
                    Id = id,
                    BrandId = brandId,
                    Code = code,
                    Name = name,
                    Description = description,
                    ColorCode = colorCode,
                });

                SaveChanges();

                scope.Complete();
            }
        }
    }
}
