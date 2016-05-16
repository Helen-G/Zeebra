using System;
using System.Runtime.Serialization;
using AFT.RegoV2.GameApi.Interface.ServiceContracts;

namespace AFT.RegoV2.GameApi.Interface.Classes
{
    [DataContract]
    public class GameApiResponseBase : IGameApiResponse
    {
        [DataMember(Name = "err")]
        public GameApiErrorCode ErrorCode { get; set; }

        [DataMember(Name = "errdesc")]
        public string ErrorDescription { get; set; }

        public GameApiResponseBase()
        {
            ErrorCode = GameApiErrorCode.NoError;
            ErrorDescription = String.Empty;
        }
    }
}
