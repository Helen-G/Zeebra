using System;

namespace AFT.RegoV2.MemberApi.Interface.GameProvider
{
    public class GameRedirectRequest
    {
        public Guid GameId { get; set; }
        public Guid GameProviderId { get; set; }
        public string PlayerIpAddress { get; set; }
        public string BrandCode { get; set; }
    }

    public class GameRedirectResponse
    {
        public Uri Url { get; set; }
        public bool IsPostRequest { get; set; }
    }
}
