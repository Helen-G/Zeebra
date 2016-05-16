using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AFT.RegoV2.Domain.Payment.Data;
using AFT.RegoV2.Infrastructure.DataAccess.Payment.Migrations;

namespace AFT.RegoV2.Infrastructure.DataAccess.Payment.Mappings
{
    public class VipLevelMap : EntityTypeConfiguration<VipLevel>
    {
        public VipLevelMap()
        {
            ToTable("VipLevels", Configuration.Schema);
        }
    }
}
