using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Domain.Payment.Data;
using AFT.RegoV2.Infrastructure.DataAccess.Payment.Migrations;

namespace AFT.RegoV2.Infrastructure.DataAccess.Payment.Mappings
{
    public class BankAccountMap : EntityTypeConfiguration<BankAccount>
    {
        public BankAccountMap()
        {
            ToTable("BankAccounts", Configuration.Schema);
            HasKey(p => p.Id);
            Property(p => p.AccountId);
            Property(p => p.AccountName);
            Property(p => p.AccountNumber);
            Property(p => p.AccountType);
            Property(p => p.CurrencyCode);
            Property(p => p.Province);
            Property(p => p.Branch);
            Property(p => p.Created);
            Property(p => p.CreatedBy);
            Property(p => p.Status);
            Property(p => p.Updated);
            Property(p => p.UpdatedBy);
            HasRequired(p => p.Bank).WithMany().WillCascadeOnDelete(false);
        }
    }
}
