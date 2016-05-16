using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Core.Security.Data;

namespace AFT.RegoV2.Infrastructure.DataAccess.Security.Mappings
{
    public class UserLicenseeIdMap : EntityTypeConfiguration<UserLicenseeId>
    {
        public UserLicenseeIdMap(string schema)
        {
            ToTable("UserLicensees", schema);
            HasKey(b => new { b.UserId, b.Id });

            HasRequired(l => l.User)
                .WithMany(u => u.Licensees)
                .WillCascadeOnDelete(true);
        }
    }
}
