using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Core.Bonus.Data;
using AFT.RegoV2.Core.Bonus.Data.ViewModels;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Events.Payment;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Helpers;
using FluentAssertions;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Bonus.Rollover
{
    class RolloverFulfillmentTests : BonusTestsBase
    {
        private Core.Game.Data.Wallet _wallet;
        private Core.Bonus.Data.Bonus _bonus;
        private GamesTestHelper _gamesTestHelper;
        private Guid _gameId;

        public override void BeforeEach()
        {
            base.BeforeEach();

            var gameRepository = Container.Resolve<IGameRepository>();

            _gamesTestHelper = Container.Resolve<GamesTestHelper>();
            _wallet = gameRepository.Wallets.Single(a => a.PlayerId == PlayerId && a.Template.IsMain);


            _gameId = BrandHelper.GetMainWalletGameId(PlayerId);

            _bonus = BonusHelper.CreateBasicBonus();
            _bonus.Template.Rules.RewardTiers.Single().BonusTiers.Single().Reward = 100;
            _bonus.Template.Wagering.HasWagering = true;
            _bonus.Template.Wagering.Multiplier = 3m;
        }

        [TestCase(true, TestName = "Bet won increases wagering completed")]
        [TestCase(false, TestName = "Bet lost increases wagering completed")]
        public void Bet_outcome_increases_wagering_completed(bool winBet)
        {
            PaymentHelper.MakeDeposit(PlayerId);
            if (winBet)
            {
                _gamesTestHelper.PlaceAndWinBet(25, 50, PlayerId, _gameId);
            }
            else
            {
                _gamesTestHelper.PlaceAndLoseBet(25, PlayerId, _gameId);
            }

            BonusRedemptions.First().Contributions.SingleOrDefault(c => c.Contribution == 25).Should().NotBeNull();
        }

        [Test]
        public void Placing_a_bet_does_not_change_wagering_completed()
        {
            PaymentHelper.MakeDeposit(PlayerId);
            _gamesTestHelper.PlaceBet(25, PlayerId, _gameId);

            BonusRedemptions.First().Contributions.Should().BeEmpty();
        }

        [Test]
        public void Meeting_wagering_threshold_completes_wagering()
        {
            _bonus.Template.Wagering.Threshold = 200;
            PaymentHelper.MakeDeposit(PlayerId);
            _gamesTestHelper.PlaceAndLoseBet(100, PlayerId, _gameId);

            var bonusRedemption = BonusRedemptions.First();

            bonusRedemption.RolloverState.Should().Be(RolloverStatus.ZeroedOut);
            var thresholdContribution = bonusRedemption.Contributions.ElementAt(1);
            thresholdContribution.Type.Should().Be(ContributionType.Threshold);
            thresholdContribution.Contribution.Should().Be(200);
        }

        [Test]
        public void Playable_balance_is_taken_into_account_to_trigger_wagering_threshold()
        {
            _bonus.Template.Wagering.Threshold = 125;
            PaymentHelper.MakeDeposit(PlayerId);
            Container.Resolve<IWalletCommands>().Lock(PlayerId, new LockUnlockParams { Amount = 100, Type = LockType.Withdrawal });
            //Total balance: 225
            //Playable balance: 125

            _gamesTestHelper.PlaceAndLoseBet(75, PlayerId, _gameId);
            //Total balance: 150
            //Playable balance: 50
            BonusRedemptions.Single().RolloverState.Should().Be(RolloverStatus.ZeroedOut);
        }

        [Test]
        public void Bet_outcome_decreases_RolloverLeft_of_bonus_redemptions_one_by_one()
        {
            var bonus2 = BonusHelper.CreateBasicBonus();
            bonus2.Template.Rules.RewardTiers.Single().BonusTiers.Single().Reward = 100;
            bonus2.Template.Wagering.HasWagering = true;
            bonus2.Template.Wagering.Multiplier = 3m;

            PaymentHelper.MakeDeposit(PlayerId, 400);

            var redemption1 = BonusRedemptions.First();
            var redemption2 = BonusRedemptions.Last();
            _gamesTestHelper.PlaceAndLoseBet(500, PlayerId, _gameId);

            redemption1.RolloverState.Should().Be(RolloverStatus.Completed);
            redemption1.Contributions.SingleOrDefault(c => c.Type == ContributionType.Bet).Should().NotBeNull();
            redemption2.RolloverState.Should().Be(RolloverStatus.Active);
            redemption2.Contributions.SingleOrDefault(c => c.Contribution == 200).Should().NotBeNull();
        }

        [Test]
        public void Bet_outcome_decreases_RolloverLeft_of_bonus_redemption_that_was_created_first()
        {
            var bonus2 = BonusHelper.CreateBasicBonus();
            bonus2.Template.Rules.RewardTiers.Single().BonusTiers.Single().Reward = 100;
            bonus2.Template.Wagering.HasWagering = true;
            bonus2.Template.Wagering.Multiplier = 3m;

            PaymentHelper.MakeDeposit(PlayerId, 400);

            var redemption1 = BonusRedemptions.First();
            var redemption2 = BonusRedemptions.Last();
            BonusRepository.GetLockedPlayer(PlayerId).Data.Wallets.First().BonusesRedeemed = new List<BonusRedemption> { redemption2, redemption1 };
            _gamesTestHelper.PlaceAndLoseBet(50, PlayerId, _gameId);

            redemption1.Contributions.SingleOrDefault(c => c.Type == ContributionType.Bet).Should().NotBeNull();
            redemption2.Contributions.Should().BeEmpty();
        }

        [Test]
        public void Bet_outcome_decreases_RolloverLeft_of_BR_fulfillment_of_which_started()
        {
            var bonus2 = BonusHelper.CreateBasicBonus(mode: IssuanceMode.AutomaticWithCode);
            bonus2.Template.Rules.RewardTiers.Single().BonusTiers.Single().Reward = 100;
            bonus2.Template.Wagering.HasWagering = true;
            bonus2.Template.Wagering.Multiplier = 3m;

            var depositId = Guid.NewGuid();
            ServiceBus.PublishMessage(new DepositSubmitted
            {
                PlayerId = PlayerId,
                Amount = 200,
                DepositId = depositId
            });
            PaymentHelper.MakeDeposit(PlayerId, 400, bonus2.Code);
            //starting to fulfill rollover of bonus redemption tied to bonus2
            _gamesTestHelper.PlaceAndLoseBet(50, PlayerId, _gameId);

            ServiceBus.PublishMessage(new DepositApproved
            {
                PlayerId = PlayerId,
                ActualAmount = 200,
                DepositId = depositId
            });

            var redemption1 = BonusRedemptions.First();
            var redemption2 = BonusRedemptions.Last();

            _gamesTestHelper.PlaceAndLoseBet(50, PlayerId, _gameId);
            
            redemption1.Contributions.Should().BeEmpty();
            redemption2.Contributions.Count(c => c.Type == ContributionType.Bet).Should().Be(2);
        }

        [Test]
        public void Bet_to_rollover_contribution_is_calculated_as_bets_amount_average()
        {
            PaymentHelper.MakeDeposit(PlayerId, 500);
            var roundId = Guid.NewGuid().ToString();
            _gamesTestHelper.PlaceBet(200, PlayerId, _gameId, roundId);
            _gamesTestHelper.PlaceBet(100, PlayerId, _gameId, roundId);
            _gamesTestHelper.LoseBet(roundId);

            BonusRedemptions.First().Contributions.SingleOrDefault(c => c.Contribution == 150).Should().NotBeNull();
        }

        [Test]
        public void All_contribution_are_calculated_as_100_percents_when_no_game_to_rollover_contributions_defined()
        {
            PaymentHelper.MakeDeposit(PlayerId);
            _gamesTestHelper.PlaceAndLoseBet(25, PlayerId, _gameId);

            BonusRedemptions.First().Contributions.SingleOrDefault(c => c.Contribution == 25).Should().NotBeNull();
        }

        [Test]
        public void Contribution_is_calculated_using_multiplier_when_game_to_rollover_contributions_are_defined()
        {
            var gameId = _gameId;
            _bonus.Template.Wagering.GameContributions.Add(new GameContribution
            {
                Contribution = 0.5m,
                GameId = gameId
            });
            PaymentHelper.MakeDeposit(PlayerId);
            _gamesTestHelper.PlaceAndLoseBet(25, PlayerId, gameId);

            BonusRedemptions.First().Contributions.SingleOrDefault(c => c.Contribution == 12.5m).Should().NotBeNull();
        }

        [Test]
        public void Contribution_is_calculated_as_100_percents_for_game_that_is_not_on_GameContributions_list()
        {
            _bonus.Template.Wagering.GameContributions.Add(new GameContribution
            {
                Contribution = 0.5m,
                GameId = Guid.NewGuid()
            });
            PaymentHelper.MakeDeposit(PlayerId);
            _gamesTestHelper.PlaceAndLoseBet(25, PlayerId, _gameId);

            BonusRedemptions.First().Contributions.SingleOrDefault(c => c.Contribution == 25).Should().NotBeNull();
        }

        [Test]
        public void Winnings_from_bonus_are_transferred_to_main_once_wagering_is_completed()
        {
            var bonus = BonusHelper.CreateBasicBonus(mode: IssuanceMode.AutomaticWithCode);
            bonus.Template.Rules.RewardTiers.First().BonusTiers.First().Reward = 100;
            bonus.Template.Wagering.HasWagering = true;
            bonus.Template.Wagering.Method = WageringMethod.Bonus;
            bonus.Template.Wagering.Multiplier = 2.75m;
            PaymentHelper.MakeDeposit(PlayerId, 200, bonus.Code);

            _gamesTestHelper.PlaceAndLoseBet(250, PlayerId, _gameId);
            _gamesTestHelper.PlaceAndWinBet(25, 1000, PlayerId, _gameId);

            _wallet.Main.Should().Be(912.5m);
            _wallet.Bonus.Should().Be(112.5m);
        }

        [Test]
        public void Winnings_from_bonus_and_bonus_are_transferred_to_main_once_wagering_is_completed()
        {
            var bonus = BonusHelper.CreateBasicBonus(mode: IssuanceMode.AutomaticWithCode);
            bonus.Template.Rules.RewardTiers.First().BonusTiers.First().Reward = 100;
            bonus.Template.Info.IsWithdrawable = true;
            bonus.Template.Wagering.HasWagering = true;
            bonus.Template.Wagering.Method = WageringMethod.Bonus;
            bonus.Template.Wagering.Multiplier = 2.75m;
            PaymentHelper.MakeDeposit(PlayerId, 200, bonus.Code);

            _gamesTestHelper.PlaceAndLoseBet(250, PlayerId, _gameId);
            _gamesTestHelper.PlaceAndWinBet(25, 1000, PlayerId, _gameId);

            _wallet.Main.Should().Be(1012.5m);
            _wallet.Bonus.Should().Be(12.5m);
        }

        [Test]
        public void Wallet_HasWagering_flag_is_true_when_player_has_active_rollover()
        {
            PaymentHelper.MakeDeposit(PlayerId);

            _wallet.HasWageringRequirement.Should().BeTrue();
        }

        [Test]
        public void Wallet_HasWagering_flag_is_false_when_player_completed_rollover()
        {
            PaymentHelper.MakeDeposit(PlayerId);
            _gamesTestHelper.PlaceAndLoseBet(300, PlayerId, _gameId);

            _wallet.HasWageringRequirement.Should().BeFalse();
        }

        [Test]
        public void Bonus_redemption_without_rollover_has_None_rollover_status_after_activation()
        {
            _bonus.Template.Wagering.HasWagering = false;

            PaymentHelper.MakeDeposit(PlayerId);

            Assert.AreEqual(RolloverStatus.None, BonusRedemptions.First().RolloverState);
        }

        [Test]
        public void Bonus_redemption_with_rollover_has_Active_rollover_status_after_activation()
        {
            PaymentHelper.MakeDeposit(PlayerId);

            Assert.AreEqual(RolloverStatus.Active, BonusRedemptions.First().RolloverState);
        }

        [Test]
        public void Wagering_completed_sets_rollover_status_to_Completed()
        {
            var bonus = BonusHelper.CreateBasicBonus(mode: IssuanceMode.AutomaticWithCode);
            bonus.Template.Info.IsWithdrawable = true;
            bonus.Template.Wagering.HasWagering = true;
            bonus.Template.Wagering.Method = WageringMethod.Bonus;
            bonus.Template.Wagering.Multiplier = 1;

            PaymentHelper.MakeDeposit(PlayerId, bonusCode: bonus.Code);
            _gamesTestHelper.PlaceAndLoseBet(25, PlayerId, _gameId);

            Assert.AreEqual(RolloverStatus.Completed, BonusRedemptions.First().RolloverState);
        }
    }
}