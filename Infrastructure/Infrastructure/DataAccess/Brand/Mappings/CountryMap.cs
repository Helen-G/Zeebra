using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Core.Brand.Data;

namespace AFT.RegoV2.Infrastructure.DataAccess.Brand.Mappings
{
    public class CountryMap : EntityTypeConfiguration<Country>
    {
        public CountryMap(string schema)
        {
            ToTable("Countries", schema);
            HasKey(c => c.Code);
            Property(p => p.Code).HasMaxLength(2);
        }
    }
}