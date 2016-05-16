using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Core.Bonus;
using AFT.RegoV2.Core.Bonus.ApplicationServices;
using AFT.RegoV2.Core.Bonus.Data;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Tests.Common.Helpers;
using AFT.RegoV2.Tests.Common.TestDoubles;
using AFT.RegoV2.WinService.Workers;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.Tests.Common.Base
{
    public abstract class BonusTestsBase : AdminWebsiteUnitTestsBase
    {
        protected BonusTestHelper BonusHelper { get; set; }
        protected BrandTestHelper BrandHelper { get; set; }
        protected GamesTestHelper GamesHelper { get; set; }
        protected PlayerTestHelper PlayerHelper { get; set; }
        protected PaymentTestHelper PaymentHelper { get; set; }
        protected IBonusRepository BonusRepository { get; set; }
        protected BonusCommands BonusCommands { get; set; }
        protected BonusManagementCommands BonusManagementCommands { get; set; }
        protected BonusQueries BonusQueries { get; set; }
        protected FakeServiceBus ServiceBus { get; set; }
        protected System.Guid PlayerId { get; set; }
        protected List<BonusRedemption> BonusRedemptions { get
        {
            return BonusRepository.GetLockedPlayer(PlayerId).Data.Wallets.First().BonusesRedeemed;
        } }

        public override void BeforeEach()
        {
            base.BeforeEach();
            
            BonusCommands = Container.Resolve<BonusCommands>();
            BonusManagementCommands = Container.Resolve<BonusManagementCommands>();
            BonusQueries = Container.Resolve<BonusQueries>();
            BonusRepository = Container.Resolve<IBonusRepository>(); 
            BonusHelper = Container.Resolve<BonusTestHelper>();
            ServiceBus = (FakeServiceBus)Container.Resolve<IServiceBus>();
            BrandHelper = Container.Resolve<BrandTestHelper>();
            GamesHelper = Container.Resolve<GamesTestHelper>();
            PlayerHelper = Container.Resolve<PlayerTestHelper>();
            PaymentHelper = Container.Resolve<PaymentTestHelper>();
            Container.Resolve<SecurityTestHelper>().SignInUser();
            Container.Resolve<BonusWorker>().Start();

            BrandHelper.CreateActiveBrandWithProducts();
            PlayerId = PlayerHelper.CreatePlayer(null);
        }
    }
}