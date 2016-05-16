using System.Linq;
using System.Transactions;
using AFT.RegoV2.Core.Bonus.ApplicationServices;
using AFT.RegoV2.Core.Bonus.Data;
using AFT.RegoV2.Core.Common.Events;
using AFT.RegoV2.Core.Common.Events.Player;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Shared;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.Core.Bonus.EventHandlers
{
    public class PlayerSubscriber
    {
        private readonly IUnityContainer _container;

        public PlayerSubscriber(IUnityContainer container)
        {
            _container = container;
        }

        public void Handle(PlayerContactVerified @event)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var repository = _container.Resolve<IBonusRepository>();

                var player = repository.GetLockedPlayer(@event.PlayerId);
                if (@event.ContactType == ContactType.Mobile) 
                    player.VerifyMobileNumber();
                if (@event.ContactType == ContactType.Email) 
                    player.VerifyEmailAddress();

                if (player.ContactsVerified)
                {
                    var bonusCommands = _container.Resolve<BonusCommands>();
                    bonusCommands.ProcessFirstBonusRedemptionOfTrigger(player, Trigger.MobilePlusEmailVerification);
                }

                repository.SaveChanges();
                scope.Complete();
            }
        }

        public void Handle(PlayersReferred @event)
        {
            _container
                .Resolve<BonusCommands>()
                .SendSmsReferFriendsNotifications(@event.ReferrerId, @event.PhoneNumbers);
        }

        public void Handle(PlayerRegistered @event)
        {
            var bonusRepository = _container.Resolve<IBonusRepository>();
            var bonusCommands = _container.Resolve<BonusCommands>();
            var bonusQueries = _container.Resolve<BonusQueries>();

            using (var scope = CustomTransactionScope.GetTransactionScope(IsolationLevel.RepeatableRead))
            {
                var player = bonusRepository.Players.SingleOrDefault(p => p.Id == @event.PlayerId);
                if (player != null)
                    throw new RegoException(string.Format("Player is already saved. Id: {0}", @event.PlayerId));

                var brand = bonusRepository.Brands.SingleOrDefault(b => b.Id == @event.BrandId);
                if (brand == null)
                    throw new RegoException(string.Format("Unable to find a brand with Id: {0}", @event.BrandId));

                player = new Player(@event, brand);
                if (@event.ReferralId.HasValue)
                {
                    var referrer = bonusRepository.Players.Single(p => p.ReferralId == @event.ReferralId.Value);
                    player.ReferredBy = referrer.Id;
                    var referralBonus =
                        bonusQueries.GetQualifiedBonuses(referrer.Id, Trigger.ReferFriend).FirstOrDefault();
                    if (referralBonus != null)
                    {
                        player.ReferredWith =
                            bonusRepository.Bonuses.Single(
                                b => b.Id == referralBonus.Id && b.Version == referralBonus.Version);
                        bonusCommands.RedeemBonus(referrer.Id, referralBonus.Id);
                    }
                }

                bonusRepository.Players.Add(player);
                bonusRepository.SaveChanges();

                var verificationBonus =
                    bonusQueries.GetQualifiedBonuses(player.Id, Trigger.MobilePlusEmailVerification)
                        .FirstOrDefault();
                if (verificationBonus != null)
                    bonusCommands.RedeemBonus(player.Id, verificationBonus.Id);

                scope.Complete();
            }
        }
    }
}