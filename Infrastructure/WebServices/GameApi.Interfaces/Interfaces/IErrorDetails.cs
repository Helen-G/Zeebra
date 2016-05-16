using AFT.RegoV2.GameApi.Interface.Classes;

namespace AFT.RegoV2.GameApi.Interface.ServiceContracts
{
    public interface IGameApiErrorDetails
    {
        GameApiErrorCode ErrorCode { get; set; }
        string ErrorDescription { get; set; }
    }
}
