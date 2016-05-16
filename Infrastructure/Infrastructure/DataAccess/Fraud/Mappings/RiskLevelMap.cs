using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Core.Fraud.Data;
using AFT.RegoV2.Infrastructure.DataAccess.Fraud.Migrations;

namespace AFT.RegoV2.Infrastructure.DataAccess.Fraud.Mappings
{
    public class RiskLevelMap : EntityTypeConfiguration<RiskLevel>
    {
        public RiskLevelMap()
        {
            ToTable("RiskLevels", Configuration.Schema);
            HasKey(x => x.Id);
            HasMany(x => x.AutoVerificationCheckConfigurations);
            Property(x => x.Name).HasMaxLength(50).IsRequired();
            Property(x => x.Description).HasMaxLength(200);
        }
    }
}
