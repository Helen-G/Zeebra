using System;
using AFT.RegoV2.Infrastructure.DependencyResolution;
using AFT.RegoV2.MemberApi;
using AFT.RegoV2.MemberApi.Interface;
using AFT.RegoV2.MemberApi.Interface.Account;
using AFT.RegoV2.MemberApi.Interface.Proxy;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using Microsoft.Owin.Hosting;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.Tests.Integration
{
    public class TestStartup : Startup
    {
        protected override IUnityContainer GetUnityContainer()
        {
            return new ApplicationContainer();
        }
    }

    internal abstract class PlayerServiceTestsBase : TestsBase
    {
        protected MemberApiProxy ServiceProxy { get; set; }

        private IDisposable _webServer;

        public override void BeforeEach()
        {
            _webServer = WebApp.Start<TestStartup>("http://*:5555");
            
            ServiceProxy = new MemberApiProxy("http://localhost:5555");
        }

        public RegisterRequest RegisterPlayerAndLogin()
        {
            
            var registrationData = TestDataGenerator.CreateRandomRegistrationRequestData();
            
            ServiceProxy.Register(registrationData);

            ServiceProxy.Login(new LoginRequest
            {
                Username = registrationData.Username,
                Password = registrationData.Password
            });

            
            return registrationData;
        }


        public override void AfterEach()
        {
            ServiceProxy.Dispose();
            _webServer.Dispose();
        }
    }


}