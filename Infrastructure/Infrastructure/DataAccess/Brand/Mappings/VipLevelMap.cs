using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Core.Brand.Data;

namespace AFT.RegoV2.Infrastructure.DataAccess.Brand.Mappings
{
    public class VipLevelMap : EntityTypeConfiguration<VipLevel>
    {
        public VipLevelMap(string schema)
        {
            ToTable("VipLevel", schema);
            HasKey(x => x.Id);
            Property(x => x.Code).IsRequired().HasMaxLength(20);
            Property(x => x.Name).IsRequired().HasMaxLength(50);
            Property(x => x.Description).IsOptional().HasMaxLength(200);
            Property(x => x.ColorCode).IsOptional().HasMaxLength(7);
            Property(x => x.Status).IsRequired();
        }
    }
}