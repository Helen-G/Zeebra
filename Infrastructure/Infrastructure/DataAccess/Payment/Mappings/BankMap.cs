using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Domain.Payment.Data;
using AFT.RegoV2.Infrastructure.DataAccess.Payment.Migrations;

namespace AFT.RegoV2.Infrastructure.DataAccess.Payment.Mappings
{
    public class BankMap : EntityTypeConfiguration<Bank>
    {
        public BankMap()
        {
            ToTable("Banks", Configuration.Schema);
            HasKey(p => p.Id);
            Property(p => p.BankId);
            Property(p => p.Name);
            Property(p => p.CountryCode);
            Property(p => p.Created);
            Property(p => p.CreatedBy);
            Property(p => p.Remark);
            Property(p => p.BrandId).HasColumnName("Brand_Id");
            HasRequired(p => p.Brand).WithMany().WillCascadeOnDelete(false);
            HasMany(p => p.Accounts).WithRequired(x => x.Bank);
        }
    }
}