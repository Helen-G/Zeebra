using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Core.Security.Data;

namespace AFT.RegoV2.Infrastructure.DataAccess.Security.Mappings
{
    public class ErrorMap : EntityTypeConfiguration<Error>
    {
        public ErrorMap(string schema)
        {
            ToTable("Errors", schema);
            HasKey(u => u.Id);
        }
    }
}
