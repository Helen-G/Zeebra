using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Player.Data;

namespace AFT.RegoV2.Core.Common.Repository.Mappings
{
    public class VipLevelMap : EntityTypeConfiguration<VipLevel>
    {
        public VipLevelMap(string schema)
        {
            ToTable("VipLevels", schema);
        }
    }
}
