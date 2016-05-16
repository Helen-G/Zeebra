using System;
using System.Collections.Generic;

namespace AFT.RegoV2.Core.Brand.Data
{
    public class WalletTemplateViewModel
    {

        public WalletTemplateViewModel()
        {
            MainWallet = new WalletViewModel();
            ProductWallets = new List<WalletViewModel>();
        }

        public Guid LicenseeId { get; set; }
        public Guid BrandId { get; set; }
        public WalletViewModel MainWallet { get; set; }
        public List<WalletViewModel> ProductWallets { get; set; }
    }
}