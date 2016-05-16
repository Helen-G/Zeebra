using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Configuration;
using AFT.RegoV2.Core.Bonus.Data;
using AFT.RegoV2.Core.Bonus.Data.Notifications;
using AFT.RegoV2.Core.Bonus.Data.ViewModels;
using AFT.RegoV2.Core.Bonus.Entities;
using AFT.RegoV2.Core.Bonus.Resources;
using AFT.RegoV2.Core.Common.Commands;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Wallet;
using AFT.RegoV2.Core.Common.Events.Bonus;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Shared;
using BonusRedemption = AFT.RegoV2.Core.Bonus.Data.BonusRedemption;
using Player = AFT.RegoV2.Core.Bonus.Entities.Player;

namespace AFT.RegoV2.Core.Bonus.ApplicationServices
{
    public class BonusCommands : IApplicationService
    {
        private readonly IBonusRepository _repository;
        private readonly IWalletCommands _walletCommands;
        private readonly BonusQueries _bonusQueries;
        private readonly IServiceBus _bus;
        private readonly IMessageTemplateService _messageTemplateService;
        private readonly IWalletQueries _walletQueries;

        public BonusCommands(
            IBonusRepository repository,
            IWalletCommands walletCommands,
            BonusQueries bonusQueries,
            IServiceBus bus,
            IMessageTemplateService messageTemplateService,
            IWalletQueries walletQueries)
        {
            _repository = repository;
            _walletCommands = walletCommands;
            _bonusQueries = bonusQueries;
            _bus = bus;
            _messageTemplateService = messageTemplateService;
            _walletQueries = walletQueries;
        }

        public void ClaimBonusRedemption(Guid playerId, Guid redemptionId)
        {
            var redemption = _repository.GetBonusRedemption(playerId, redemptionId);
            ClaimBonusRedemption(redemption);
            _repository.SaveChanges();
        }
        public void CancelBonusRedemption(Guid playerId, Guid bonusRedemptionId)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var bonusRedemption = _repository.GetBonusRedemption(playerId, bonusRedemptionId);
                if (bonusRedemption.Data.RolloverState != RolloverStatus.Active)
                    throw new RegoException(ValidatorMessages.BonusRedemptionStatusIsIncorrectForCancellation);

                var balances = _walletQueries.GetPlayerBalance(playerId);
                var mainBalance = balances.Main;
                var bonusBalance = balances.Bonus;
                var adjustment = new AdjustmentParams(AdjustmentReason.BonusCancelled);

                var transactionsDuringRollover = GetTransactionsDuringRollover(bonusRedemption);

                var betPlacedDuringRollover = transactionsDuringRollover
                    .Where(tr => tr.Type == TransactionType.BetPlaced)
                    .ToList();

                var realMoneyContribution = betPlacedDuringRollover.Sum(tr => tr.MainBalanceAmount);
                var betPlacedTotalDuringRollover = betPlacedDuringRollover.Sum(tr => tr.TotalAmount);

                var betWonTotalDuringRollover = transactionsDuringRollover
                    .Where(tr => tr.Type == TransactionType.BetWon)
                    .Sum(tr => tr.TotalAmount);

                var netWinLoss = betWonTotalDuringRollover - betPlacedTotalDuringRollover;
                //only net losses should be adjusted
                if (netWinLoss > 0)
                {
                    netWinLoss = 0m;
                }
                var mainBalanceAdjustment = realMoneyContribution + netWinLoss;
                if (mainBalanceAdjustment > 0)
                {
                    mainBalance += mainBalanceAdjustment;
                    adjustment.MainBalanceAdjustment += mainBalanceAdjustment;
                }

                var bonusBalanceAdjustment = betWonTotalDuringRollover + bonusRedemption.Data.Amount;
                if (bonusBalanceAdjustment > 0)
                {
                    var amountFromBonus = Math.Min(bonusBalance, bonusBalanceAdjustment);
                    adjustment.BonusBalanceAdjustment -= amountFromBonus;
                    var wallet = bonusRedemption.Wallet;
                    var newWalletBalance = bonusBalance - bonusBalanceAdjustment;
                    if (newWalletBalance < wallet.NotWithdrawableBalance)
                    {
                        wallet.NotWithdrawableBalance = newWalletBalance;
                    }

                    var amountFromMain = Math.Min(mainBalance, bonusBalanceAdjustment - amountFromBonus);
                    adjustment.MainBalanceAdjustment -= amountFromMain;
                }

