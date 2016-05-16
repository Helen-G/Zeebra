using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Core.Brand.Data;

namespace AFT.RegoV2.Infrastructure.DataAccess.Brand.Mappings
{
    public class BrandCountryMap : EntityTypeConfiguration<BrandCountry>
    {
        public BrandCountryMap(string schema)
        {
            ToTable("BrandCountry", schema);
            HasKey(x => new { x.BrandId, x.CountryCode });
        }
    }
}