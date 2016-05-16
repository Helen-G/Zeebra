using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Domain.Payment.Data;
using AFT.RegoV2.Infrastructure.DataAccess.Payment.Migrations;

namespace AFT.RegoV2.Infrastructure.DataAccess.Payment.Mappings
{
    public class OfflineDepositMap : EntityTypeConfiguration<OfflineDeposit>
    {
        public OfflineDepositMap()
        {
            ToTable("OfflineDeposits", Configuration.Schema);
            HasKey(p => p.Id);
            Property(p => p.PlayerId).HasColumnName("Player_Id");
            HasRequired(p => p.Player).WithMany().WillCascadeOnDelete(false);
            Property(p => p.BrandId).HasColumnName("Brand_Id");
            HasRequired(p => p.Brand).WithMany().WillCascadeOnDelete(false);
            Property(p => p.BankAccountId).HasColumnName("BankAccount_Id");
            HasRequired(p => p.BankAccount).WithMany().WillCascadeOnDelete(false);
            Property(p => p.CurrencyCode);
            Property(p => p.TransactionNumber);
            Property(p => p.Created);
            Property(p => p.CreatedBy);
            Property(p => p.Verified);
            Property(p => p.VerifiedBy);
            Property(p => p.Approved);
            Property(p => p.ApprovedBy);
            Property(p => p.NotificationMethod);
            Property(p => p.Status);
            Property(p => p.PlayerAccountName);
            Property(p => p.PlayerAccountNumber);
            Property(p => p.ReferenceNumber);
            Property(p => p.Amount);
            Property(p => p.ActualAmount);
            Property(p => p.Fee);
            Property(p => p.PaymentMethod);
            Property(p => p.TransferType);
            Property(p => p.DepositMethod);
            Property(p => p.DepositType);
            Property(p => p.IdFrontImage);
            Property(p => p.IdBackImage);
            Property(p => p.ReceiptImage);
            Property(p => p.Remark);
            Property(p => p.PlayerRemark);
        }
    }
}
