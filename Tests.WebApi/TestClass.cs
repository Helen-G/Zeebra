using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AFT.RegoV2.Infrastructure.DependencyResolution;
using AFT.RegoV2.MemberApi;
using AFT.RegoV2.MemberApi.Interface.Player;
using AFT.RegoV2.MemberApi.Interface.Proxy;
using Microsoft.Owin.Hosting;
using Microsoft.Owin.Testing;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Integration.WebApi
{

    public class TestStartup : Startup
    {
        protected override IUnityContainer GetUnityContainer()
        {
            return new ApplicationContainer();
        }
    }
    [TestFixture, Explicit, Category("Integration")]
    public class TestClass
    {
        
        [Test]
        public void TestMethod()
        {
            using (WebApp.Start<TestStartup>("http://*:5555"))
            {
                var token = GetToken();
            }
        }

        [Test]
        public void TestMethod2()
        {
            using (WebApp.Start<TestStartup>("http://*:5555"))
            {
                var token = GetToken();
            }
        }


        private string GetToken()
        {
            string accessToken;
            using (var proxy = new MemberApiProxy("http://localhost:5555"))
            {
                var loginResult = proxy.Login(new LoginRequest
                {
                    Username = "testplayer",
                    Password = "123456"
                });
                accessToken = loginResult.AccessToken;
            }
            using (var proxy = new MemberApiProxy("http://localhost:5555", accessToken))
            {

                var result = proxy.SecurityQuestions(new SecurityQuestionsRequest());

                Assert.AreNotEqual(0, result.SecurityQuestions.Count);

                return accessToken;
            }
        }

    }
}
