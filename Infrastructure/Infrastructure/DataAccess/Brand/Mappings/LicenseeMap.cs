using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Core.Brand.Data;

namespace AFT.RegoV2.Infrastructure.DataAccess.Brand.Mappings
{
    public class LicenseeMap : EntityTypeConfiguration<Licensee>
    {
        public LicenseeMap(string schema)
        {
            ToTable("Licensees", schema);
            HasKey(l => l.Id);
            HasMany(l => l.Cultures).WithMany().Map(x =>
            {
                x.MapLeftKey("LicenseeId");
                x.MapRightKey("CultureCode");
                x.ToTable("xref_LicenseeCultures", schema);
            });
            HasMany(l => l.Currencies).WithMany().Map(x =>
            {
                x.MapLeftKey("LicenseeId");
                x.MapRightKey("CurrencyCode");
                x.ToTable("xref_LicenseeCurrencies", schema);
            });
            HasMany(l => l.Countries).WithMany().Map(x =>
            {
                x.MapLeftKey("LicenseeId");
                x.MapRightKey("CountryCode");
                x.ToTable("xref_LicenseeCountries", schema);
            });
           
        }
    }
}