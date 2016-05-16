using System.Runtime.Serialization;

namespace AFT.RegoV2.GameApi.Interface.ServiceContracts.SlotServer
{
    [DataContract]
    public class WalletOperationResponse
    {
        [DataMember(Name="amt")]
        public decimal Amount { get; set; }
        [DataMember(Name = "err")]
        public string Error { get; set; }
    }
}