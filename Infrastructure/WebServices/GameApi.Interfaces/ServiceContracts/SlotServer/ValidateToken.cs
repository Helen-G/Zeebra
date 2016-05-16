using System.Runtime.Serialization;

namespace AFT.RegoV2.GameApi.Interface.ServiceContracts.SlotServer
{
    //[Route("/api/slotserver/validateToken")]
    [DataContract]
    public class ValidateToken
    {
        [DataMember]
        public string Token { get; set; }
        [DataMember]
        public string IpAddress { get; set; }
    }
}