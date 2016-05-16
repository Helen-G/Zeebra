using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Core.Security.Data;

namespace AFT.RegoV2.Core.Services.Security.Repository.Mappings
{
    public class CurrencyCodeMap : EntityTypeConfiguration<CurrencyCode>
    {
        public CurrencyCodeMap(string schema)
        {
            ToTable("UserCurrencies", schema);
            HasKey(b => new { b.UserId, b.Currency });

            HasRequired(b => b.User)
                .WithMany(u => u.Currencies)
                .WillCascadeOnDelete(true);
        }
    }
}
