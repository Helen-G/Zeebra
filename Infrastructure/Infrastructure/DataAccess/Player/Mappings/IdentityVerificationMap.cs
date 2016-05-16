using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Core.Common.Data;

namespace AFT.RegoV2.Core.Common.Repository.Mappings
{
    public class IdentityVerificationMap : EntityTypeConfiguration<IdentityVerification>
    {
        public IdentityVerificationMap(string schema)
        {
            ToTable("IdentityVerification", schema);
            HasKey(x => x.Id);
            HasRequired(x => x.Player).WithMany().WillCascadeOnDelete(false);
        }
    }
}
