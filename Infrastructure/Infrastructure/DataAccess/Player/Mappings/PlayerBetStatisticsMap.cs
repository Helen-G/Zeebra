using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Core.Common.Data;

namespace AFT.RegoV2.Core.Common.Repository.Mappings
{
    public class PlayerBetStatisticsMap : EntityTypeConfiguration<PlayerBetStatistics>
    {
        public PlayerBetStatisticsMap(string schema)
        {
            ToTable("PlayerBetStatistics", schema);
            HasKey(p => p.PlayerId);
            Property(p => p.TotalWon);
            Property(p => p.TotalLoss);
            Property(p => p.TotlAdjusted);
        }
    }
}