using AFT.RegoV2.Core.Payment.Events;
using AFT.RegoV2.Domain.BoundedContexts.Payment.ApplicationServices.Events;
using AFT.RegoV2.Domain.Payment.Data;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.Core.Player.ApplicationServices.EventHandlers.ActivityLog
{
    public class WithdrawActivityLogEventHandlers : PlayerActivityLogEventHandlersBase
    {
        public WithdrawActivityLogEventHandlers(IUnityContainer container) : base(container)
        {
            Category = "Withdraw";
        }

        public void Handle(WithdrawalCreated @event)
        {
            AddActivityLog("Create OW Request", @event, @event.PlayerId);
        }

        public void Handle(WithdrawalVerified @event)
        {
            AddActivityLog("Verify OW Request", @event, @event.PlayerId, @event.Remarks);
        }

        public void Handle(WithdrawalAccepted @event)
        {
            AddActivityLog("Accept OW Request", @event, @event.PlayerId, @event.Remarks);
        }

        public void Handle(WithdrawalWagerChecked @event)
        {
            AddActivityLog("Wager Check OW Request", @event, @event.PlayerId, @event.Remarks);
        }

        public void Handle(WithdrawalInvestigated @event)
        {
            AddActivityLog("Verify OW Request", @event, @event.PlayerId, @event.Remarks);
        }

        public void Handle(WithdrawalApproved @event)
        {
            AddActivityLog("Verify OW Request", @event, @event.PlayerId, @event.Remarks);
        }

        public void Handle(WithdrawalCancelled @event)
        {
            if (@event.Status == WithdrawalStatus.Unverified)
            {
                AddActivityLog("Unverify OW Request", @event, @event.PlayerId, @event.Remarks);
            }
            else if (@event.Status == WithdrawalStatus.Rejected)
            {
                AddActivityLog("Reject OW Request", @event, @event.PlayerId, @event.Remarks);
            }
            else if (@event.Status == WithdrawalStatus.Reverted)
            {
                AddActivityLog("Revert OW Request", @event, @event.PlayerId, @event.Remarks);
            }
        }
    }
}
