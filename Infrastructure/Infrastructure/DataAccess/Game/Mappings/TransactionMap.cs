using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Core.Game.Data;

namespace AFT.RegoV2.Infrastructure.DataAccess.Game.Mappings
{
    public class TransactionMap : EntityTypeConfiguration<Transaction>
    {
        public TransactionMap(string schema)
        {
            ToTable("Transactions", schema);

            Property(x => x.RoundId)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_BetId") { IsUnique = false }));
        }
    }
}
