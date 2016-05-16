using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Core.Payment.Data;
using AFT.RegoV2.Infrastructure.DataAccess.Payment.Migrations;

namespace AFT.RegoV2.Infrastructure.DataAccess.Payment.Mappings
{
    public class PlayerBankAccountMap : EntityTypeConfiguration<PlayerBankAccount>
    {
        public PlayerBankAccountMap()
        {
            ToTable("PlayerBankAccounts", Configuration.Schema);
            HasKey(p => p.Id);
            Property(p => p.AccountName);
            Property(p => p.AccountNumber);
            Property(p => p.Province);
            Property(p => p.City);
            Property(p => p.Branch);
            Property(p => p.SwiftCode);
            Property(p => p.Address);
            Property(p => p.Remarks);
            Property(p => p.Created);
            Property(p => p.CreatedBy);
            Property(p => p.Status);
            Property(p => p.Updated);
            Property(p => p.UpdatedBy);
            HasRequired(p => p.Bank).WithMany().WillCascadeOnDelete(false);
            HasRequired(p => p.Player).WithMany().WillCascadeOnDelete(false);
        }
    }
}
