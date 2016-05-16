using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Core.Brand.Data;
using AFT.RegoV2.Core.Common.Brand.Data;

namespace AFT.RegoV2.Infrastructure.DataAccess.Brand.Mappings
{
    public class CultureCodeMap : EntityTypeConfiguration<Culture>
    {
        public CultureCodeMap(string schema)
        {
            ToTable("CultureCodes", schema);
            HasKey(c => c.Code);
            Property(p => p.Code).HasMaxLength(10);
            Property(p => p.Name).HasMaxLength(50).IsRequired();
            Property(p => p.NativeName).HasMaxLength(50).IsRequired();
        }
    }
}