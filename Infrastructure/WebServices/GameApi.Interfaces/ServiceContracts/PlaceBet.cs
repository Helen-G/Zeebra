using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using AFT.RegoV2.GameApi.Interface.Classes;

namespace AFT.RegoV2.GameApi.Interface.ServiceContracts
{
    [DataContract, KnownType(typeof(GameApiRequestBase))]
    public class PlaceBet : GameApiRequestBase
    {
        [DataMember(Name="transactions")]
        public List<PlaceBetTransaction> Transactions { get; set; }
    }


    [DataContract]
    public class PlaceBetTransaction
    {
        [DataMember(Name = "txid")]
        public string Id { get; set; }

        [DataMember(Name = "amt")]
        public decimal Amount { get; set; }

        [DataMember(Name = "cur")]
        public string CurrencyCode { get; set; }

        [DataMember(Name = "gameid")]
        public string RoundId { get; set; }

        [DataMember(Name = "gametypeid")]
        public string GameTypeId { get; set; }

        [DataMember(Name = "timestamp")]
        public DateTimeOffset TimeStamp { get; set; }

        [DataMember(Name = "desc")]
        public string Description { get; set; }
    }

    [DataContract, KnownType(typeof(GameApiResponseBase))]
    public class PlaceBetResponse : GameApiResponseBase
    {
        [DataMember(Name = "bal")]
        public decimal Balance { get; set; }

        [DataMember(Name = "cur")]
        public string CurrencyCode { get; set; }

        [DataMember(Name="transactions")]
        public List<PlaceBetResponseTransaction> Transactions { get; set; }
    }

    [DataContract]
    public class PlaceBetResponseTransaction
    {
        [DataMember(Name = "txid")]
        public string Id { get; set; }

        [DataMember(Name = "ptxid")]
        public Guid GameActionId { get; set; }

        [DataMember(Name = "dup")]
        public int IsDuplicate { get; set; }
    }



}