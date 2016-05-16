using System;
using System.Collections.Generic;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Shared;

namespace AFT.RegoV2.MemberApi.Interface.Payment
{
    public class FundTransferFormDataRequest
    {
        public Guid BrandId { get; set; }
    }

    public class FundTransferFormDataResponse 
    {
        public Dictionary<Guid, string> Wallets { get; set; }
    }
    
    public class FundRequest
    {
        public Guid WalletId { get; set; }
        public decimal Amount { get; set; }
        public TransferFundType TransferFundType { get; set; }
        public string BonusCode { get; set; }
    }

    public class FundResponse
    {
        public string TransferId { get; set; }
    }
}
