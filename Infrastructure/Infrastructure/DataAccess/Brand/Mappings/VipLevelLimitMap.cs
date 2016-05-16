using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Core.Brand.Data;

namespace AFT.RegoV2.Infrastructure.DataAccess.Brand.Mappings
{
    public class VipLevelLimitMap : EntityTypeConfiguration<VipLevelGameProviderBetLimit>
    {
        public VipLevelLimitMap(string schema)
        {
            ToTable("xref_VipLevelBetLimit", schema);
            HasKey(x => x.Id);
            HasRequired(x => x.VipLevel).WithMany(x => x.VipLevelLimits).WillCascadeOnDelete(false);
            HasRequired(x => x.Currency).WithMany().HasForeignKey(x => x.CurrencyCode);
            Property(x => x.GameProviderId).IsRequired();
            Property(x => x.BetLimitId).IsRequired();
        }
    }
}