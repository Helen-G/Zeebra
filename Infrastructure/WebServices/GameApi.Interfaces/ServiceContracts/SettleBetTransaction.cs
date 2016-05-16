using System;
using System.Runtime.Serialization;

namespace AFT.RegoV2.GameApi.Interface.ServiceContracts
{
    [DataContract]
    public class SettleBetTransaction
    {
        [DataMember(Name = "txid")]
        public string Id { get; set; }

        [DataMember(Name = "amt")]
        public decimal Amount { get; set; }

        [DataMember(Name = "cur")]
        public string CurrencyCode { get; set; }

        [DataMember(Name = "gameid")]
        public string RoundId { get; set; }

        [DataMember(Name = "timestamp")]
        public DateTimeOffset TimeStamp { get; set; }

        [DataMember(Name = "desc")]
        public string Description { get; set; }
    }
}