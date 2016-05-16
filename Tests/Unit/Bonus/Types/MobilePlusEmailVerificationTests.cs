using System.Linq;
using AFT.RegoV2.Core.Bonus.Data;
using AFT.RegoV2.Core.Common.Events.Player;
using AFT.RegoV2.Tests.Common.Base;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Bonus.Types
{
    class MobilePlusEmailVerificationTests : BonusTestsBase
    {
        [Test]
        public void Can_activate_mobile_plus_email_verification_bonus()
        {
            var bonus = BonusHelper.CreateBasicBonus();
            bonus.Template.Info.BonusTrigger = Trigger.MobilePlusEmailVerification;

            var player = PlayerHelper.CreatePlayer();
            var bonusRedemption = BonusRepository.Players.Single(p => p.Id == player.Id).Wallets.First().BonusesRedeemed.First();

            Assert.AreEqual(ActivationStatus.Pending, bonusRedemption.ActivationState);
            Assert.AreEqual(25, bonusRedemption.Amount);

            ServiceBus.PublishMessage(new PlayerContactVerified(player.Id, ContactType.Mobile));
            ServiceBus.PublishMessage(new PlayerContactVerified(player.Id, ContactType.Email));

            Assert.AreEqual(ActivationStatus.Activated, bonusRedemption.ActivationState);
            Assert.AreEqual(25, bonusRedemption.Amount);
        }

        [TestCase(ContactType.Mobile)]
        [TestCase(ContactType.Email)]
        public void Cannot_redeem_mobile_plus_email_verification_bonus_without_both_verified(ContactType contactType)
        {
            var bonus = BonusHelper.CreateBasicBonus();
            bonus.Template.Info.BonusTrigger = Trigger.MobilePlusEmailVerification;

            var player = PlayerHelper.CreatePlayer();
            var bonusRedemption = BonusRepository.Players.Single(p => p.Id == player.Id).Wallets.First().BonusesRedeemed.First();

            ServiceBus.PublishMessage(new PlayerContactVerified(player.Id, contactType));

            Assert.AreEqual(ActivationStatus.Pending, bonusRedemption.ActivationState);
        }
    }
}