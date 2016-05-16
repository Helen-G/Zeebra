using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Core.Brand.Data;

namespace AFT.RegoV2.Infrastructure.DataAccess.Brand.Mappings
{
    public class BrandCultureMap : EntityTypeConfiguration<BrandCulture>
    {
        public BrandCultureMap(string schema)
        {
            ToTable("BrandCulture", schema);
            HasKey(x => new {x.BrandId, x.CultureCode});
        }
    }
}