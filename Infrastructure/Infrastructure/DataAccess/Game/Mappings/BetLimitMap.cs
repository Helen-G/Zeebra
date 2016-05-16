using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Core.Game.Data;

namespace AFT.RegoV2.Infrastructure.DataAccess.Game.Mappings
{
    public class BetLimitMap : EntityTypeConfiguration<GameProviderBetLimit>
    {
        public BetLimitMap(string schema)
        {
            ToTable("BetLimits", schema);
        }
    }
}