using System.Linq;
using AFT.RegoV2.Core.Bonus.ApplicationServices;
using AFT.RegoV2.Core.Bonus.Data;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Payment;
using AFT.RegoV2.Core.Common.Events.Payment;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Domain.Payment.Events;
using Microsoft.Practices.Unity;
using BonusRedemption = AFT.RegoV2.Core.Bonus.Entities.BonusRedemption;

namespace AFT.RegoV2.Core.Bonus.EventHandlers
{
    public class PaymentSubscriber
    {
        private readonly IUnityContainer _container;

        public PaymentSubscriber(IUnityContainer container)
        {
            _container = container;
        }

        public void Handle(TransferFundCreated @event)
        {
            if (@event.Type == TransferFundType.FundOut || @event.Status == TransferFundStatus.Rejected)
                return;

            var bonusCommands = _container.Resolve<BonusCommands>();
            var bonusQueries = _container.Resolve<BonusQueries>();
            var redemptionParams = new RedemptionParams { TransferAmount = @event.Amount, TransferWalletTemplateId = @event.DestinationWalletStructureId };

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                bonusQueries
                    .GetQualifiedBonusIds(@event.PlayerId, @event.BonusCode, Trigger.FundIn, redemptionParams)
                    .ForEach(bonusId => bonusCommands.ActivateFundInBonus(@event.PlayerId, bonusId, redemptionParams));

                scope.Complete();
            }
        }

        public void Handle(DepositSubmitted @event)
        {
            var bonusCommands = _container.Resolve<BonusCommands>();
            var bonusQueries = _container.Resolve<BonusQueries>();
            var redemptionParams = new RedemptionParams { TransferAmount = @event.Amount, TransferExternalId = @event.DepositId };

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                bonusQueries
                    .GetQualifiedBonusIds(@event.PlayerId, @event.BonusCode, Trigger.Deposit, redemptionParams)
                    .ForEach(bonusId => bonusCommands.RedeemBonus(@event.PlayerId, bonusId, redemptionParams));

                scope.Complete();
            }
        }

        public void Handle(DepositUnverified @event)
        {
            var repository = _container.Resolve<IBonusRepository>();
            var player = repository.GetLockedPlayer(@event.PlayerId);

            var bonusRedemptions = player.BonusesRedeemed
                .Where(r => r.Parameters.TransferExternalId == @event.DepositId)
                .Select(brd => new BonusRedemption(brd));
            foreach (var redemption in bonusRedemptions)
            {
                redemption.Negate();
            }

            repository.SaveChanges();
        }

        public void Handle(DepositApproved @event)
        {
            var bonusCommands = _container.Resolve<BonusCommands>();
            var repository = _container.Resolve<IBonusRepository>();
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var player = repository.GetLockedPlayer(@event.PlayerId);
                player.Data.DepositTransactionsCount++;

                var depositBonusRedemptions = player.BonusesRedeemed
                    .Where(r => r.Parameters.TransferExternalId == @event.DepositId)
                    .Select(br => new BonusRedemption(br));
                foreach (var depositBonusRedemption in depositBonusRedemptions)
                {
                    depositBonusRedemption.Data.Parameters.TransferAmount = @event.ActualAmount;
                    bonusCommands.ProcessBonusRedemptionLifecycle(depositBonusRedemption);
                }

                repository.SaveChanges();
                scope.Complete();
            }
        }
    }
}