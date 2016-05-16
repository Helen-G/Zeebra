using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Core.Payment.Data;
using AFT.RegoV2.Infrastructure.DataAccess.Payment.Migrations;

namespace AFT.RegoV2.Infrastructure.DataAccess.Payment.Mappings
{
    public class CurrencyMap : EntityTypeConfiguration<Currency>
    {
        public CurrencyMap()
        {
            ToTable("Currencies", Configuration.Schema);
            HasKey(c => c.Code);
            Property(p => p.Code).HasMaxLength(3);
            Property(p => p.Name).HasMaxLength(100).IsRequired();
        }
    }
}
