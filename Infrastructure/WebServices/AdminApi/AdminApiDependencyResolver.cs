using AFT.RegoV2.AdminApi.Provider;
using AFT.RegoV2.Core.Security.Interfaces;
using AFT.RegoV2.Infrastructure.DependencyResolution;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.AdminApi
{
    public interface IAdminApiDependencyResolver
    {
        IUnityContainer Container { get; }
    }

    public class AdminApiDependencyResolver : IAdminApiDependencyResolver
    {
        public static readonly IAdminApiDependencyResolver Default = new AdminApiDependencyResolver();

        private readonly IUnityContainer _container = new AdminApiContainerFactory().CreateWithRegisteredTypes();

        private AdminApiDependencyResolver()
        {
        }

        IUnityContainer IAdminApiDependencyResolver.Container
        {
            get { return _container; }
        }
    }

    public class AdminApiContainerFactory : ApplicationContainerFactory
    {
        public override void RegisterTypes(IUnityContainer container)
        {
            base.RegisterTypes(container);

            container.RegisterType<IUserInfoProvider, UserInfoProvider>();
        }
    }
}