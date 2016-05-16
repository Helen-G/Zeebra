using AFT.RegoV2.Core.Brand;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Tests.Common.Helpers;
using AFT.RegoV2.WinService.Workers;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.Tests.Common.Base
{
    public abstract class BrandTestsBase : PermissionsTestsBase
    {
        protected BrandTestHelper BrandHelper { get; set; }
        protected PaymentTestHelper PaymentHelper { get; set; }
        protected BrandCommands BrandCommands { get; set; }
        protected BrandQueries BrandQueries { get; set; }
        protected LicenseeCommands LicenseeCommands { get; set; }
        protected IBrandRepository BrandRepository { get; set; }

        public override void BeforeEach()
        {
            base.BeforeEach();

            BrandHelper = Container.Resolve<BrandTestHelper>();
            BrandCommands = Container.Resolve<BrandCommands>();
            BrandQueries = Container.Resolve<BrandQueries>();
            LicenseeCommands = Container.Resolve<LicenseeCommands>();
            BrandRepository = Container.Resolve<IBrandRepository>();
            PaymentHelper = Container.Resolve<PaymentTestHelper>();
            SecurityTestHelper = Container.Resolve<SecurityTestHelper>();
            SecurityTestHelper.PopulatePermissions();
            SecurityTestHelper.SignInUser();
            Container.Resolve<BonusWorker>().Start();
        }
    }
}