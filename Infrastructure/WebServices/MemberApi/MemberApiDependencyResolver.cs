using AFT.RegoV2.Infrastructure.DependencyResolution;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.MemberApi
{

    public interface IMemberApiDependencyResolver
    {
        IUnityContainer Container { get; }
    }
    public class MemberApiDependencyResolver : IMemberApiDependencyResolver
    {
        public static readonly IMemberApiDependencyResolver Default = new MemberApiDependencyResolver();

        private readonly IUnityContainer _container = new ApplicationContainerFactory().CreateWithRegisteredTypes();

        private MemberApiDependencyResolver()
        {
        }

        IUnityContainer IMemberApiDependencyResolver.Container
        {
            get { return _container; }
        }
    }
}