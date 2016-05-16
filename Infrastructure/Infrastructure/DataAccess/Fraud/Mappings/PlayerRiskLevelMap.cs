using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Core.Fraud.Data;
using AFT.RegoV2.Infrastructure.DataAccess.Player;

namespace AFT.RegoV2.Infrastructure.DataAccess.Fraud.Mappings
{
    // https://msdn.microsoft.com/en-gb/data/jj591617.aspx
    // https://msdn.microsoft.com/en-gb/data/jj591620.aspx
    public class PlayerRiskLevelMap : EntityTypeConfiguration<PlayerRiskLevel>
    {
        public PlayerRiskLevelMap()
        {
            ToTable("RiskLevels", PlayerRepository.Schema);
            HasKey(x => x.Id);

            Property(p => p.PlayerId)
                .IsRequired();

            Property(p => p.RiskLevelId)
                .IsRequired();

            Property(x => x.Description)
                .HasMaxLength(200);

        }
    }
}
