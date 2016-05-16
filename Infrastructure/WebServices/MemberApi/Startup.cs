using System;
using System.Web.Http;
using AFT.RegoV2.MemberApi.Filters;
using AFT.RegoV2.MemberApi.Provider;
using AFT.RegoV2.Shared;
using MemberApi;
using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security.OAuth;
using Microsoft.Practices.Unity;
using Owin;

[assembly: OwinStartup(typeof(AFT.RegoV2.MemberApi.Startup))]

namespace AFT.RegoV2.MemberApi
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var unityContainer = GetUnityContainer();
            
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            app.Use<InvalidLoginOwinMiddleware>();

            app.UseOAuthAuthorizationServer(new OAuthAuthorizationServerOptions
            {
                AllowInsecureHttp = true,
                TokenEndpointPath = new PathString("/token"),
                AuthorizationCodeExpireTimeSpan = TimeSpan.FromHours(1),
                Provider = GetAuthorizationServerProvider()
            });

            app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions());

            app.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);

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
            return MemberApiDependencyResolver.Default.Container;
        }
    }
}
