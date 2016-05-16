using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Core.Fraud.Data;
using AFT.RegoV2.Infrastructure.DataAccess.Fraud.Migrations;

namespace AFT.RegoV2.Infrastructure.DataAccess.Fraud.Mappings
{
    public class WagerConfigurationMap : EntityTypeConfiguration<WagerConfiguration>
    {
        #region Constructors

        public WagerConfigurationMap()
        {
            ToTable("WagerConfiguration", Configuration.Schema);
            HasKey(p => p.Id);
        }

        #endregion
    }
}