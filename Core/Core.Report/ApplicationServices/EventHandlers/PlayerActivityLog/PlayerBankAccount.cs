using AFT.RegoV2.Core.Common.Events;
using AFT.RegoV2.Core.Player.ApplicationServices;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.Core.Report.ApplicationServices.EventHandlers.PlayerActivityLog
{
    public class PlayerBankAccountLogEventHandlers : PlayerActivityLogEventHandlersBase
    {
        public PlayerBankAccountLogEventHandlers(IUnityContainer container)
            : base(container)
        {
            Category = "Player Bank Account";
        }

        public void Handle(PlayerBankAccountVerified @event)
        {
            AddActivityLog("Player bank account verified", @event, @event.PlayerId, @event.Remarks);
        }

        public void Handle(PlayerBankAccountRejected @event)
        {
            AddActivityLog("Player bank account rejected", @event, @event.PlayerId, @event.Remarks);
        }
    }
}
