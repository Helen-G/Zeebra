using System;
using System.Web.Http;
using AFT.RegoV2.AdminApi.Filters;
using AFT.RegoV2.AdminApi.Provider;
using AFT.RegoV2.Shared;
using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;
using Microsoft.Practices.Unity;
using Owin;

[assembly: OwinStartup(typeof(AFT.RegoV2.AdminApi.Startup))]

namespace AFT.RegoV2.AdminApi
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);

            var unityContainer = GetUnityContainer();

            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            //app.Use<InvalidLoginOwinMiddleware>();

            app.UseOAuthAuthorizationServer(new OAuthAuthorizationServerOptions
            {
                AllowInsecureHttp = true,
                TokenEndpointPath = new PathString("/token"),
                AuthorizationCodeExpireTimeSpan = TimeSpan.FromHours(1),
                Provider = GetAuthorizationServerProvider(),
                AccessTokenExpireTimeSpan = TimeSpan.FromHours(1),
                RefreshTokenProvider = new RefreshTokenProvider()
            });

            app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions());

            var config = new HttpConfiguration();

            config.DependencyResolver = new UnityResolver(unityContainer, config);

            WebApiConfig.Register(config);

            app.UseWebApi(config);
        }

        protected virtual OAuthAuthorizationServerProvider GetAuthorizationServerProvider()
        {
            return new AuthServerProvider(GetUnityContainer());
        }

        protected virtual IUnityContainer GetUnityContainer()
        {
            return AdminApiDependencyResolver.Default.Container;
        }
    }
}