                bonusRedemption.Cancel();
                _walletCommands.AdjustBalances(playerId, adjustment);
                IssueUnlock(bonusRedemption);

                _repository.SaveChanges();
                scope.Complete();
            }
        }
        public void IssueBonusByCs(IssueBonusByCsVM model)
        {
            var validationResult = _bonusQueries.GetValidationResult(model);
            if (validationResult.IsValid == false)
                throw new RegoException(validationResult.Errors.First().ErrorMessage);

            var transaction =
                _repository.Players.Single(p => p.Id == model.PlayerId)
                    .Wallets.SelectMany(w => w.Transactions)
                    .Single(t => t.Id == model.TransactionId);
            var redemptionParams = new RedemptionParams
            {
                IsIssuedByCs = true,
                TransferAmount = transaction.TotalAmount
            };
            var bonusRedemption = RedeemBonus(model.PlayerId, model.BonusId, redemptionParams);
            ProcessBonusRedemptionLifecycle(bonusRedemption);

            _bus.PublishMessage(new BonusIssuedByCs
            {
                BonusId = model.BonusId,
                PlayerId = model.PlayerId,
                TransactionId = model.TransactionId,
                Description = string.Format(
                "Bonus ({0}) of {1}{2} was issued to Player ({3})", 
                bonusRedemption.Data.Bonus.Name, 
                bonusRedemption.Data.Player.CurrencyCode,
                bonusRedemption.Data.Amount.ToString("N"),
                bonusRedemption.Data.Player.Name)
            });

            _repository.SaveChanges();
        }

        internal void ProcessFirstBonusRedemptionOfTrigger(Player player, Trigger trigger)
        {
            var bonusRedemption =
                player.BonusesRedeemed
                .OrderBy(r => r.CreatedOn)
                .FirstOrDefault(r =>
                        r.Bonus.Template.Info.BonusTrigger == trigger &&
                        r.ActivationState == ActivationStatus.Pending);
            if (bonusRedemption != null)
            {
                ProcessBonusRedemptionLifecycle(new Entities.BonusRedemption(bonusRedemption));
            }
        }
        internal void ProcessHighDepositBonus(Player player)
        {
            SendHighDepositBonusSmsNotifications(player);
            while (_bonusQueries.GetQualifiedHighDepositBonuses(player).Any())
            {
                var bonusData = _bonusQueries.GetQualifiedHighDepositBonuses(player).First();
                var bonusRedemption = RedeemBonus(player.Data.Id, bonusData.Id);
                ProcessBonusRedemptionLifecycle(bonusRedemption);
            }
        }
        internal void ActivateFundInBonus(Guid playerId, Guid bonusId, RedemptionParams redemptionParams)
        {
            var bonusRedemption = RedeemBonus(playerId, bonusId, redemptionParams);
            ProcessBonusRedemptionLifecycle(bonusRedemption);

            _repository.SaveChanges();
        }
        internal Entities.BonusRedemption RedeemBonus(Guid playerId, Guid bonusId, RedemptionParams redemptionParams = null)
        {
            var bonus = _repository.GetLockedBonus(bonusId);
            var player = _repository.GetLockedPlayer(playerId);

            var redemption = player.Redeem(bonus, redemptionParams);

            _repository.SaveChanges();
            return redemption;
        }
        internal void ProcessBonusRedemptionLifecycle(Entities.BonusRedemption redemption)
        {
            if (redemption.Data.Bonus.Template.Wagering.IsAfterWager)
            {
                if (redemption.Data.Bonus.Template.Info.Mode != IssuanceMode.ManualByPlayer || redemption.Data.ActivationState == ActivationStatus.Pending)
                {
                    if (redemption.IsActivationQualified())
                    {
                        IssueWagering(redemption);
                    }
                    else
                    {
                        redemption.Negate();
                    }
                }
            }
            else
            {
                if (redemption.IsActivationQualified())
                {
                    redemption.MakeClaimable();
                    if (redemption.Data.Bonus.Template.Info.Mode != IssuanceMode.ManualByPlayer)
                    {
                        ClaimBonusRedemption(redemption);
                    }
                }
                else
                {
                    redemption.Negate();
                }
            }
        }
        internal void WageringFulfilled(Entities.BonusRedemption redemption)
        {
            if (redemption.Data.Bonus.Template.Wagering.IsAfterWager)
            {
                redemption.MakeClaimable();
                if (redemption.Data.Bonus.Template.Info.Mode != IssuanceMode.ManualByPlayer)
                {
                    ClaimBonusRedemption(redemption);
                }
            }

            IssueUnlock(redemption);
            SetDestinationWalletHasWagering(redemption, false);
            TransferWinningsFromBonusToMain(redemption);
        }

        void ClaimBonusRedemption(Entities.BonusRedemption redemption)
        {
            if (redemption.Data.ActivationState != ActivationStatus.Claimable)
                throw new RegoException(string.Format("Redemption (Id: {0}) is not claimable.", redemption.Data.Id));

            if (redemption.IsClaimQualified())
            {
                var issuanceParams = redemption.Activate();
                _walletCommands.IssueBonus(redemption.Data.Player.Id, issuanceParams);
                if (redemption.Data.Bonus.Template.Wagering.IsAfterWager == false)
                    IssueWagering(redemption);
                SendActivationNotifications(redemption.Data);
            }
            else
            {
                redemption.Negate();
            }
        }
        void SetDestinationWalletHasWagering(Entities.BonusRedemption redemption, bool hasWagering)
        {
            _walletCommands.SetHasWageringRequirement(redemption.Data.Player.Id, redemption.Data.Bonus.Template.Info.WalletTemplateId, hasWagering);
        }
        void TransferWinningsFromBonusToMain(Entities.BonusRedemption bonusRedemption)
        {
            var transactionsDuringRollover = GetTransactionsDuringRollover(bonusRedemption);

            if (transactionsDuringRollover.Any() == false) return;

            var avarageBbBet = transactionsDuringRollover
                .Where(tr => tr.Type == TransactionType.BetPlaced)
                .Average(tr => tr.BonusBalanceAmount);

            var netWinFromBb = transactionsDuringRollover.Where(tr => tr.Type == TransactionType.BetWon).Sum(tr => tr.TotalAmount - avarageBbBet);
            var netLossFromBb = transactionsDuringRollover.Where(tr => tr.Type == TransactionType.BetLost).Sum(tr => tr.BonusBalanceAmount);

            var netWin = netWinFromBb - netLossFromBb;
            var amountToTransfer = bonusRedemption.Data.Bonus.Template.Info.IsWithdrawable
                ? netWin + bonusRedemption.Data.Amount
                : netWin;
            var wallet = bonusRedemption.Wallet;
            var actualBonusBalance = _walletQueries.GetPlayerBalance(bonusRedemption.Data.Player.Id).Bonus;
            if (amountToTransfer > 0)
            {
                var deficit = actualBonusBalance - amountToTransfer - wallet.NotWithdrawableBalance;
                if (deficit < 0)
                {
                    amountToTransfer = amountToTransfer + deficit;
                }

                if (amountToTransfer > 0)
                {
                    actualBonusBalance -= amountToTransfer;
                    _walletCommands.AdjustBalances(bonusRedemption.Data.Player.Id, new AdjustmentParams(AdjustmentReason.WageringFinished)
                    {
                        MainBalanceAdjustment = amountToTransfer,
                        BonusBalanceAdjustment = -amountToTransfer
                    });
                }
            }

            if (actualBonusBalance < wallet.NotWithdrawableBalance)
            {
                wallet.NotWithdrawableBalance = actualBonusBalance;
            }
        }
        void IssueWagering(Entities.BonusRedemption redemption)
        {
            if (redemption.Data.Bonus.Template.Wagering.HasWagering == false)
                return;

            SetDestinationWalletHasWagering(redemption, true);
            redemption.CalculateRolloverAmount();
            var computedLocks = redemption.ActivateRollover();
            foreach (var lockUnlockParam in computedLocks)
            {
                lockUnlockParam.Description = "Wagering requirement lock.";
                _walletCommands.Lock(redemption.Data.Player.Id, lockUnlockParam);
            }
        }
        void IssueUnlock(Entities.BonusRedemption bonusRedemption)
        {
            if (bonusRedemption.Data.LockedAmount > decimal.Zero)
            {
                var computedLocks = bonusRedemption.GetLockUnlockParams();
                var description = bonusRedemption.GetUnlockDescription();
                foreach (var lockUnlockParam in computedLocks)
                {
                    lockUnlockParam.Description = description;
                    _walletCommands.Unlock(bonusRedemption.Data.Player.Id, lockUnlockParam);
                }
            }
        }
        List<BonusTransaction> GetTransactionsDuringRollover(Entities.BonusRedemption bonusRedemption)
        {
            var betIdsDuringRollover = bonusRedemption.Data.Contributions
                .Where(tr => tr.Type == ContributionType.Bet)
                .Select(tr => tr.Transaction.RoundId)
                .ToList();

            var transactionsDuringRollover = bonusRedemption
                .Wallet
                .Transactions
                .Where(tr => tr.RoundId.HasValue)
                .Where(tr => betIdsDuringRollover.Contains(tr.RoundId))
                .ToList();
            return transactionsDuringRollover;
        }
        void SendHighDepositBonusSmsNotifications(Player player)
        {
            var highDepositBonuses = _bonusQueries.GetCurrentVersionBonuses(player.Data.Brand.Id)
                .Where(x => x.Template.Info.BonusTrigger == Trigger.Deposit && x.Template.Info.DepositKind == DepositKind.High)
                .ToList()
                .Select(b => new Entities.Bonus(b));

            foreach (var bonus in highDepositBonuses)
            {
                var bonusRewardThreshold = bonus.CalculateRewardThreshold(player);
                if (bonusRewardThreshold != null)
                {
                    var message = FormHighDepositReminder(player, bonusRewardThreshold);
                    SendSms(player.Data.PhoneNumber, message);
                }
            }
        }
        string FormHighDepositReminder(Player player, BonusRewardThreshold bonusRewardThreshold)
        {
            var notificationMessage = new HighDepositReminderNotificationModel
            {
                Username = player.Data.Name,
                Currency = player.Data.CurrencyCode,
                BonusAmount = bonusRewardThreshold.BonusAmount,
                Brand = player.Data.Brand.Name,
                RemainingAmount = bonusRewardThreshold.RemainingAmount,
                DepositAmountRequired = bonusRewardThreshold.DepositAmountRequired
            };

            return _messageTemplateService.GetMessage(
                    MessageTemplateIdentifiers.HighDepositReminderMessage,
                    notificationMessage);


        }
        void SendActivationNotifications(BonusRedemption bonusRedemption)
        {
            var bonusActivationNotificationModel = new BonusActivationNotificationModel
            {
                Brand = bonusRedemption.Bonus.Template.Info.Brand.Name,
                Amount = bonusRedemption.Amount,
                Currency = bonusRedemption.Player.CurrencyCode,
                Username = bonusRedemption.Player.Name
            };

            if (bonusRedemption.Bonus.Template.Notification.EmailTemplateId.HasValue && !string.IsNullOrEmpty(bonusRedemption.Player.Email))
            {
                var message = _messageTemplateService.GetMessage(
                    bonusRedemption.Bonus.Template.Notification.EmailTemplateId.Value,
                    bonusActivationNotificationModel);
                SendEmail(
                    message,
                    WebConfigurationManager.AppSettings["Smtp.From"],
                    bonusRedemption.Player.Email,
                    "Bonus activated",
                    "Customer Support");
            }

            if (bonusRedemption.Bonus.Template.Notification.SmsTemplateId.HasValue && !string.IsNullOrEmpty(bonusRedemption.Player.PhoneNumber))
            {
                var message = _messageTemplateService.GetMessage(
                    bonusRedemption.Bonus.Template.Notification.SmsTemplateId.Value,
                    bonusActivationNotificationModel);
                SendSms(bonusRedemption.Player.PhoneNumber, message);
            }
        }

        #region Notifications

        internal void SendSmsReferFriendsNotifications(Guid referrerId, List<string> phoneNumbers)
        {
            var referrer = _repository.GetLockedPlayer(referrerId).Data;

            var memberMockUrl = WebConfigurationManager.AppSettings["MemberWebsiteUrl"];
            var referralLink = string.Format("{0}Home/Register?referralId={1}", memberMockUrl, referrer.ReferralId);

            var notificationMessage = new ReferFriendsNotificationsMessage
            {
                ReferalName = referrer.Name,
                ReferalLink = referralLink,
                Brand = referrer.Brand.Name
            };
            var message = _messageTemplateService.GetMessage(MessageTemplateIdentifiers.RefferFriendMessage, notificationMessage);
            phoneNumbers.ForEach(phoneNumber => SendSms(phoneNumber, message));
        }

        private void SendSms(string phoneNumber, string smsBody)
        {
            var smsMsg = new SmsCommandMessage(phoneNumber, smsBody);
            _bus.PublishMessage(smsMsg);
        }

        private void SendEmail(
           string body,
           string from,
           string to,
           string subject,
           string fromTitle,
           string toTitle = "")
        {
            var message = new EmailCommandMessage(
                from,
                fromTitle,
                to,
                toTitle,
                subject,
                body);
            _bus.PublishMessage(message);
        }

        #endregion
    }
}