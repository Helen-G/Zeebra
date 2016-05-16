using Microsoft.Practices.Unity;

namespace AFT.RegoV2.Tests.Common.Base
{
    /// <summary>
    /// This is a base class for all tests which are using UnityContainer for creating it's dependencies
    /// </summary>
    public abstract class ContainerTestsBase : TestsBase
    {
        protected IUnityContainer Container { get; private set; }

        public override void BeforeEach()
        {
            base.BeforeAll();

            Container = CreateContainer();
        }

        protected virtual IUnityContainer CreateContainer()
        {
            return new UnityContainer();
        }
    }
}