using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Core.Payment.Data;
using AFT.RegoV2.Domain.Payment.Data;
using AFT.RegoV2.Infrastructure.DataAccess.Payment.Migrations;

namespace AFT.RegoV2.Infrastructure.DataAccess.Payment.Mappings
{
    public class CurrencyExchangeMap : EntityTypeConfiguration<CurrencyExchange>
    {
        public CurrencyExchangeMap()
        {
            ToTable("CurrencyExchange", Configuration.Schema);
            HasKey(x => new { x.BrandId, x.CurrencyToCode });
        }
    }
}
