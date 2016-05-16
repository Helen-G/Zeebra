using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Core.Game.Data;

namespace AFT.RegoV2.Infrastructure.DataAccess.Game.Mappings
{
    public class GameProviderMap : EntityTypeConfiguration<GameProvider>
    {
        public GameProviderMap(string schema)
        {
            ToTable("GameProviders", schema);
        }
    }
}
