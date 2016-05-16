using AFT.RegoV2.AdminApi.Provider;
using AFT.RegoV2.Core.Security.Interfaces;
using Microsoft.Owin.Security.OAuth;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.AdminApi.Tests.Integration.Core
{
    public class TestStartup : Startup
    {
        public static IUnityContainer Container;
        protected override IUnityContainer GetUnityContainer()
        {
            return Container;
        }

        protected override OAuthAuthorizationServerProvider GetAuthorizationServerProvider()
        {
            Container.RegisterType<IUserInfoProvider, UserInfoProvider>();
            return new AuthServerProvider(Container);
        }
    }
}
