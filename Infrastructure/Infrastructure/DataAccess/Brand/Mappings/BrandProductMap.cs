using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Core.Brand.Data;

namespace AFT.RegoV2.Infrastructure.DataAccess.Brand.Mappings
{
    public class BrandProductMap : EntityTypeConfiguration<BrandProduct>
    {
        public BrandProductMap(string schema)
        {
            ToTable("BrandProducts", schema);
            HasKey(x => new { x.BrandId, ProductId = x.ProductId });
            HasRequired(x => x.Brand).WithMany(x => x.Products).HasForeignKey(x => x.BrandId);
        }
    }
}