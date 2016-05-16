using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Core.Security.Data;

namespace AFT.RegoV2.Core.Services.Security.Repository.Mappings
{
    public class BrandFilterSelectionMap : EntityTypeConfiguration<BrandFilterSelection>
    {
        public BrandFilterSelectionMap(string schema)
        {
            ToTable("BrandFilterSelections", schema);
            HasKey(b => new { b.UserId, b.BrandId });
            
            HasRequired(b => b.User)
                .WithMany(u => u.BrandFilterSelections)
                .WillCascadeOnDelete(true);
        }
    }
}
