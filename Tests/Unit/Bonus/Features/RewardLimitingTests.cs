using System.Linq;
using AFT.RegoV2.Core.Bonus.Data;
using AFT.RegoV2.Tests.Common.Base;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Bonus.Features
{
    class RewardLimitingTests : BonusTestsBase
    {
        [Test]
        public void Percentage_bonus_reward_is_limited_by_transaction_limit()
        {
            const decimal limit = 50m;
            var bonus = BonusHelper.CreateBasicBonus();
            bonus.Template.Rules.RewardType = BonusRewardType.Percentage;
            bonus.Template.Rules.RewardTiers.Single().Tiers.Single().Reward = 0.5m;
            bonus.Template.Rules.RewardTiers.Single().Tiers.Single().MaxAmount = limit;

            PaymentHelper.MakeDeposit(PlayerId, 1000);

            Assert.AreEqual(limit, BonusRedemptions.First().Amount);
        }

        [Test]
        public void Bonus_reward_is_limited_by_RewardTier_reward_limit()
        {
            const decimal limit = 45m;
            var bonus = BonusHelper.CreateBasicBonus();
            bonus.Template.Info.DepositKind = DepositKind.Reload;
            bonus.Template.Rules.RewardTiers.Single().RewardAmountLimit = limit;

            //Make 1st deposit so 2nd and 3rd deposits will be qualified
            PaymentHelper.MakeDeposit(PlayerId);
            PaymentHelper.MakeDeposit(PlayerId);

            Assert.AreEqual(25, BonusRedemptions.First().Amount);

            PaymentHelper.MakeDeposit(PlayerId);

            Assert.AreEqual(20, BonusRedemptions.Last().Amount);
        }
    }
}