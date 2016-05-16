using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Core.Brand.Data;

namespace AFT.RegoV2.Infrastructure.DataAccess.Brand.Mappings
{
    public class ContractMap: EntityTypeConfiguration<Contract>
    {
        public ContractMap(string schema)
        {
            ToTable("LicenseeContracts", schema);
            HasRequired(c => c.Licensee).WithMany(x => x.Contracts).HasForeignKey(x => x.LicenseeId);
        }
    }
}
