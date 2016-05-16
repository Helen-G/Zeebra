using AFT.RegoV2.Tests.Common.Containers;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.Tests.Common.Base
{
    /// <summary>
    /// this class is needed in order to create Unity container instance through our factory, because SpecFlow IoC capabilities are rudimentary
    /// </summary>
    public class SpecFlowContainerFactory
    {
        private IUnityContainer _container;
        private readonly object _syncRoot = new object();

        public IUnityContainer GetOrCreate()
        {
            if (_container != null)
                return _container;

            lock (_syncRoot)
            {
                return _container = new AdminWebsiteUnitTestContainerFactory().CreateWithRegisteredTypes();
            }
        }
    }
}