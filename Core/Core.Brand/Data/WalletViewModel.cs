using System;
using System.Collections.Generic;

namespace AFT.RegoV2.Core.Brand.Data
{
    public class WalletViewModel
    {
        public WalletViewModel()
        {
            ProductIds = new List<Guid>();
        }

        public Guid? Id { get; set; }
        public string Name { get; set; }
        public bool IsMain { get; set; }
        public IEnumerable<Guid> ProductIds { get; set; }
    }
}