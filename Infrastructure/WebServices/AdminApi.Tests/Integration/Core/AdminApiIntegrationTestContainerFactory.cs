using AFT.RegoV2.Infrastructure.DependencyResolution;
using AFT.RegoV2.Tests.Common;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.AdminApi.Tests.Integration.Core
{
    public class AdminApiIntegrationTestContainerFactory : IContainerFactory
    {
        private readonly AdminApiContainerFactory _adminApiContainerFactory;
        private readonly SingleProcessTestContainerFactory _singleProcessTestContainerFactory;

        public AdminApiIntegrationTestContainerFactory()
        {
            //we're not doing injection here, as it is factory already and it's very unlikely that we may need it here
            _adminApiContainerFactory = new AdminApiContainerFactory();
            _singleProcessTestContainerFactory = new SingleProcessTestContainerFactory();
        }

        public IUnityContainer CreateWithRegisteredTypes()
        {
            var container = new UnityContainer();
            _adminApiContainerFactory.RegisterTypes(container);
            _singleProcessTestContainerFactory.RegisterTypes(container);
            this.RegisterTypes(container);
            return container;
        }

        public void RegisterTypes(IUnityContainer container)
        {
        }
    }
}