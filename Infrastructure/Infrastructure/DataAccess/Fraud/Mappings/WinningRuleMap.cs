using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Core.Fraud.Data;
using AFT.RegoV2.Infrastructure.DataAccess.Fraud.Migrations;

namespace AFT.RegoV2.Infrastructure.DataAccess.Fraud.Mappings
{
    public class WinningRuleMap : EntityTypeConfiguration<WinningRule>
    {
        public WinningRuleMap()
        {
            ToTable("WinningRules", Configuration.Schema);
            HasKey(x => x.Id);
            HasRequired(x => x.AutoVerificationCheckConfiguration)
                .WithMany(configuration => configuration.WinningRules)
                .HasForeignKey(o => o.AutoVerificationCheckConfigurationId);

            /*HasRequired(x => x.Product)
                .WithMany()
                .HasForeignKey(o => o.ProductId);*/
        }
    }
}
