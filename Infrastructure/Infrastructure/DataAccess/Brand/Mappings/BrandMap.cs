using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Core.Brand.Data;

namespace AFT.RegoV2.Infrastructure.DataAccess.Brand.Mappings
{
    public class BrandMap : EntityTypeConfiguration<Core.Brand.Data.Brand>
    {
        public BrandMap(string schema)
        {
            ToTable("Brands", schema);

            HasKey(b => b.Id);

            HasMany(b => b.WalletTemplates);

            HasRequired(b => b.Licensee)
                .WithMany(l => l.Brands)
                .WillCascadeOnDelete(false);

            HasMany(b => b.BrandCultures)
                .WithRequired(bc => bc.Brand)
                .WillCascadeOnDelete(false);

            HasMany(b => b.BrandCurrencies)
                .WithRequired(bc => bc.Brand)
                .WillCascadeOnDelete(false);

            HasMany(b => b.BrandCountries)
                .WithRequired(bc => bc.Brand)
                .WillCascadeOnDelete(false);

            HasOptional(b => b.DefaultVipLevel)
                .WithMany()
                .HasForeignKey(o => o.DefaultVipLevelId);

            HasMany(b => b.VipLevels)
                .WithRequired(o => o.Brand)
                .HasForeignKey(o => o.BrandId);
        }
    }
}