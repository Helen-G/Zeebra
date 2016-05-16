using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Domain.Payment.Data;
using AFT.RegoV2.Infrastructure.DataAccess.Payment.Migrations;

namespace AFT.RegoV2.Infrastructure.DataAccess.Payment.Mappings
{
    public class PaymentLevelMap : EntityTypeConfiguration<PaymentLevel>
    {
        public PaymentLevelMap()
        {
            ToTable("PaymentLevel", Configuration.Schema);
            HasKey(pl => pl.Id);
            Property(x => x.CurrencyCode);
            Property(p => p.BrandId).HasColumnName("Brand_Id");
            HasRequired(pl => pl.Brand).WithMany().WillCascadeOnDelete(false);
            HasMany(pl => pl.BankAccounts).WithMany(a => a.PaymentLevels).Map(x =>
            {
                x.MapLeftKey("PaymentLevelId");
                x.MapRightKey("BankAccountId");
                x.ToTable("PaymentLevelBankAccounts", Configuration.Schema);
            });
        }
    }
}