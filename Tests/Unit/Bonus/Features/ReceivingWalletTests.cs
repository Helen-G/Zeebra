using System.Linq;
using AFT.RegoV2.Core.Brand;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.Tests.Common.Base;
using FluentAssertions;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Bonus.Features
{
    class ReceivingWalletTests : BonusTestsBase
    {
        [Test]
        public void Bonus_is_issued_to_the_correct_player_wallet()
        {
            var brandRepository = Container.Resolve<IBrandRepository>();
            var walletTemplate = brandRepository.Brands.Single().WalletTemplates.Single(t => t.IsMain == false);
            var bonus = BonusHelper.CreateBasicBonus();
            bonus.Template.Info.WalletTemplateId = walletTemplate.Id;

            PaymentHelper.MakeDeposit(PlayerId);
            var walletRepository = Container.Resolve<IGameRepository>();
            var wallet = walletRepository.Wallets.Single(w => w.Template.Id == walletTemplate.Id);

            wallet.Bonus.Should().Be(25);
        }
    }
}