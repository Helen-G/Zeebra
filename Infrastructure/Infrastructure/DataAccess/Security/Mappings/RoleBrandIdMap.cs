using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Domain.BoundedContexts.Security.Data;

namespace AFT.RegoV2.Core.Services.Security.Repository.Mappings
{
    public class RoleBrandIdMap : EntityTypeConfiguration<RoleBrandId>
    {
        public RoleBrandIdMap(string schema)
        {
            ToTable("RoleBrands", schema);
            HasKey(b => new { b.RoleId, b.BrandId });

            HasRequired(b => b.Role);
        }
    }
}
