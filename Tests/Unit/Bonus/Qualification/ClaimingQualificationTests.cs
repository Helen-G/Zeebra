using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Core.Bonus.Data;
using AFT.RegoV2.Core.Bonus.Data.ViewModels;
using AFT.RegoV2.Core.Common.Events.Payment;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Tests.Common.Base;
using FluentAssertions;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Bonus.Qualification
{
    class ClaimingQualificationTests : BonusTestsBase
    {
        [Test]
        public void Pending_expired_bonus_can_be_activated_during_claim_period()
        {
            var bonus = BonusHelper.CreateBasicBonus();
            bonus.DaysToClaim = 1;

            ServiceBus.PublishMessage(new DepositSubmitted
            {
                Amount = 200,
                PlayerId = PlayerId
            });

            Assert.AreEqual(ActivationStatus.Pending, BonusRedemptions.Single().ActivationState);
            //expiring the bonus
            bonus.ActiveTo = bonus.ActiveTo.AddDays(-2);
            ServiceBus.PublishMessage(new DepositApproved
            {
                ActualAmount = 200,
                PlayerId = PlayerId
            });

            Assert.AreEqual(ActivationStatus.Activated, BonusRedemptions.Single().ActivationState);
        }

        [Test]
        public void Claiming_bonus_outside_claim_duration_negates_it()
        {
            var bonus = BonusHelper.CreateBasicBonus(mode: IssuanceMode.ManualByPlayer);
            bonus.DaysToClaim = 1;

            PaymentHelper.MakeDeposit(PlayerId);
            //expiring the bonus
            bonus.ActiveTo = bonus.ActiveTo.AddDays(-3);

            var bonusRedemption = BonusRedemptions.First();
            BonusCommands.ClaimBonusRedemption(PlayerId, bonusRedemption.Id);

            bonusRedemption
                .ActivationState.Should()
                .Be(ActivationStatus.Negated);
        }

        [Test]
        public void Rest_of_qualification_is_not_processed_during_claim()
        {
            PaymentHelper.MakeDeposit(PlayerId);

            var bonus = BonusHelper.CreateBasicBonus(mode: IssuanceMode.ManualByPlayer);
            bonus.Template.Info.DepositKind = DepositKind.Reload;

            PaymentHelper.MakeDeposit(PlayerId);
            BonusCommands.ClaimBonusRedemption(PlayerId, BonusRedemptions.First().Id);

            PaymentHelper.MakeDeposit(PlayerId);
            var bonusRedemption = BonusRedemptions.Last();

            bonus.DurationType = DurationType.Custom;
            bonus.DurationStart = SystemTime.Now.AddMinutes(5);
            bonus.DurationEnd = SystemTime.Now.AddMinutes(10);
            bonus.Template.Availability.ParentBonusId = Guid.NewGuid();
            bonus.Template.Availability.VipLevels = new List<BonusVip> { new BonusVip { Code = "Bronze" } };
            bonus.Template.Availability.PlayerRedemptionsLimit = 1;

            BonusCommands.ClaimBonusRedemption(PlayerId, bonusRedemption.Id);

            bonusRedemption
                .ActivationState.Should()
                .Be(ActivationStatus.Activated);
        }
    }
}