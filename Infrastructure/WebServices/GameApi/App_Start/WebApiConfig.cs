using System.Linq;
using System.Web.Http;
using System.Web.Http.Filters;
using AFT.RegoV2.Core.Game.Services;
using AFT.RegoV2.GameApi.Interface.Services;
using AFT.RegoV2.Shared;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.GameApi
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API routes
            config.MapHttpAttributeRoutes();

            config.DependencyResolver = new UnityResolver(GameApiFactory.Default.Container, config);

            RegisterFilterProviders(config, GameApiFactory.Default.Container);
        }

        private static void RegisterFilterProviders(HttpConfiguration config, IUnityContainer container)
        {
            var providers = config.Services.GetFilterProviders().ToList();
            config.Services.Add(typeof(IFilterProvider), new WebApiUnityActionFilterProvider(container));
            var defaultprovider = providers.First(p => p is ActionDescriptorFilterProvider);
            config.Services.Remove(typeof(IFilterProvider), defaultprovider);
        }
    }
}