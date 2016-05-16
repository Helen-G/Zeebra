using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Core.Brand.Data;

namespace AFT.RegoV2.Infrastructure.DataAccess.Brand.Mappings
{
    public class RiskLevelMap : EntityTypeConfiguration<RiskLevel>
    {
        public RiskLevelMap(string schema)
        {
            ToTable("RiskLevel", schema);
        }
    }
}