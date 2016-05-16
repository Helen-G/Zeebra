using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Core.Game.Data;

namespace AFT.RegoV2.Infrastructure.DataAccess.Game.Mappings
{
    public class GameProviderConfigurationsMap : EntityTypeConfiguration<GameProviderConfiguration>
    {
        public GameProviderConfigurationsMap(string schema)
        {
            ToTable("GameProviderConfigurations", schema);

            HasRequired(o => o.GameProvider)
                .WithMany()
                .HasForeignKey(o => o.GameProviderId);
        }
    }
}
