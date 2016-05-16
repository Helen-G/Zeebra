using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Core.Bonus.Data;
using AFT.RegoV2.Core.Common.Events.Payment;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Tests.Common.Base;
using FluentAssertions;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Bonus.Qualification
{
    class ActivationQualificationTests : BonusTestsBase
    {
        [Test]
        public void Pending_deposit_bonus_redemption_is_negated_if_deposit_amount_became_unqualified()
        {
            var bonus = BonusHelper.CreateBasicBonus();
            bonus.Template.Rules.RewardTiers.Single().BonusTiers.Single().From = 150;

            var depositId = Guid.NewGuid();
            ServiceBus.PublishMessage(new DepositSubmitted
            {
                PlayerId = PlayerId,
                Amount = 200,
                DepositId = depositId
            });
            ServiceBus.PublishMessage(new DepositApproved
            {
                PlayerId = PlayerId,
                ActualAmount = 100,
                DepositId = depositId
            });

            BonusRedemptions.First().ActivationState.Should().Be(ActivationStatus.Negated);
        }

        [Test]
        public void Per_player_bonus_issuance_limit_qualification_is_processed_during_activation()
        {
            PaymentHelper.MakeDeposit(PlayerId);

            var bonus = BonusHelper.CreateBasicBonus();
            bonus.Template.Availability.PlayerRedemptionsLimit = 1;
            bonus.Template.Info.DepositKind = DepositKind.Reload;

            var depositId = Guid.NewGuid();
            ServiceBus.PublishMessage(new DepositSubmitted
            {
                PlayerId = PlayerId,
                Amount = 200,
                DepositId = depositId
            });

            PaymentHelper.MakeDeposit(PlayerId);

            ServiceBus.PublishMessage(new DepositApproved
            {
                PlayerId = PlayerId,
                ActualAmount = 200,
                DepositId = depositId
            });

            BonusRedemptions.First().ActivationState.Should().Be(ActivationStatus.Negated);
        }

        [Test]
        public void FraudRiskLevel_qualification_is_processed_during_activation()
        {
            var riskLevelId = Guid.NewGuid();
            var bonus = BonusHelper.CreateBasicBonus();
            bonus.Template.Availability.ExcludeRiskLevels = new List<RiskLevelExclude> { new RiskLevelExclude { ExcludedRiskLevelId = riskLevelId } };

            var depositId = Guid.NewGuid();
            ServiceBus.PublishMessage(new DepositSubmitted
            {
                PlayerId = PlayerId,
                Amount = 200,
                DepositId = depositId
            });

            var player = BonusRepository.Players.Single(x => x.Id == PlayerId);
            player.RiskLevels = new List<RiskLevel>
            {
                new RiskLevel
                {
                    Id = riskLevelId,
                    IsActive = true
                }
            };

            ServiceBus.PublishMessage(new DepositApproved
            {
                PlayerId = PlayerId,
                ActualAmount = 200,
                DepositId = depositId
            });

            BonusRedemptions.First().ActivationState.Should().Be(ActivationStatus.Negated);
        }

        //Has claimed disqualification test in a separate test class

        [Test]
        public void Rest_of_qualification_is_not_processed_during_activation()
        {
            var bonus = BonusHelper.CreateBasicBonus();
            var depositId = Guid.NewGuid();
            ServiceBus.PublishMessage(new DepositSubmitted
            {
                PlayerId = PlayerId,
                Amount = 100,
                DepositId = depositId
            });

            bonus.DurationType = DurationType.Custom;
            bonus.DurationStart = SystemTime.Now.AddMinutes(5);
            bonus.DurationEnd = SystemTime.Now.AddMinutes(10);
            bonus.Template.Availability.ParentBonusId = Guid.NewGuid();
            bonus.Template.Availability.VipLevels = new List<BonusVip> { new BonusVip { Code = "Bronze" } };

            ServiceBus.PublishMessage(new DepositApproved
            {
                PlayerId = PlayerId,
                ActualAmount = 100,
                DepositId = depositId
            });

            BonusRedemptions.First()
                .ActivationState.Should()
                .Be(ActivationStatus.Activated);
        }
    }
}