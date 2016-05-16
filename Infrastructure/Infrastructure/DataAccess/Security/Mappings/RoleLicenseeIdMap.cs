using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Core.Security.Data;

namespace AFT.RegoV2.Infrastructure.DataAccess.Security.Mappings
{
    public class RoleLicenseeIdMap : EntityTypeConfiguration<RoleLicenseeId>
    {
        public RoleLicenseeIdMap(string schema)
        {
            ToTable("RoleLicensees", schema);
            HasKey(b => new { b.RoleId, b.Id });
            
            HasRequired(l => l.Role)
                .WithMany(r => r.Licensees)
                .WillCascadeOnDelete(true);
        }
    }
}
