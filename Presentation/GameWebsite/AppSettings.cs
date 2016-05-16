using System;
using System.Configuration;

namespace AFT.RegoV2.GameWebsite
{
    public class AppSettings
    {
        public AppSettings()
        {
            GameApiUrl = new Uri(ConfigurationManager.AppSettings["GameApiUrl"]);
        }

        public Uri GameApiUrl { get; private set; }
    }
}