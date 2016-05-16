using System;

namespace AFT.RegoV2.AdminWebsite.Controllers
{
    public class ChangeVipLevelCommand
    {
        public Guid PlayerId { get; set; }
        public Guid NewVipLevel { get; set; }
        public string Remarks { get; set; }
    }
}