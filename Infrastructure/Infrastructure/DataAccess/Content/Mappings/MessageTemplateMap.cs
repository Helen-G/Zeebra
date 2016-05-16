using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Core.Content.Data;

namespace AFT.RegoV2.Infrastructure.DataAccess.Content.Mappings
{
    public class MessageTemplateMap : EntityTypeConfiguration<MessageTemplate>
    {
        public MessageTemplateMap(string schema)
        {
            ToTable("MessageTemplates", schema);
        }
    }
}