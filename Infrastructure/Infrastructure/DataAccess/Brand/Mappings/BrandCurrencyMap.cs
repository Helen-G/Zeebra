using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Core.Brand.Data;

namespace AFT.RegoV2.Infrastructure.DataAccess.Brand.Mappings
{
    public class BrandCurrencyMap : EntityTypeConfiguration<BrandCurrency>
    {
        public BrandCurrencyMap(string schema)
        {
            ToTable("BrandCurrency", schema);
            HasKey(x => new {x.BrandId, x.CurrencyCode});
        }
    }
}