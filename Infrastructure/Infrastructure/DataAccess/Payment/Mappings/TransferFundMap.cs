using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Domain.Payment.Data;
using AFT.RegoV2.Infrastructure.DataAccess.Payment.Migrations;

namespace AFT.RegoV2.Infrastructure.DataAccess.Payment.Mappings
{
    public class TransferFundMap : EntityTypeConfiguration<TransferFund>
    {
        public TransferFundMap()
        {
            ToTable("TransferFund", Configuration.Schema);
            HasKey(p => p.Id);
            Property(p => p.TransactionNumber);
            Property(p => p.TransferType);
            Property(p => p.WalletId);
            Property(p => p.Status);
            Property(p => p.Amount);
            Property(p => p.Created);
            Property(p => p.CreatedBy);
            Property(p => p.Remarks);
        }
    }
}
