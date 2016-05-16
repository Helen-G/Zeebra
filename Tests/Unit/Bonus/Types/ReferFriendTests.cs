using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Core.Bonus.ApplicationServices;
using AFT.RegoV2.Core.Bonus.Data;
using AFT.RegoV2.Core.Bonus.Data.ViewModels;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Helpers;
using FluentAssertions;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Bonus.Types
{
    class ReferFriendTests : BonusTestsBase
    {
        private GamesTestHelper _gamesTestHelper;
        private Guid _gameId;
        public override void BeforeEach()
        {
            base.BeforeEach();
            _gamesTestHelper = Container.Resolve<GamesTestHelper>();

            var walletTemplate = BrandHelper.GetMainWalletTemplate();
            _gameId = GamesHelper.GetGameId(walletTemplate);

        }

        [Test]
        public void Can_redeem_refer_friend_bonus_of_matched_tier()
        {
            BonusHelper.CreateBonusWithReferFriendTiers();


            var bonusPlayer = BonusRepository.GetLockedPlayer(PlayerId);
            for (var i = 1; i <= 9; i++)
            {
                BonusHelper.CompleteReferAFriendRequirments(PlayerId, _gameId);

                var referRedemption = bonusPlayer.BonusesRedeemed.ElementAt(i - 1);
                Assert.AreEqual(ActivationStatus.Activated, referRedemption.ActivationState);
                Assert.AreEqual(GetExpectedRedemptionAmount(i), referRedemption.Amount);
            }
        }

        [Test]
        public void Can_redeem_refer_friend_bonus_with_exact_matched_tier()
        {
            var referTier = new BonusTier
            {
                From = 1,
                To = 1,
                Reward = 10
            };
            var bonus = BonusHelper.CreateBonusWithReferFriendTiers();
            bonus.Template.Rules.RewardTiers.Single().BonusTiers.Clear();
            bonus.Template.Rules.RewardTiers.Single().BonusTiers.Add(referTier);

            BonusHelper.CompleteReferAFriendRequirments(PlayerId, _gameId);

            var bonusPlayer = BonusRepository.GetLockedPlayer(PlayerId);
            var referRedemption = bonusPlayer.BonusesRedeemed.SingleOrDefault(x => x.ActivationState == ActivationStatus.Activated);
            Assert.NotNull(referRedemption);
            Assert.AreEqual(referTier.Reward, referRedemption.Amount);
        }

        [Test]
        public void Cannot_redeem_refer_friend_bonus_with_wager_requirement_not_met()
        {
            BonusHelper.CreateBonusWithReferFriendTiers();
            var newPlayerId = PlayerHelper.CreatePlayer(PlayerId);
            PaymentHelper.MakeDeposit(newPlayerId);

            _gamesTestHelper.PlaceAndLoseBet(90, newPlayerId, _gameId);

            var redemption = BonusRepository.GetLockedPlayer(PlayerId).BonusesRedeemed.Single();
            redemption.ActivationState.Should().Be(ActivationStatus.Pending);
        }

        [Test]
        public void Cannot_redeem_refer_friend_bonus_with_min_first_deposit_requirement_not_met()
        {
            var bonus = BonusHelper.CreateBonusWithReferFriendTiers();
            bonus.Template.Rules.ReferFriendMinDepositAmount = 300;
            var newPlayerId = PlayerHelper.CreatePlayer(PlayerId);

            PaymentHelper.MakeDeposit(newPlayerId);
            _gamesTestHelper.PlaceAndLoseBet(90, newPlayerId, _gameId);

            var redemption = BonusRepository.GetLockedPlayer(PlayerId).BonusesRedeemed.Single();
            redemption.ActivationState.Should().Be(ActivationStatus.Pending);
        }

        [Test]
        public void Playing_beyound_minimal_rollover_does_not_create_additional_bonus_redemptions()
        {
            BonusHelper.CreateBonusWithReferFriendTiers();

            var referredPlayerId = PlayerHelper.CreatePlayer(PlayerId);
            PaymentHelper.MakeDeposit(referredPlayerId);
            _gamesTestHelper.PlaceAndWinBet(200, 200, referredPlayerId, _gameId);
            _gamesTestHelper.PlaceAndWinBet(200, 200, referredPlayerId, _gameId);

            var bonusPlayer = BonusRepository.GetLockedPlayer(PlayerId);
            bonusPlayer.BonusesRedeemed.Count.Should().Be(1);
        }

        [Test]
        public void Can_not_get_2_refer_a_friend_redemptions_using_1_referred()
        {
            //Getting bonus #1
            var bonus = BonusHelper.CreateBonusWithReferFriendTiers();
            var referredPlayerId = PlayerHelper.CreatePlayer(PlayerId);
            PaymentHelper.MakeDeposit(referredPlayerId);
            _gamesTestHelper.PlaceAndWinBet(200, 200, referredPlayerId, _gameId);
            bonus.IsActive = false;

            //Trying to get bonus #2
            bonus = BonusHelper.CreateBonusWithReferFriendTiers();
            bonus.Template.Rules.ReferFriendWageringCondition = 2;
            PaymentHelper.MakeDeposit(referredPlayerId, 201);
            _gamesTestHelper.PlaceAndWinBet(201, 201, referredPlayerId, _gameId);

            var bonusPlayer = BonusRepository.GetLockedPlayer(PlayerId);
            bonusPlayer.BonusesRedeemed.Count.Should().Be(1);
        }

        [Test]
        public void Referer_can_claim_2_different_bonus_redemptions()
        {
            var bonusCommands = Container.Resolve<BonusCommands>();

            var bonus = BonusHelper.CreateBonusWithReferFriendTiers();
            bonus.Template.Info.Mode = IssuanceMode.ManualByPlayer;
            BonusHelper.CompleteReferAFriendRequirments(PlayerId, _gameId);
            bonus.IsActive = false;

            var bonus2 = BonusHelper.CreateBonusWithReferFriendTiers();
            bonus2.Template.Info.Mode = IssuanceMode.ManualByPlayer;
            BonusHelper.CompleteReferAFriendRequirments(PlayerId, _gameId);
            bonus.IsActive = true;

            var bonusPlayer = BonusRepository.GetLockedPlayer(PlayerId);
            bonusPlayer.BonusesRedeemed.Count.Should().Be(2);
            bonusPlayer.BonusesRedeemed.All(br => br.Amount == 10).Should().BeTrue();

            bonusCommands.ClaimBonusRedemption(PlayerId, bonusPlayer.BonusesRedeemed.ElementAt(0).Id);
            bonusCommands.ClaimBonusRedemption(PlayerId, bonusPlayer.BonusesRedeemed.ElementAt(1).Id);
        }

        [Test]
        public void Referer_tiers_are_bonus_dependent()
        {
            var bonus1 = BonusHelper.CreateBonusWithReferFriendTiers();
            BonusHelper.CompleteReferAFriendRequirments(PlayerId, _gameId);
            bonus1.IsActive = false;

            var bonus2 = BonusHelper.CreateBonusWithReferFriendTiers();
            bonus2.Template.Rules.RewardTiers.Single().BonusTiers = new List<TierBase>
            {
                new BonusTier {From = 1, To = 1, Reward = 10}
            };
            BonusHelper.CompleteReferAFriendRequirments(PlayerId, _gameId);
            bonus1.IsActive = true;

            var bonusPlayer = BonusRepository.GetLockedPlayer(PlayerId);
            bonusPlayer.BonusesRedeemed.Count.Should().Be(2);
            bonusPlayer.BonusesRedeemed.All(br => br.Amount == 10).Should().BeTrue();
        }

        private decimal GetExpectedRedemptionAmount(int referralCount)
        {
            if (referralCount >= 1 && referralCount <= 3)
            {
                return 10;
            }
            if (referralCount >= 4 && referralCount <= 6)
            {
                return 20;
            }
            if (referralCount >= 7 && referralCount <= 9)
            {
                return 30;
            }
            return 0;
        }
    }
}