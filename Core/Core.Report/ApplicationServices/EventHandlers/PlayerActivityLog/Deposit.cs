using AFT.RegoV2.Core.Common.Data.Payment;
using AFT.RegoV2.Core.Common.Events.Payment;
using AFT.RegoV2.Domain.Payment.Events;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.Core.Player.ApplicationServices.EventHandlers.ActivityLog
{
    public class DepositActivityLogEventHandlers : PlayerActivityLogEventHandlersBase
    {
        public DepositActivityLogEventHandlers(IUnityContainer container) : base(container)
        {
            Category = "Deposit";
        }

        public void Handle(DepositSubmitted @event)
        {
            AddActivityLog("Create OD Request", @event, @event.PlayerId, @event.Remarks);
        }

        public void Handle(DepositConfirmed @event)
        {
            AddActivityLog("Confirm OD Request", @event, @event.PlayerId, @event.Remarks);
        }

        public void Handle(DepositVerified @event)
        {
            AddActivityLog("Verify OD Request", @event, @event.PlayerId, @event.Remarks);
        }

        public void Handle(DepositApproved @event)
        {
            AddActivityLog("Approve OD Request", @event, @event.PlayerId);

            if (@event.DepositWagering != 0)
            {
                AddActivityLog("Deposit Wagering Requirement", @event, @event.PlayerId);
            }
        }

        public void Handle(DepositUnverified @event)
        {
            if (@event.Status == OfflineDepositStatus.Unverified)
            {
                AddActivityLog("Unverify OD Request", @event, @event.PlayerId, @event.Remarks);
            }
            else if (@event.Status == OfflineDepositStatus.Rejected)
            {
                AddActivityLog("Reject OD Request", @event, @event.PlayerId, @event.Remarks);
            }
        }
    }
}
