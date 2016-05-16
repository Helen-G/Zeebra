using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Core.Brand.Data;

namespace AFT.RegoV2.Infrastructure.DataAccess.Brand.Mappings
{
    public class WalletTemplateProductMap : EntityTypeConfiguration<WalletTemplateProduct>
    {
        public WalletTemplateProductMap(string schema)
        {
            ToTable("WalletTemplateProducts", schema);
            HasKey(p => p.Id);
            HasRequired(x => x.WalletTemplate);
        }
    }
}