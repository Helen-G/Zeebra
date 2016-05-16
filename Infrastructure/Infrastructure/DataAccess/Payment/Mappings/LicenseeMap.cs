using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Infrastructure.DataAccess.Payment.Migrations;

namespace AFT.RegoV2.Infrastructure.DataAccess.Payment.Mappings
{
    public class LicenseeMap : EntityTypeConfiguration<Domain.Payment.Data.Licensee>
    {
        public LicenseeMap()
        {
            ToTable("Licensees", Configuration.Schema);
            HasKey(b => b.Id);
            Property(b => b.Name);
        }
    }
}
