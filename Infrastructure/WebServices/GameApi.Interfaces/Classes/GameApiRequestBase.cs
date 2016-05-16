using System.Runtime.Serialization;
using AFT.RegoV2.GameApi.Interface.ServiceContracts;

namespace AFT.RegoV2.GameApi.Interface.Classes
{
    [DataContract]
    public class GameApiRequestBase : IGameApiRequest
    {
        [DataMember(Name="authtoken")]
        public string AuthToken { get; set; }
    }


}
