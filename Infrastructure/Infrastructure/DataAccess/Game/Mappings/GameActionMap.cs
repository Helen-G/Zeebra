using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Core.Game.Data;

namespace AFT.RegoV2.Infrastructure.DataAccess.Game.Mappings
{
    public class GameActionMap : EntityTypeConfiguration<Core.Game.Data.GameAction>
    {
        public GameActionMap(string schema)
        {
            ToTable("GameActions", schema);
            HasKey(x => x.Id);

            Property(x => x.ExternalTransactionId)
                .HasMaxLength(50)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_ExternalTransactionId") { IsUnique = false }));

            HasRequired(x => x.Round)
                .WithMany(x => x.GameActions)
                .Map(x => x.MapKey("RoundId"));
        }
    }
}
