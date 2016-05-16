namespace AFT.RegoV2.Domain.Security.Data
{
    public class VerifyIpResult
    {
        public bool Allowed { get; set; }
        public string BlockingType { get; set; }
        public string RedirectionUrl { get; set; }
    }
}
