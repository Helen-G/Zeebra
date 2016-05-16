using AFT.RegoV2.Core.Brand.Data;
using AFT.RegoV2.Tests.Common.Helpers;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.Tests.Common.Base
{
    public abstract class ReportsTestsBase : AdminWebsiteUnitTestsBase
    {
        protected PaymentTestHelper PaymentTestHelper { get; set; }
        protected BrandTestHelper BrandTestHelper { get; set; }
        public SecurityTestHelper SecurityTestHelper { get; set; }
        protected Brand CurrentBrand { get; set; }

        protected const string LocalIPAddress = "127.0.0.1";

        public override void BeforeEach()
        {
            base.BeforeEach();
            PaymentTestHelper = Container.Resolve<PaymentTestHelper>();
            BrandTestHelper = Container.Resolve<BrandTestHelper>();
            SecurityTestHelper = Container.Resolve<SecurityTestHelper>();
            StartWorkers();
            SecurityTestHelper.SignInUser();
            CurrentBrand = BrandTestHelper.CreateActiveBrandWithProducts();
        }

        protected abstract void StartWorkers();

    }
}