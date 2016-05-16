using System;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Content;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Content;
using AFT.RegoV2.Core.Content.Data;
using AFT.RegoV2.Infrastructure.DataAccess.Content.Mappings;

namespace AFT.RegoV2.Infrastructure.DataAccess.Content
{
    public class ContentRepository : DbContext, IContentRepository, ISeedable
    {
        public const string Schema = "content";

        public IDbSet<MessageTemplate> MessageTemplates { get; set; }
        public IDbSet<Core.Content.Data.Brand> Brands { get; set; }
        public IDbSet<Language> Languages { get; set; }
        public IDbSet<Core.Content.Data.Player> Players { get; set; }

        static ContentRepository()
        {
            Database.SetInitializer(new ContentRepositoryInitializer());
        }

        public ContentRepository() : base("name=Default") { }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Configurations.Add(new MessageTemplateMap(Schema));
            modelBuilder.Configurations.Add(new LanguageMap(Schema));
            modelBuilder.Configurations.Add(new BrandMap(Schema));
            modelBuilder.Configurations.Add(new PlayerMap(Schema));
        }

        public MessageTemplate FindMessageTemplateById(Guid id)
        {
            return MessageTemplates.FirstOrDefault(x => x.Id == id);
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
            SeedCultures();
            SeedBrands();
            SeedPlayers();
            SeedMessageTemplates();
        }

        private void SeedCultures()
        {
            Languages.AddOrUpdate(new Language {Code = "en-GB", Name = "English UK"});
            Languages.AddOrUpdate(new Language {Code = "en-US", Name = "English US"});
            Languages.AddOrUpdate(new Language { Code = "zh-CN", Name = "Chinese Simplified" });
            Languages.AddOrUpdate(new Language { Code = "zh-TW", Name = "Chinese Traditional" });
            
            SaveChanges();
        }

        private void SeedBrands()
        {
            var cultures = Languages.Where(x => x.Code == "en-US" || x.Code == "zh-TW").ToList();

            Brands.AddOrUpdate(new Core.Content.Data.Brand
            {
                Id = new Guid("00000000-0000-0000-0000-000000000138"), 
                Name = "138",
                Languages = cultures
            });

            Brands.AddOrUpdate(new Core.Content.Data.Brand
            {
                Id = new Guid("D2CFDF01-FB79-4365-9E47-FC8E4DC1349C"),
                Name = "831",
                Languages = cultures
            });

            SaveChanges();
        }

        private void SeedPlayers()
        {
            SeedPlayer(new Guid("66AF937B-63D3-4CD4-89C4-7EECAD4FBB05"), "testuser", "Test", "User", "testuser@gmail.com", "en-US", new Guid("00000000-0000-0000-0000-000000000138"));
            SeedPlayer(new Guid("315E8BBA-41AC-41A0-A3C1-C60E5A9CE960"), "testplayer", "Test", "Player", "testplayer@gmail.com", "en-US", new Guid("00000000-0000-0000-0000-000000000138"));
            SeedPlayer(new Guid("D568A5E8-56AA-4A0E-96BC-D2FBCBFB5D34"), "lockeduser", "Locked", "User", "lockeduser@gmail.com", "en-US", new Guid("00000000-0000-0000-0000-000000000138"));
            SeedPlayer(new Guid("8D1E5228-CA52-4503-89B8-E65B720C82DE"), "inactiveuser", "Inactive", "User", "inactiveuser@gmail.com", "en-US", new Guid("00000000-0000-0000-0000-000000000138"));

            SaveChanges();
        }

        private void SeedPlayer(
            Guid id, 
            string username, 
            string firstName, 
            string lastName, 
            string email,
            string cultureCode, 
            Guid brandId)
        {
            Players.AddOrUpdate(new Core.Content.Data.Player
            {
                Id = id,
                Username = username,
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                LanguageCode = cultureCode,
                BrandId = brandId
            });
        }

        private void SeedMessageTemplates()
        {
            MessageTemplates.AddOrUpdate(t => t.TemplateName, new MessageTemplate
            {
                Id = Guid.NewGuid(),
                BrandId = new Guid("00000000-0000-0000-0000-000000000138"),
                LanguageCode = "en-US",
                MessageType = MessageType.PlayerRegistered,
                MessageDeliveryMethod = MessageDeliveryMethod.Email,
                TemplateName = "Default Player Registered Email",
                MessageContent = @"Hello, {{model.Username}}. You are now registered with {{model.BrandName}}",
                SenderName = "138",
                SenderEmail = "no-reply@138.com",
                Subject = "138 Registration",
                Status = Status.Active,
                Created = DateTimeOffset.UtcNow,
                CreatedBy = "System",
                Activated = DateTimeOffset.UtcNow,
                ActivatedBy = "System"
            });

            SaveChanges();
        }
    }
}