using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Domain.Payment.Data;
using AFT.RegoV2.Infrastructure.DataAccess.Payment.Migrations;

namespace AFT.RegoV2.Infrastructure.DataAccess.Payment.Mappings
{
    public class PaymentGatewayMap : EntityTypeConfiguration<PaymentGateway>
    {
        public PaymentGatewayMap()
        {
            ToTable("PaymentGateway", Configuration.Schema);
            HasKey(pl => pl.Id);
            Property(p => p.PaymentMethod);
            HasOptional(x => x.BankAccount).WithMany().WillCascadeOnDelete(false);
        }
    }
}