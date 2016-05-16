using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Core.Security.Data;

namespace AFT.RegoV2.Core.Services.Security.Repository.Mappings
{
    public class LicenseeFilterSelectionMap : EntityTypeConfiguration<LicenseeFilterSelection>
    {
        public LicenseeFilterSelectionMap(string schema)
        {
            ToTable("LicenseeFilterSelections", schema);
            HasKey(b => new { b.UserId, b.LicenseeId });
            
            HasRequired(b => b.User)
                .WithMany(u => u.LicenseeFilterSelections)
                .WillCascadeOnDelete(true);
        }
    }
}
