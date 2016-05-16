using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Core.Game.Data;

namespace AFT.RegoV2.Infrastructure.DataAccess.Game.Mappings
{
    public class LockMap : EntityTypeConfiguration<Lock>
    {
        public LockMap(string schema)
        {
            ToTable("Locks", schema);
        }
    }
}
