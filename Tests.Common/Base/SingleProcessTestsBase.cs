using Microsoft.Practices.Unity;

namespace AFT.RegoV2.Tests.Common.Base
{
    public abstract class SingleProcessTestsBase : ContainerTestsBase
    {
        protected override IUnityContainer CreateContainer()
        {
            return new SingleProcessTestContainerFactory().CreateWithRegisteredTypes();
        }
    }
}