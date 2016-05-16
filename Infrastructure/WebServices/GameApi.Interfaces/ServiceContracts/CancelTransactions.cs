using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using AFT.RegoV2.GameApi.Interface.Classes;

namespace AFT.RegoV2.GameApi.Interface.ServiceContracts
{
    [DataContract]
    public class CancelTransactions : IGameApiBatchRequest
    {
        [DataMember(Name = "batchid")]
        public string BatchId { get; set; }

        [DataMember(Name = "brandkey")]
        public string BrandKey { get; set; }

        [DataMember(Name = "timestamp")]
        public DateTimeOffset TimeStamp { get; set; }

        [DataMember(Name = "transactions")]
        public List<BatchCancelTransactionData> Transactions { get; set; }
    }
    public class BatchCancelTransactionData : CancelTransactionDataBase
    {
        [DataMember(Name = "userid")]
        public Guid UserId { get; set; }

        [DataMember(Name = "tag")]
        public string BrandCode { get; set; }

    }

    [DataContract, KnownType(typeof(GameApiResponseBase))]
    public class CancelTransactionsResponse : GameApiResponseBase
    {
        [DataMember(Name = "batchid")]
        public string BatchId { get; set; }

        [DataMember(Name = "dup")]
        public int IsDuplicate { get; set; }

        [DataMember(Name = "batchtime")]
        public string BatchTimestamp { get; set; }

        [DataMember(Name = "transcount")]
        public int TransactionCount { get; set; }

        [DataMember(Name = "errors")]
        public List<BatchTransactionError> Errors { get; set; }
    }

}
