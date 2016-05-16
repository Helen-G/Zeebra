using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Core.Brand.Data;

namespace AFT.RegoV2.Infrastructure.DataAccess.Brand.Mappings
{
    public class WalletTemplateMap : EntityTypeConfiguration<WalletTemplate>
    {
        public WalletTemplateMap(string schema)
        {
            ToTable("WalletTemplates", schema);
            HasKey(p => p.Id);
            HasRequired(x => x.Brand);
            HasMany(l => l.WalletTemplateProducts);
        }
    }
}