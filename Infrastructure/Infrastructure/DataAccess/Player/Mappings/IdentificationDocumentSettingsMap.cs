using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Core.Player.Data;

namespace AFT.RegoV2.Core.Common.Repository.Mappings
{
    public class IdentificationDocumentSettingsMap : EntityTypeConfiguration<IdentificationDocumentSettings>
    {
        public IdentificationDocumentSettingsMap(string schema)
        {
            ToTable("IdentificationDocumentSettings", schema);

            HasKey(p => p.Id);
            HasRequired(o => o.Brand)
                .WithMany()
                .HasForeignKey(o => o.BrandId);
        }
    }
}
