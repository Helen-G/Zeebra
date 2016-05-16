using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AFT.RegoV2.Core.Brand.Data;

namespace AFT.RegoV2.Infrastructure.DataAccess.Brand.Mappings
{
    public class ContentTranslationMap : EntityTypeConfiguration<ContentTranslation>
    {
        public ContentTranslationMap(string schema)
         {
             ToTable("ContentTranslations", schema);
                HasKey(b => b.Id);
             
                Property(p => p.Name).HasMaxLength(50).IsRequired();
                Property(p => p.Source).HasMaxLength(200).IsRequired();
                Property(p => p.Language).IsRequired();
                Property(p => p.Translation).IsRequired();
                
         }
    }
}
