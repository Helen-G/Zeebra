using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using AFT.RegoV2.GameApi.Interface.Classes;

namespace AFT.RegoV2.GameApi.Interface.ServiceContracts
{
    [DataContract, KnownType(typeof(GameApiRequestBase))]
    public class CancelTransaction : GameApiRequestBase
    {
        [DataMember(Name = "transactions")]
        public List<CancelTransactionData> Transactions { get; set; }
    }
    [DataContract, KnownType(typeof(CancelTransactionDataBase))]
    public class CancelTransactionData : CancelTransactionDataBase
    {
        [DataMember(Name = "timestamp")]
        public DateTimeOffset TimeStamp { get; set; }
    }

    [DataContract]
    public class CancelTransactionDataBase
    {
        [DataMember(Name = "txid")]
        public string Id { get; set; }

        [DataMember(Name = "txrefid")]
        public string ReferenceId { get; set; }

        [DataMember(Name = "gameid")]
        public string RoundId { get; set; }

        [DataMember(Name = "desc")]
        public string Description { get; set; }
    }

    [DataContract, KnownType(typeof(GameApiResponseBase))]
    public class CancelTransactionResponse : GameApiResponseBase
    {
        [DataMember(Name = "bal")]
        public decimal Balance { get; set; }

        [DataMember(Name = "cur")]
        public string CurrencyCode { get; set; }

        [DataMember(Name = "transactions")]
        public List<CancelTransactionDataResponse> Transactions { get; set; }
    }

    [DataContract]
    public class CancelTransactionDataResponse
    {
        [DataMember(Name = "txid")]
        public string Id { get; set; }

        [DataMember(Name = "ptxid")]
        public Guid GameActionId { get; set; }

        [DataMember(Name = "dup")]
        public int IsDuplicate { get; set; }

    }
}
