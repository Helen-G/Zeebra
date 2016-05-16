using System.Linq;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Helpers;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Wallet
{
    public class WalletCreationTests : AdminWebsiteUnitTestsBase
    {
        public override void BeforeEach()
        {
            base.BeforeEach();

            Container.Resolve<SecurityTestHelper>().SignInUser();
            Container.Resolve<BrandTestHelper>().CreateBrand();
        }

        [Test]
        public void Main_and_product_wallets_are_created_for_registered_player()
        {
            var player = Container.Resolve<PlayerTestHelper>().CreatePlayer();

            var wallets = Container.Resolve<IGameRepository>().Wallets.Where(x => x.PlayerId == player.Id).ToArray();
            Assert.IsNotEmpty(wallets);
            Assert.AreEqual(2, wallets.Length);
            Assert.IsTrue(wallets.Any(w => w.Template.IsMain));
            Assert.IsTrue(wallets.Any(w => w.Template.IsMain == false));
        }
    }
}