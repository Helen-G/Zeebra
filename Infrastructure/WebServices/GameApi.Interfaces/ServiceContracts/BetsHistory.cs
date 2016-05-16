using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using AFT.RegoV2.GameApi.Interface.Classes;

namespace AFT.RegoV2.GameApi.Interface.ServiceContracts
{
    [DataContract, KnownType(typeof(GameApiRequestBase))]
    public class RoundsHistory : GameApiRequestBase
    {

        [DataMember(Name = "count")]
        public int RecordCount { get; set; }

        public RoundsHistory()
        {
            RecordCount = 10;
        }
    }

    [DataContract, KnownType(typeof(GameApiResponseBase))]
    public class BetsHistoryResponse : GameApiResponseBase
    {
        [DataMember(Name = "rounds")]
        public List<RoundHistoryData> Rounds { get; set; }
    }

    [Serializable]
    [DataContract]
    public class RoundHistoryData
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }
        [DataMember(Name = "status")]
        public string Status { get; set; }
        [DataMember(Name = "amount")]
        public decimal Amount { get; set; }
        [DataMember(Name = "wonAmount")]
        public decimal WonAmount { get; set; }
        [DataMember(Name = "adjustedAmount")]
        public decimal AdjustedAmount { get; set; }

        [DataMember(Name = "createdOn")]
        public DateTimeOffset CreatedOn { get; set; }
        [DataMember(Name = "closedOn")]
        public DateTimeOffset? ClosedOn { get; set; }
        [DataMember(Name = "betTransactions")]
        public List<GameActionHistoryData> GameActions { get; set; }

    }
    [DataContract]
    public class GameActionHistoryData
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }
        [DataMember(Name = "platformTxId")]
        public Guid PlatformTxId { get; set; }
        [DataMember(Name = "tokenId")]
        public Guid TokenId { get; set; }
        [DataMember(Name = "amount")]
        public decimal Amount { get; set; }
        [DataMember(Name = "description")]
        public string Description { get; set; }
        [DataMember(Name = "transactionType")]
        public string TransactionType { get; set; }
        [DataMember(Name = "createdOn")]
        public DateTimeOffset CreatedOn { get; set; }
    }


}
