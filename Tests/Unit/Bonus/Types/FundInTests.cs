using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Core.Bonus.Data;
using AFT.RegoV2.Core.Bonus.Data.ViewModels;
using AFT.RegoV2.Core.Bonus.EventHandlers;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Payment;
using AFT.RegoV2.Core.Common.Events.Payment;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using FluentAssertions;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Bonus.Types
{
    class FundInTests : BonusTestsBase
    {
        private Core.Bonus.Data.Bonus _bonus;
        private Guid _brandWalletId;

        public override void BeforeEach()
        {
            base.BeforeEach();
            _bonus = BonusHelper.CreateBasicBonus();
            _brandWalletId = _bonus.Template.Info.Brand.WalletTemplates.First().Id;
            _bonus.Template.Info.BonusTrigger = Trigger.FundIn;
            _bonus.Template.Rules.FundInWallets = new List<BonusFundInWallet>
            {
                new BonusFundInWallet {WalletId = _brandWalletId}
            };
        }

        [Test]
        public void Can_receive_flat_amount_fund_in_bonus()
        {
            //depositing funds to use them for fund in
            PaymentHelper.MakeDeposit(PlayerId);
            PaymentHelper.MakeFundIn(PlayerId, _brandWalletId, 100);

            var bonusRedemption = BonusRedemptions.First();

            Assert.AreEqual(ActivationStatus.Activated, bonusRedemption.ActivationState);
            Assert.AreEqual(_bonus.Template.Rules.RewardTiers.Single().BonusTiers.Single().Reward, bonusRedemption.Amount);
        }

        [TestCase(100, 10)]
        [TestCase(200, 40)]
        [TestCase(300, 50)]
        public void Can_receive_tiered_fund_in_bonus_reward(int fundInAmount, int expectedRedemptionAmount)
        {
            var bonusCode = TestDataGenerator.GetRandomString();
            var bonus = BonusHelper.CreateBonusWithBonusTiers(BonusRewardType.TieredPercentage);
            bonus.Template.Info.BonusTrigger = Trigger.FundIn;
            bonus.Template.Info.Mode = IssuanceMode.AutomaticWithCode;
            bonus.Code = bonusCode;
            bonus.Template.Rules.FundInWallets = new List<BonusFundInWallet>
            {
                new BonusFundInWallet {WalletId = _brandWalletId}
            };

            //depositing funds to use them for fund in
            PaymentHelper.MakeDeposit(PlayerId, fundInAmount);
            PaymentHelper.MakeFundIn(PlayerId, _brandWalletId, fundInAmount, bonusCode);

            var bonusRedemption = BonusRedemptions.First();

            Assert.AreEqual(ActivationStatus.Activated, bonusRedemption.ActivationState);
            Assert.AreEqual(expectedRedemptionAmount, bonusRedemption.Amount);
        }

        [Test]
        public void Min_fund_in_amount_restriction_works()
        {
            _bonus.Template.Rules.RewardTiers.Single().BonusTiers.Single().From = 100;

            //depositing funds to use them for fund in
            PaymentHelper.MakeDeposit(PlayerId);
            PaymentHelper.MakeFundIn(PlayerId, _brandWalletId, 99);

            BonusRedemptions.Should().BeEmpty();
        }

        [Test]
        public void Destination_wallet_restriction_works()
        {
            //using handler directly, 'cos MakeFundIn method does not accept invalid wallet ids
            Container.Resolve<PaymentSubscriber>().Handle(new TransferFundCreated
            {
                Amount = 100,
                DestinationWalletStructureId = Guid.NewGuid(),
                Type = TransferFundType.FundIn,
                Status = TransferFundStatus.Approved,
                PlayerId = PlayerId
            });

            BonusRedemptions.Should().BeEmpty();
        }
    }
}