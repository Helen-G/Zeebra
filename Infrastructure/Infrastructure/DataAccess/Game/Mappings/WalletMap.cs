using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Core.Game.Data;

namespace AFT.RegoV2.Infrastructure.DataAccess.Game.Mappings
{
    public class WalletMap : EntityTypeConfiguration<Wallet>
    {
        public WalletMap(string schema)
        {
            ToTable("Wallets", schema);

            Property(x => x.PlayerId)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_PlayerId") { IsUnique = false }));
        }
    }
}
