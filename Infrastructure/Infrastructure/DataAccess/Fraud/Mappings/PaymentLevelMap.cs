using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Core.Fraud.Data;
using AFT.RegoV2.Infrastructure.DataAccess.Fraud.Migrations;

namespace AFT.RegoV2.Infrastructure.DataAccess.Fraud.Mappings
{
    public class PaymentLevelMap : EntityTypeConfiguration<PaymentLevel>
    {
        public PaymentLevelMap()
        {
            ToTable("PaymentLevels", Configuration.Schema);
            HasKey(x => x.Id);
            HasMany(x => x.AutoVerificationCheckConfigurations);
            Property(x => x.Name).HasMaxLength(50).IsRequired();
            Property(x => x.Code).HasMaxLength(3);
        }
    }
}
