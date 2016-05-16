using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using AFT.RegoV2.AdminApi.Interface.Common;
using AFT.RegoV2.AdminApi.Interface.Proxy;
using AFT.RegoV2.AdminApi.Tests.Integration.Core;
using AFT.RegoV2.Core.Security.ApplicationServices;
using AFT.RegoV2.Core.Security.Data;
using AFT.RegoV2.Core.Security.Entities;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Helpers;
using Microsoft.Owin.Hosting;
using Microsoft.Practices.Unity;
using Newtonsoft.Json;
using TechTalk.SpecFlow;

namespace AFT.RegoV2.AdminApi.Tests.Integration.Steps
{
    [Binding]
    public class BaseSteps
    {
        private static IDisposable _webServer;
        protected static AdminApiProxy AdminApiProxy { get; set; }
        protected static string AdminApiUrl { get; set; }
        protected static string Token { get; set; }
        protected static IUnityContainer Container { get; set; }

        [BeforeFeature]
        public static void Before()
        {
            AdminApiUrl = GetFullAdminApiUrl();

            Container = new AdminApiTestsBase().Container;
            TestStartup.Container = Container;
            _webServer = WebApp.Start<TestStartup>(AdminApiUrl);
        }

        [AfterFeature]
        public static void After()
        {
            _webServer.Dispose();
        }

        protected static void LogInAdminApi(string username, string password)
        {
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("username", username),
                new KeyValuePair<string, string>("password", password),
                new KeyValuePair<string, string>("grant_type", "password")
            });

            var response = new HttpClient().PostAsync(AdminApiUrl + "token", formContent).Result;
            var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(response.Content.ReadAsStringAsync().Result);
            Token = tokenResponse.AccessToken;

            AdminApiProxy = new AdminApiProxy(AdminApiUrl, Token);
        }

        protected User CreateUserWithPermissions(string category, string[] permissions, string password)
        {
            var securityTestHelper = Container.Resolve<SecurityTestHelper>();
            var brandTestHelper = Container.Resolve<BrandTestHelper>();
            var licensee = brandTestHelper.CreateLicensee();
            var brand = brandTestHelper.CreateBrand(licensee, isActive: true);
            var brands = new[] { brand };

            return securityTestHelper.CreateUserWithPermissions(category, permissions, brands, password);
        }

        protected void LogWithNewUser(string category, string permission)
        {
            var password = TestDataGenerator.GetRandomString(8);
            var user = CreateUserWithPermissions(category, new[] { permission }, password);
            LogInAdminApi(user.Username, password);
        }

        private static string GetFullAdminApiUrl()
        {
            var ipAddress = IPAddress.Loopback;
            var portAvailable = GetAvailablePort(6368, 6568, ipAddress);

            return String.Format("http://{0}:{1}/", "localhost", portAvailable);
        }

        private static int GetAvailablePort(int rangeStart, int rangeEnd, IPAddress ip, bool includeIdlePorts = false)
        {
            IPGlobalProperties ipProps = IPGlobalProperties.GetIPGlobalProperties();

            // if the ip we want a port on is an 'any' or loopback port we need to exclude all ports that are active on any IP
            Func<IPAddress, bool> isIpAnyOrLoopBack = i => IPAddress.Any.Equals(i) ||
                                                           IPAddress.IPv6Any.Equals(i) ||
                                                           IPAddress.Loopback.Equals(i) ||
                                                           IPAddress.IPv6Loopback.
                                                               Equals(i);
            // get all active ports on specified IP.
            List<ushort> excludedPorts = new List<ushort>();

            // if a port is open on an 'any' or 'loopback' interface then include it in the excludedPorts
            excludedPorts.AddRange(from n in ipProps.GetActiveTcpConnections()
                                   where
                                       n.LocalEndPoint.Port >= rangeStart &&
                                       n.LocalEndPoint.Port <= rangeEnd && (
                                       isIpAnyOrLoopBack(ip) || n.LocalEndPoint.Address.Equals(ip) ||
                                        isIpAnyOrLoopBack(n.LocalEndPoint.Address)) &&
                                        (!includeIdlePorts || n.State != TcpState.TimeWait)
                                   select (ushort)n.LocalEndPoint.Port);

            excludedPorts.AddRange(from n in ipProps.GetActiveTcpListeners()
                                   where n.Port >= rangeStart && n.Port <= rangeEnd && (
                                   isIpAnyOrLoopBack(ip) || n.Address.Equals(ip) || isIpAnyOrLoopBack(n.Address))
                                   select (ushort)n.Port);

            excludedPorts.AddRange(from n in ipProps.GetActiveUdpListeners()
                                   where n.Port >= rangeStart && n.Port <= rangeEnd && (
                                   isIpAnyOrLoopBack(ip) || n.Address.Equals(ip) || isIpAnyOrLoopBack(n.Address))
                                   select (ushort)n.Port);

            excludedPorts.Sort();

            for (int port = rangeStart; port <= rangeEnd; port++)
            {
                if (!excludedPorts.Contains((ushort)port))
                {
                    return port;
                }
            }

            return 0;
        }
    }
}