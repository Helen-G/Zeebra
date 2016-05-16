using System.Data.Entity.ModelConfiguration;

namespace AFT.RegoV2.Infrastructure.DataAccess.Content.Mappings
{
    public class LanguageMap : EntityTypeConfiguration<Core.Content.Data.Language>
    {
        public LanguageMap(string schema)
        {
            ToTable("Languages", schema);

            HasKey(x => x.Code);
        }
    }
}