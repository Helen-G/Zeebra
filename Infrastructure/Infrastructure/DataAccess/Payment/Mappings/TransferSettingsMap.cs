using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Domain.Payment.Data;
using AFT.RegoV2.Infrastructure.DataAccess.Payment.Migrations;

namespace AFT.RegoV2.Infrastructure.DataAccess.Payment.Mappings
{
    public class TransferSettingsMap : EntityTypeConfiguration<TransferSettings>
    {
        public TransferSettingsMap()
        {
            ToTable("TransferSettings", Configuration.Schema);
            HasKey(pl => pl.Id);
            Property(p => p.BrandId).HasColumnName("Brand_Id");
            HasRequired(pl => pl.Brand).WithMany().WillCascadeOnDelete(false);
            Property(x => x.TransferType);
            Property(p => p.VipLevelId).HasColumnName("VipLevel_Id");
            HasRequired(pl => pl.VipLevel).WithMany().WillCascadeOnDelete(false);
            Property(x => x.CurrencyCode);
            Property(x => x.WalletId);
            Property(x => x.MinAmountPerTransaction);
            Property(x => x.MaxAmountPerTransaction);
            Property(x => x.MaxAmountPerDay);
            Property(x => x.MaxTransactionPerDay);
            Property(x => x.MaxTransactionPerWeek);
            Property(x => x.MaxTransactionPerMonth);
            Property(x => x.Enabled);
            Property(x => x.CreatedBy);
            Property(x => x.CreatedDate);
            Property(x => x.UpdatedBy);
            Property(x => x.UpdatedDate);
            Property(x => x.EnabledBy);
            Property(x => x.EnabledDate);
            Property(x => x.DisabledBy);
            Property(x => x.DisabledDate);
        }
    }
}
