using System.Collections.Generic;
using System.Runtime.Serialization;
using AFT.RegoV2.GameApi.Interface.Classes;

namespace AFT.RegoV2.GameApi.Interface.ServiceContracts
{
    [DataContract, KnownType(typeof(GameApiRequestBase))]
    public class FreeBet : GameApiRequestBase
    {
        [DataMember(Name = "transactions")]
        public List<SettleBetTransaction> Transactions { get; set; }
    }
    [DataContract, KnownType(typeof(GameApiResponseBase))]
    public class FreeBetResponse : GameApiResponseBase
    {
        [DataMember(Name = "bal")]
        public decimal Balance { get; set; }

        [DataMember(Name = "cur")]
        public string CurrencyCode { get; set; }

        [DataMember(Name = "transactions")]
        public List<SettleBetResponseTransaction> Transactions { get; set; }
    }
}
