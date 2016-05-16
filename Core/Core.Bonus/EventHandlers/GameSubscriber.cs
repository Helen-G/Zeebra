using System;
using System.Linq;
using AFT.RegoV2.Core.Bonus.ApplicationServices;
using AFT.RegoV2.Core.Bonus.Data;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Wallet;
using AFT.RegoV2.Core.Common.Events.Game;
using AFT.RegoV2.Core.Common.Events.Wallet;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Shared;
using AutoMapper;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.Core.Bonus.EventHandlers
{
    public class GameSubscriber
    {
        private readonly IUnityContainer _container;
        private const string NoGameFormatter = "Game does not exist: {0}";

        static GameSubscriber()
        {
            Mapper.CreateMap<TransactionProcessed, BonusTransaction>()
                .ForMember(src => src.Id, opt => opt.MapFrom(tr => tr.TransactionId))
                .ForMember(src => src.Type, opt => opt.MapFrom(tr => tr.PaymentType))
                .ForMember(src => src.MainBalanceAmount, opt => opt.MapFrom(tp => Math.Abs(tp.MainBalanceAmount)))
                .ForMember(src => src.BonusBalanceAmount, opt => opt.MapFrom(tp => Math.Abs(tp.BonusBalanceAmount)))
                .ForMember(src => src.TotalAmount, opt => opt.MapFrom(tp => Math.Abs(tp.MainBalanceAmount + tp.BonusBalanceAmount + tp.TemporaryBalanceAmount)));
        }

        public GameSubscriber(IUnityContainer container)
        {
            _container = container;
        }

        public void Handle(GameCreated @event)
        {
            var repository = _container.Resolve<IBonusRepository>();

            repository.Games.Add(new Game
            {
                Id = @event.Id,
                ProductId = @event.GameProviderId
            });
            repository.SaveChanges();
        }

        public void Handle(GameUpdated @event)
        {
            var repository = _container.Resolve<IBonusRepository>();
            var game = repository.Games.SingleOrDefault(g => g.Id == @event.Id);
            if (game == null)
                throw new RegoException(string.Format(NoGameFormatter, @event.Id));

            game.ProductId = @event.GameProviderId;
            repository.SaveChanges();
        }

        public void Handle(GameDeleted @event)
        {
            var repository = _container.Resolve<IBonusRepository>();
            var game = repository.Games.SingleOrDefault(g => g.Id == @event.Id);
            if (game == null)
                throw new RegoException(string.Format(NoGameFormatter, @event.Id));

            repository.RemoveGameContributionsForGame(@event.Id);
            repository.Games.Remove(game);
            repository.SaveChanges();
        }

        public void Handle(TransactionProcessed @event)
        {
            var repository = _container.Resolve<IBonusRepository>();
            var player = repository.GetLockedPlayer(@event.Wallet.PlayerId);
            var targetWallet = player.Data.Wallets.Single(w => w.TemplateId == @event.Wallet.WalletTemplateId);
            var transaction = Mapper.Map<BonusTransaction>(@event);

            if (@event.PaymentType == TransactionType.BetPlaced ||
                @event.PaymentType == TransactionType.BetWon ||
                @event.PaymentType == TransactionType.BetLost)
            {
                targetWallet.Transactions.Add(transaction);

                if (@event.PaymentType == TransactionType.BetWon || @event.PaymentType == TransactionType.BetLost)
                {
                    var playableBalance = @event.Wallet.Playable;
                    var redemptionsWithActiveRollover = player.GetRedemptionsWithActiveRollover(@event.Wallet.WalletTemplateId);
                    if (@event.PaymentType == TransactionType.BetWon && @event.TemporaryBalanceAmount > 0)
                    {
                        var walletCommands = _container.Resolve<IWalletCommands>();
                        var adjustment = new BetWonAdjustmentParams
                        {
                            WalletTemplateId = @event.Wallet.WalletTemplateId,
                            RelatedTransactionId = transaction.Id,
                            BetWonDuringRollover = redemptionsWithActiveRollover.Any()
                        };

                        walletCommands.AdjustBetWonTransaction(player.Data.Id, adjustment);
                        playableBalance += @event.TemporaryBalanceAmount;
                    }

                    var bonusCommands = _container.Resolve<BonusCommands>();
                    var wageringToDistribute = player.GetWageringToDistribute(@event.Wallet.WalletTemplateId, transaction.RoundId.Value);
                    var wageringLeftToDistribute = wageringToDistribute;
                    foreach (var redemption in redemptionsWithActiveRollover)
                    {
                        var contributionMultipler = redemption.GetGameToWageringContributionMultiplier(@event.GameId.Value);
                        if (contributionMultipler > 0m)
                        {
                            var contributionAmount = wageringLeftToDistribute * contributionMultipler;
                            var handledAmount = Math.Min(contributionAmount, redemption.RolloverLeft);
                            redemption.Data.Contributions.Add(new RolloverContribution
                            {
                                Transaction = transaction,
                                Contribution = handledAmount,
                                Type = ContributionType.Bet
                            });
                            wageringLeftToDistribute -= handledAmount;

                            if (redemption.RolloverLeft == 0m)
                            {
                                redemption.CompleteRollover();
                                bonusCommands.WageringFulfilled(redemption);
                            }
                        }

                        if (wageringLeftToDistribute == 0m)
                        {
                            if (redemption.WageringThresholdIsMet(playableBalance) &&
                                // Check if rollover is still Active, 'cos it can become Completed several lines before
                                redemption.Data.RolloverState == RolloverStatus.Active)
                            {
                                redemption.ZeroOutRollover(transaction);
                                bonusCommands.WageringFulfilled(redemption);
                            }
                            break;
                        }
                    }

                    player.Data.AccumulatedWageringAmount += wageringToDistribute;
                    if (player.Data.ReferredBy.HasValue && player.CompletedReferralRequirements(wageringToDistribute))
                    {
                        var referrer = repository.GetLockedPlayer(player.Data.ReferredBy.Value);
                        bonusCommands.ProcessFirstBonusRedemptionOfTrigger(referrer, Trigger.ReferFriend);
                    }
                }
            }
            else if (@event.PaymentType == TransactionType.Deposit)
            {
                targetWallet.Transactions.Add(transaction);
                _container.Resolve<BonusCommands>().ProcessHighDepositBonus(player);
            }
            else if (@event.PaymentType == TransactionType.FundIn && @event.MainBalanceAmount > 0)
            {
                targetWallet.Transactions.Add(transaction);
            }

            repository.SaveChanges();
        }
    }
}