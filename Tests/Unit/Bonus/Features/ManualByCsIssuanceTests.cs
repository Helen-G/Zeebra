using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.ApplicationServices.Report;
using AFT.RegoV2.Core.Bonus.Data;
using AFT.RegoV2.Core.Bonus.Data.ViewModels;
using AFT.RegoV2.Core.Common.Data.Wallet;
using AFT.RegoV2.Core.Report.Data.Admin;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.WinService.Workers;
using FluentAssertions;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Bonus.Features
{
    class ManualByCsIssuanceTests : BonusTestsBase
    {
        [Test]
        public void Can_issue_first_deposit_bonus()
        {
            PaymentHelper.MakeDeposit(PlayerId);
            var transaction = BonusRepository.Players.Single(p => p.Id == PlayerId).Wallets.SelectMany(t => t.Transactions).First();
            var bonus = BonusHelper.CreateBasicBonus();

            BonusCommands.IssueBonusByCs(new IssueBonusByCsVM
            {
                BonusId = bonus.Id,
                PlayerId = PlayerId,
                TransactionId = transaction.Id
            });

            var bonusRedemption = BonusRedemptions.Single();
            bonusRedemption.ActivationState.Should().Be(ActivationStatus.Activated);
        }

        [Test]
        public void Can_issue_reload_deposit_bonus()
        {
            PaymentHelper.MakeDeposit(PlayerId);
            PaymentHelper.MakeDeposit(PlayerId);
            var transaction = BonusRepository.Players.Single(p => p.Id == PlayerId).Wallets.SelectMany(t => t.Transactions).Last();
            var bonus = BonusHelper.CreateBasicBonus();
            bonus.Template.Info.DepositKind = DepositKind.Reload;

            BonusCommands.IssueBonusByCs(new IssueBonusByCsVM
            {
                BonusId = bonus.Id,
                PlayerId = PlayerId,
                TransactionId = transaction.Id
            });

            var bonusRedemption = BonusRedemptions.Single();
            bonusRedemption.ActivationState.Should().Be(ActivationStatus.Activated);
        }

        [Test]
        public void Can_issue_fundin_bonus()
        {
            var brandWalletId = BonusRepository.Brands.First().WalletTemplates.Last().Id;
            var bonus = BonusHelper.CreateBasicBonus(mode: IssuanceMode.ManualByCs);
            bonus.Template.Info.BonusTrigger = Trigger.FundIn;
            bonus.Template.Rules.FundInWallets = new List<BonusFundInWallet>
            {
                new BonusFundInWallet {WalletId = brandWalletId}
            };

            PaymentHelper.MakeDeposit(PlayerId);
            PaymentHelper.MakeFundIn(PlayerId, brandWalletId, 100);

            var transaction = BonusRepository.Players.Single(p => p.Id == PlayerId).Wallets.SelectMany(t => t.Transactions).Single(t => t.Type == TransactionType.FundIn);

            BonusCommands.IssueBonusByCs(new IssueBonusByCsVM
            {
                BonusId = bonus.Id,
                PlayerId = PlayerId,
                TransactionId = transaction.Id
            });

            var bonusRedemption = BonusRedemptions.Single();
            bonusRedemption.ActivationState.Should().Be(ActivationStatus.Activated);
        }

        [Test]
        public void System_does_not_claim_ManualByPlayer_bonus_issued_by_CS()
        {
            PaymentHelper.MakeDeposit(PlayerId);
            var transaction = BonusRepository.Players.Single(p => p.Id == PlayerId).Wallets.SelectMany(t => t.Transactions).First();
            var bonus = BonusHelper.CreateBasicBonus(mode: IssuanceMode.ManualByPlayer);

            BonusCommands.IssueBonusByCs(new IssueBonusByCsVM
            {
                BonusId = bonus.Id,
                PlayerId = PlayerId,
                TransactionId = transaction.Id
            });

            var bonusRedemption = BonusRedemptions.Single();
            bonusRedemption.ActivationState.Should().Be(ActivationStatus.Claimable);
        }

        [Test]
        public void System_claims_ManualByCs_bonus()
        {
            PaymentHelper.MakeDeposit(PlayerId);
            var transaction = BonusRepository.Players.Single(p => p.Id == PlayerId).Wallets.SelectMany(t => t.Transactions).First();
            var bonus = BonusHelper.CreateBasicBonus(mode: IssuanceMode.ManualByCs);

            BonusCommands.IssueBonusByCs(new IssueBonusByCsVM
            {
                BonusId = bonus.Id,
                PlayerId = PlayerId,
                TransactionId = transaction.Id
            });

            var bonusRedemption = BonusRedemptions.Single();
            bonusRedemption.ActivationState.Should().Be(ActivationStatus.Activated);
        }

        [Test]
        public void Before_wager_bonus_isnot_issued_before_wagering_requirement_is_fulfilled()
        {
            PaymentHelper.MakeDeposit(PlayerId);
            var transaction = BonusRepository.Players.Single(p => p.Id == PlayerId).Wallets.SelectMany(t => t.Transactions).First();
            var bonus = BonusHelper.CreateBasicBonus(mode: IssuanceMode.ManualByCs);
            bonus.Template.Wagering.HasWagering = true;
            bonus.Template.Wagering.Multiplier = 2;
            bonus.Template.Wagering.IsAfterWager = true;

            BonusCommands.IssueBonusByCs(new IssueBonusByCsVM
            {
                BonusId = bonus.Id,
                PlayerId = PlayerId,
                TransactionId = transaction.Id
            });

            var bonusRedemption = BonusRedemptions.Single();
            bonusRedemption.ActivationState.Should().Be(ActivationStatus.Pending);
            bonusRedemption.RolloverState.Should().Be(RolloverStatus.Active);
        }

        [Test]
        public void Issuance_is_logged_to_admin_activity_log()
        {
            Container.Resolve<AdminActivityLogWorker>().Start();

            PaymentHelper.MakeDeposit(PlayerId);
            var transaction = BonusRepository.Players.Single(p => p.Id == PlayerId).Wallets.SelectMany(t => t.Transactions).First();
            var bonus = BonusHelper.CreateBasicBonus();

            BonusCommands.IssueBonusByCs(new IssueBonusByCsVM
            {
                BonusId = bonus.Id,
                PlayerId = PlayerId,
                TransactionId = transaction.Id
            });

            Container.Resolve<ReportQueries>()
                .GetAdminActivityLog()
                .SingleOrDefault(e => e.Category == AdminActivityLogCategory.Bonus && e.Remarks.Contains("TransactionId"))
                .Should()
                .NotBeNull();
        }

        [Test]
        public void Bonus_issued_by_Cs_ignores_bonus_claim_duration_qualification()
        {
            PaymentHelper.MakeDeposit(PlayerId);
            var transaction = BonusRepository.Players.Single(p => p.Id == PlayerId).Wallets.SelectMany(t => t.Transactions).First();
            var bonus = BonusHelper.CreateBasicBonus(mode: IssuanceMode.ManualByPlayer);
            bonus.DaysToClaim = 1;

            BonusCommands.IssueBonusByCs(new IssueBonusByCsVM
            {
                BonusId = bonus.Id,
                PlayerId = PlayerId,
                TransactionId = transaction.Id
            });

            var bonusRedemption = BonusRedemptions.Single();

            //expiring the bonus
            bonus.ActiveTo = bonus.ActiveTo.AddDays(-2);
            BonusCommands.ClaimBonusRedemption(PlayerId, bonusRedemption.Id);

            bonusRedemption.ActivationState.Should().Be(ActivationStatus.Activated);
        }
    }
}