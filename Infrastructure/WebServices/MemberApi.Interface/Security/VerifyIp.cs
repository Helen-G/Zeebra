using AFT.RegoV2.Domain.Security.Data;
using AFT.RegoV2.Shared;

namespace AFT.RegoV2.MemberApi.Interface.Security
{
    public class VerifyIpRequest 
    {
        public string IpAddress { get; set; }

        public string BrandName { get; set; }
    }

    public class VerifyIpResponse
    {
        public VerifyIpResponse()
        {
        }

        public VerifyIpResponse(VerifyIpResult result)
        {
            Allowed = result.Allowed;
            BlockingType = result.BlockingType;
            RedirectionUrl = result.RedirectionUrl;
        }

        public bool Allowed { get; set; }

        public string BlockingType { get; set; }

        public string RedirectionUrl { get; set; }
    }

    public class BlockingTypes
    {
        public const string Redirection = "Redirection";
        public const string LoginRegistration = "Login/Registration";
    }
}
