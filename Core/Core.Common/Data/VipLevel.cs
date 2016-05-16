using System;
using System.Collections.Generic;

namespace AFT.RegoV2.Core.Common.Data
{
    public class VipLevelLimitViewModel
    {
        public Guid Id { get; set; }
        public Guid? GameServerId { get; set; }
        public string CurrencyCode { get; set; }
        public Guid? BetLimitId { get; set; }
    }

    public class VipLevelViewModel
    {
        public VipLevelViewModel()
        {
            Limits = new List<VipLevelLimitViewModel>();
        }

        public Guid Id { get; set; }
        public Guid Brand { get; set; }
        public bool IsDefault { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Color { get; set; }
        public string Remark { get; set; }
        public int Rank { get; set; }
        public IEnumerable<VipLevelLimitViewModel> Limits { get; set; }
    }
}