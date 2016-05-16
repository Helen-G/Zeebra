using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Domain.Payment.Data;
using AFT.RegoV2.Infrastructure.DataAccess.Payment.Migrations;

namespace AFT.RegoV2.Infrastructure.DataAccess.Payment.Mappings
{
    public class PlayerPaymentLevelMap : EntityTypeConfiguration<PlayerPaymentLevel>
    {
        public PlayerPaymentLevelMap()
        {
            ToTable("PlayerPaymentLevel", Configuration.Schema);
            HasKey(x => x.PlayerId);
            HasRequired(x => x.PaymentLevel).WithMany().WillCascadeOnDelete(false);
        }
    }
}