using System;
using System.Configuration;

namespace AFT.RegoV2.MemberWebsite
{
    public class AppSettings
    {
        public AppSettings()
        {
            MemberApiUrl = new Uri(ConfigurationManager.AppSettings["MemberApiUrl"]);
            BrandId = new Guid(ConfigurationManager.AppSettings["BrandId"]);
            BrandCode = ConfigurationManager.AppSettings["BrandCode"];
        }

        public Uri MemberApiUrl { get; private set; }
        public Guid BrandId { get; private set; }
        public string BrandCode { get; private set; }
    }
}