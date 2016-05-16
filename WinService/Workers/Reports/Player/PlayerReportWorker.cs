using AFT.RegoV2.ApplicationServices.Report.EventHandlers;
using AFT.RegoV2.Core.Common.Events;
using AFT.RegoV2.Core.Domain.Player.Events;
using AFT.RegoV2.Core.Player.Events;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.WinService.Workers
{
    public class PlayerReportWorker : WorkerBase
    {
        private readonly PlayerReportEventHandlers _eventHandlers;

        public PlayerReportWorker(
            IUnityContainer container,
            PlayerReportEventHandlers eventHandlers
            )
            : base(container)
        {
            _eventHandlers = eventHandlers;
        }

        protected override void RegisterEventHandlers()
        {
            RegisterEventHandler<PlayerRegistered>(_eventHandlers.Handle);
            RegisterEventHandler<PlayerUpdated>(_eventHandlers.Handle);
            RegisterEventHandler<PlayerStatusChanged>(_eventHandlers.Handle);
        }
    }
}
