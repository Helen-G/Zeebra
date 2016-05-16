using AFT.RegoV2.Core.Common.Events;
using AFT.RegoV2.Core.Common.Events.Payment;
using AFT.RegoV2.Core.Common.Events.Player;
using AFT.RegoV2.Core.Payment.Events;
using AFT.RegoV2.Core.Player.Data;
using AFT.RegoV2.Core.Player.Events;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.Core.Player.ApplicationServices.EventHandlers.ActivityLog
{
    public class PlayerActivityLogEventHandlers : PlayerActivityLogEventHandlersBase
    {
        public PlayerActivityLogEventHandlers(IUnityContainer container)
            : base(container)
        {
            Category = "Player";
        }

        public void Handle(PlayerStatusChanged @event)
        {
            if (@event.AccountStatus == AccountStatus.Active)
            {
                AddActivityLog("Activation performed", @event, @event.PlayerId);
            }
            else if (@event.AccountStatus == AccountStatus.Inactive)
            {
                AddActivityLog("Account deactivated", @event, @event.PlayerId);
            }
        }

        public void Handle(PlayerRegistered @event)
        {
            if (@event.Status == AccountStatus.Active.ToString())
            {
                AddActivityLog("Automatic activation performed", @event, @event.PlayerId);
            }
        }

        public void Handle(ActivationLinkSent @event)
        {
            if (@event.ContactType == ContactType.Email)
            {
                AddActivityLog("Activation link sent by email", @event, @event.PlayerId, GetPlayerName(@event.PlayerId),
                    string.Empty);
            }
            else if (@event.ContactType == ContactType.Mobile)
            {
                AddActivityLog("Activation link sent by sms", @event, @event.PlayerId, GetPlayerName(@event.PlayerId), string.Empty);
            }
        }

        public void Handle(WithdrawalCreated @event)
        {
            AddActivityLog(string.Format("Withdrawal created. Amount: {0}", @event.Amount), @event, @event.PlayerId, GetPlayerName(@event.PlayerId));
        }

        public void Handle(PlayerContactVerified @event)
        {
            if (@event.ContactType == ContactType.Email)
            {
                AddActivityLog("Activation performed by email", @event, @event.PlayerId, GetPlayerName(@event.PlayerId), string.Empty);
            }
            else if (@event.ContactType == ContactType.Mobile)
            {
                AddActivityLog("Activation performed by sms", @event, @event.PlayerId, GetPlayerName(@event.PlayerId), string.Empty);
            }
        }

        public void Handle(TransferFundCreated @event)
        {
            AddActivityLog(string.Format("Transfer Fund created. Amount: {0}. Transaction Number: {1}", @event.Amount, @event.TransactionNumber), @event, @event.PlayerId,
                GetPlayerName(@event.PlayerId), @event.Remarks);
        }
    }
}
