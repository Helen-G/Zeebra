using System.Runtime.Serialization;

namespace AFT.RegoV2.GameApi.Interface.ServiceContracts.SlotServer
{
    [DataContract]
    public class ValidateTokenResponse
    {
        [DataMember(Name = "validateTokenXMLResult")]
        public bool Result { get; set; }

        [DataMember(Name = "memberCode")]
        public string MemberCode { get; set; }

        [DataMember(Name = "currency")]
        public string Currency { get; set; }

        [DataMember(Name = "ipAddress")]
        public string IpAddress { get; set; }

        [DataMember(Name = "statusCode")]
        public string StatusCode { get; set; }

        [DataMember(Name = "statusDesc")]
        public string StatusDesc { get; set; }

        [DataMember(Name = "username")]
        public string UserName { get; set; }

        [DataMember(Name = "lobbyUrl")]
        public string LobbyUrl { get; set; }

        [DataMember(Name = "cashierUrl")]
        public string CashierUrl { get; set; }

        [DataMember(Name = "customerSupportUrl")]
        public string CustomerSupportUrl { get; set; }


    }
}