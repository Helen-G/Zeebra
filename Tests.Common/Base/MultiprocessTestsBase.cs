using AFT.RegoV2.Infrastructure.DependencyResolution;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Common.Base
{
    /// <summary>
    /// Use this base class for tests which require multiprocess communication with third-party components like database or message bus
    /// Such tests are integration tests by nature, this is why we're marking them with "Integration" category.
    /// </summary>
    [Category("Integration"), Category("Multiprocess")]
    public abstract class MultiprocessTestsBase : ContainerTestsBase
    {

        protected override IUnityContainer CreateContainer()
        {
            //for multiprocess integration tests we are using the same ApplicationContainerFactory as real application does.
            return new ApplicationContainerFactory().CreateWithRegisteredTypes();
        }
    }
}