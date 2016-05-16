using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Core.Brand.Data;

namespace AFT.RegoV2.Infrastructure.DataAccess.Brand.Mappings
{
    public class LicenseeProductMap : EntityTypeConfiguration<LicenseeProduct>
    {
        public LicenseeProductMap(string schema)
        {
            ToTable("LicenseeProducts", schema);
            HasKey(x => new { x.LicenseeId, x.ProductId });
            HasRequired(x => x.Licensee).WithMany(x => x.Products).HasForeignKey(x => x.LicenseeId);
        }
    }
}