using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Infrastructure.DataAccess.Fraud.Migrations;

namespace AFT.RegoV2.Infrastructure.DataAccess.Fraud.Mappings
{
    public class BrandMap : EntityTypeConfiguration<Core.Fraud.Data.Brand>
    {
        public BrandMap()
        {
            ToTable("Brands", Configuration.Schema);
            HasKey(x => x.Id);
//            HasMany(x => x.AutoVerificationCheckConfigurations).WithRequired(x => x.Brand).Map(x =>
//            {
//                x.MapKey("Brand_Id");
//            });
            Property(x => x.Name).HasMaxLength(20).IsRequired();
            Property(x => x.Code).HasMaxLength(20).IsRequired();
            Property(x => x.LicenseeName).HasMaxLength(50).IsRequired();
        }
    }
}
