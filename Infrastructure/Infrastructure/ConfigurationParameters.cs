using System.Configuration;

namespace AFT.RegoV2.Infrastructure
{
    public class ConfigurationParameters
    {
        public static string RmqLogin { get { return ConfigurationManager.AppSettings["rmq.login"]; } }
        public static string RmqPassword { get { return ConfigurationManager.AppSettings["rmq.password"]; } }
        public static string RmqUrl { get { return ConfigurationManager.AppSettings["rmq.url"]; } }
        public static string RmqPort { get { return ConfigurationManager.AppSettings["rmq.port"]; } }
        public static string RmqVhost { get { return ConfigurationManager.AppSettings["rmq.vhost"]; } }
    }
}