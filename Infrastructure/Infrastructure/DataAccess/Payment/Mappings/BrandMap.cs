using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Infrastructure.DataAccess.Payment.Migrations;

namespace AFT.RegoV2.Infrastructure.DataAccess.Payment.Mappings
{
    public class BrandMap : EntityTypeConfiguration<Domain.Payment.Data.Brand>
    {
        public BrandMap()
        {
            ToTable("Brands", Configuration.Schema);
            HasKey(b => b.Id);
            Property(b => b.Code);
            Property(b => b.Name);
            Property(b => b.LicenseeId);
            Property(b => b.LicenseeName);
            Property(b => b.BaseCurrencyCode);
            HasRequired(o => o.Licensee)
                .WithMany()
                .HasForeignKey(o => o.LicenseeId);
        }
    }
}