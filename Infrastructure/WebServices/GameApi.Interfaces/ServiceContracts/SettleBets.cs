using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using AFT.RegoV2.GameApi.Interface.Classes;

namespace AFT.RegoV2.GameApi.Interface.ServiceContracts
{
    [DataContract]
    public class SettleBets
    {
        [DataMember(Name = "batchid")]
        public string BatchId { get; set; }

        [DataMember(Name = "brandkey")]
        public string BrandKey { get; set; }

        [DataMember(Name = "timestamp")]
        public DateTimeOffset TimeStamp { get; set; }

        [DataMember(Name = "transactions")]
        public List<BatchSettleBetTransaction> Transactions { get; set; }
    }

    [DataContract, KnownType(typeof(SettleBetTransaction))]
    public class BatchSettleBetTransaction : SettleBetTransaction
    {
        [DataMember(Name = "userid")]
        public Guid UserId { get; set; }

        [DataMember(Name = "tag")]
        public string BrandCode { get; set; }

        [DataMember(Name = "txtype")]
        public int TransactionType { get; set; }
    }

    public static class BatchSettleBetTransactionType
    {
        public const int Win = 510;
        public const int Lose = 520;
    }

    [DataContract, KnownType(typeof(GameApiResponseBase))]
    public class SettleBetsResponse : GameApiResponseBase
    {
        [DataMember(Name = "batchid")]
        public string BatchId { get; set; }

        [DataMember(Name = "dup")]
        public int IsDuplicate { get; set; }

        [DataMember(Name = "batchtime")]
        public string BatchTimestamp { get; set; }

        [DataMember(Name = "elapsed")]
        public long Elapsed { get; set; }

        [DataMember(Name = "transcount")]
        public int TransactionCount { get; set; }

        [DataMember(Name = "errors")]
        public List<BatchTransactionError> Errors  { get; set; }
    }

    [DataContract]
    public class BatchTransactionError : SettleBetResponseTransaction, IGameApiErrorDetails
    {
        [DataMember(Name = "userid")]
        public Guid UserId { get; set; }

        [DataMember(Name = "err")]
        public GameApiErrorCode ErrorCode { get; set; }

        [DataMember(Name = "errdesc")]
        public string ErrorDescription { get; set; }
    }
}
