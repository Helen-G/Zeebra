using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Core.Security.Data;

namespace AFT.RegoV2.Core.Services.Security.Repository.Mappings
{
    public class BrandIdMap : EntityTypeConfiguration<BrandId>
    {
        public BrandIdMap(string schema)
        {
            ToTable("UserBrands", schema);
            HasKey(b => new { b.UserId, b.Id });
            
            HasRequired(b => b.User)
                .WithMany(u => u.AllowedBrands)
                .WillCascadeOnDelete(true);
        }
    }
}
