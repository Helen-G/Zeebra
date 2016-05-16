using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Core.Bonus.Data;
using AFT.RegoV2.Core.Bonus.Data.ViewModels;
using AFT.RegoV2.Core.Common.Events.Payment;
using AFT.RegoV2.Tests.Common.Base;
using FluentAssertions;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Bonus.Qualification
{
    class HasClaimedDisqualificationTests : BonusTestsBase
    {
        private Core.Bonus.Data.Bonus _excludedBonus1;
        private Core.Bonus.Data.Bonus _excludedBonus2;
        private Core.Bonus.Data.Bonus _bonus;

        public override void BeforeEach()
        {
            base.BeforeEach();

            //making a deposit so that reload deposit bonuses will be qualified
            PaymentHelper.MakeDeposit(PlayerId);

            _excludedBonus1 = BonusHelper.CreateBasicBonus(mode: IssuanceMode.AutomaticWithCode);
            _excludedBonus1.Template.Info.DepositKind = DepositKind.Reload;
            _excludedBonus2 = BonusHelper.CreateBasicBonus(mode: IssuanceMode.AutomaticWithCode);
            _excludedBonus2.Template.Info.DepositKind = DepositKind.Reload;

            _bonus = BonusHelper.CreateBasicBonus(mode: IssuanceMode.AutomaticWithCode);
            _bonus.Template.Info.DepositKind = DepositKind.Reload;
            _bonus.Template.Availability.ExcludeOperation = Operation.Any;
            _bonus.Template.Availability.ExcludeBonuses = new List<BonusExclude> 
            { 
                new BonusExclude { ExcludedBonusId = _excludedBonus1.Id },
                new BonusExclude { ExcludedBonusId = _excludedBonus2.Id }
            };
        }

        [Test]
        public void All_bonuses_are_qualified_if_no_exclude_bonuses_are_selected()
        {
            BonusQueries
                .GetOfflineDepositQualifiedBonuses(PlayerId)
                .Count()
                .Should()
                .Be(3);
        }

        [Test]
        public void Bonus_is_not_qualified_if_ANY_of_excluded_bonuses_were_redeemed()
        {
            PaymentHelper.MakeDeposit(PlayerId, bonusCode: _excludedBonus1.Code);

            BonusQueries.GetOfflineDepositQualifiedBonuses(PlayerId)
                .ToList()
                .Should()
                .NotContain(qualifiedBonus => qualifiedBonus.Name == _bonus.Name);
        }

        [Test]
        public void Bonus_is_qualified_if_only_one_of_ALL_excluded_bonuses_were_redeemed()
        {
            _bonus.Template.Availability.ExcludeOperation = Operation.All;

            PaymentHelper.MakeDeposit(PlayerId, bonusCode: _excludedBonus1.Code);

            BonusQueries
                .GetOfflineDepositQualifiedBonuses(PlayerId)
                .Count()
                .Should()
                .Be(3, because: "Only one of excluded bonuses was redeemed.");
        }

        [Test]
        public void Bonus_is_not_qualified_if_ALL_of_excluded_bonuses_were_redeemed()
        {
            _bonus.Template.Availability.ExcludeOperation = Operation.All;

            PaymentHelper.MakeDeposit(PlayerId, bonusCode: _excludedBonus1.Code);
            PaymentHelper.MakeDeposit(PlayerId, bonusCode: _excludedBonus2.Code);

            BonusQueries.GetOfflineDepositQualifiedBonuses(PlayerId)
                .ToList()
                .Should()
                .NotContain(qualifiedBonus => qualifiedBonus.Name == _bonus.Name);
        }

        [TestCase(ActivationStatus.Pending, ExpectedResult = 0)]
        [TestCase(ActivationStatus.Activated, ExpectedResult = 0)]
        [TestCase(ActivationStatus.Claimable, ExpectedResult = 0)]
        [TestCase(ActivationStatus.Negated, ExpectedResult = 1)]
        [TestCase(ActivationStatus.Canceled, ExpectedResult = 1)]
        public int Disqualification_is_processed_based_on_correct_bonus_redemptions(ActivationStatus status)
        {
            BonusRepository
                .Players
                .Single(p => p.Id == PlayerId)
                .Wallets
                .First()
                .BonusesRedeemed.Add(new BonusRedemption
                {
                    Bonus = _excludedBonus1,
                    ActivationState = status
                });

            return BonusQueries.GetOfflineDepositQualifiedBonuses(PlayerId).Count(b => b.Name == _bonus.Name);
        }

        [Test]
        public void Disqualification_is_processed_during_activation()
        {
            var depositId = Guid.NewGuid();
            ServiceBus.PublishMessage(new DepositSubmitted
            {
                PlayerId = PlayerId,
                Amount = 200,
                BonusCode = _bonus.Code,
                DepositId = depositId
            });
            var bonusRedemption = BonusRedemptions.First();

            PaymentHelper.MakeDeposit(PlayerId, bonusCode: _excludedBonus1.Code);

            ServiceBus.PublishMessage(new DepositApproved
            {
                PlayerId = PlayerId,
                ActualAmount = 100,
                DepositId = depositId
            });

            bonusRedemption.ActivationState.Should().Be(ActivationStatus.Negated);
        }
    }
}