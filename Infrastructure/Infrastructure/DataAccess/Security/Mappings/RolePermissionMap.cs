using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Core.Security.Data;
namespace AFT.RegoV2.Infrastructure.DataAccess.Security.Mappings
{
    public class RolePermissionMap : EntityTypeConfiguration<RolePermission>
    {
        public RolePermissionMap(string schema)
        {
            ToTable("RolePermissions", schema);
            HasKey(ro => new { ro.RoleId, ro.PermissionId });

            HasRequired(ro => ro.Role).WithMany(r => r.Permissions).WillCascadeOnDelete(true);
        }
    }
}
