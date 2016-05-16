using AFT.RegoV2.ApplicationServices.Report.EventHandlers;
using AFT.RegoV2.Core.Brand.Events;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.WinService.Workers
{
    public class LicenseeReportWorker : WorkerBase
    {
        private readonly LicenseeReportEventHandlers _eventHandlers;

        public LicenseeReportWorker(
            IUnityContainer container,
            LicenseeReportEventHandlers eventHandlers
            )
            : base(container)
        {
            _eventHandlers = eventHandlers;
        }

        protected override void RegisterEventHandlers()
        {
            RegisterEventHandler<LicenseeCreated>(_eventHandlers.Handle);
            RegisterEventHandler<LicenseeUpdated>(_eventHandlers.Handle);
            RegisterEventHandler<LicenseeActivated>(_eventHandlers.Handle);
            RegisterEventHandler<LicenseeDeactivated>(_eventHandlers.Handle);
        }
    }
}
