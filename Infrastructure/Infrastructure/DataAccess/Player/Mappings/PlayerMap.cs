using System.Data.Entity.ModelConfiguration;

namespace AFT.RegoV2.Core.Common.Repository.Mappings
{
    public class PlayerMap : EntityTypeConfiguration<Player.Data.Player>
    {
        public PlayerMap(string schema)
        {
            ToTable("Players", schema);
            HasKey(p => p.Id);
            Property(p => p.IpAddress).HasMaxLength(15);
            Property(p => p.DomainName).HasMaxLength(255);
            Property(p => p.Username).HasMaxLength(255);
            Property(p => p.PasswordEncrypted).HasMaxLength(255);
            Property(p => p.FirstName).HasMaxLength(255);
            Property(p => p.LastName).HasMaxLength(255);
            Property(p => p.MailingAddressLine1).HasMaxLength(255);
            Property(p => p.MailingAddressPostalCode).HasMaxLength(10);
            Property(p => p.CountryCode);
            Property(p => p.CurrencyCode);
            Property(p => p.CultureCode);
            Property(p => p.Email).HasMaxLength(255);
            Property(p => p.PhoneNumber).HasMaxLength(20);
            Property(p => p.DateOfBirth);
            Property(p => p.DateRegistered);
            Property(p => p.MailingAddressLine1).HasMaxLength(50);
            Property(p => p.MailingAddressLine2).HasMaxLength(50);
            Property(p => p.MailingAddressLine3).HasMaxLength(50);
            Property(p => p.MailingAddressLine4).HasMaxLength(50);
            Property(p => p.MailingAddressCity).HasMaxLength(50);
            Property(p => p.Comments).HasMaxLength(1500);
            HasMany(p => p.IdentityVerifications).WithRequired(x => x.Player);
        }
    }
}
