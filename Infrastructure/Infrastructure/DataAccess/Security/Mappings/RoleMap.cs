using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Core.Security.Data;

namespace AFT.RegoV2.Infrastructure.DataAccess.Security.Mappings
{
    public class RoleMap : EntityTypeConfiguration<Role>
    {
        public RoleMap(string schema)
        {
            ToTable("Roles", schema);
            HasKey(r => r.Id);

            Property(r => r.Code).HasMaxLength(50);
            Property(r => r.Name).HasMaxLength(255);
            Property(r => r.Description).IsOptional().HasMaxLength(255);
       
            Property(r => r.CreatedDate);
            Property(r => r.UpdatedDate);

            HasOptional(r => r.UpdatedBy);
            HasOptional(r => r.CreatedBy);
        }
    }
}
