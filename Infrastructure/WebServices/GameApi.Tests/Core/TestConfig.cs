using System.Configuration;

namespace AFT.RegoV2.GameApi.Tests.Core
{
    public interface ITestConfig
    {
        string GameApiUrl { get; }
        string MemberApiUrl { get; }
    }
    public sealed class TestConfig : ITestConfig
    {
        string ITestConfig.GameApiUrl { get { return ConfigurationManager.AppSettings["GameApiUrl"]; } }
        string ITestConfig.MemberApiUrl { get { return ConfigurationManager.AppSettings["MemberApiUrl"]; } }
    }
}