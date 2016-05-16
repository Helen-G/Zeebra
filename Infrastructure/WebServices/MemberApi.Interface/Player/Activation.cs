using AFT.RegoV2.Shared;

namespace AFT.RegoV2.MemberApi.Interface.Player
{
    public class ActivationRequest 
    {
        public string Token { get; set; }
    }

    public class ActivationResponse 
    {
        public string Token { get; set; }
        public bool Activated { get; set; }
    }
}
