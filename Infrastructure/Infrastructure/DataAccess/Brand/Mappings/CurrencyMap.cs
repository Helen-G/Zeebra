using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Core.Brand.Data;

namespace AFT.RegoV2.Infrastructure.DataAccess.Brand.Mappings
{
    public class CurrencyMap : EntityTypeConfiguration<Currency>
    {
        public CurrencyMap(string schema)
        {
            ToTable("Currencies", schema);
            HasKey(c => c.Code);
            Property(p => p.Code).HasMaxLength(3);
            Property(p => p.Name).HasMaxLength(100).IsRequired(); 
        }
    }
}