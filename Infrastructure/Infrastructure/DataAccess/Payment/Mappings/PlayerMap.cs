using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Infrastructure.DataAccess.Payment.Migrations;

namespace AFT.RegoV2.Infrastructure.DataAccess.Payment.Mappings
{
    public class PlayerMap : EntityTypeConfiguration<Core.Payment.Data.Player>
    {
        public PlayerMap()
        {
            ToTable("Players", Configuration.Schema);

            HasKey(p => p.Id);
            Property(p => p.DomainName).HasMaxLength(255);
            Property(p => p.Username).HasMaxLength(255);
            Property(p => p.FirstName).HasMaxLength(255);
            Property(p => p.LastName).HasMaxLength(255);
            Property(p => p.Address).HasMaxLength(255);
            Property(p => p.ZipCode).HasMaxLength(10);
            Property(p => p.Email).HasMaxLength(255);
            Property(p => p.PhoneNumber).HasMaxLength(20);

            HasRequired(o => o.Brand)
                .WithMany()
                .HasForeignKey(o => o.BrandId);

            HasOptional(p => p.CurrentBankAccount)
                .WithOptionalDependent();
        }
    }
}
