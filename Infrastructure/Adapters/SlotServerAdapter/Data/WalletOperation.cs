using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AFT.RegoV2.Webservices.Adapters.SlotServer.Data
{
    public class WalletOperation
    {
        public string Operation { get; set; }
        public string MemberCode { get; set; }
        public string Currency { get; set; }
        public decimal Amount { get; set; }
        public string TransactionId { get; set; }
        /*public string GameId { get; set; }
        public string TransactionSubTypeId { get; set; }
        public string HandId { get; set; }
        public string PlayerHandle { get; set; }*/
    }
}