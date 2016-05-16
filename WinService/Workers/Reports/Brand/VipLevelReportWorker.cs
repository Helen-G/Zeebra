using AFT.RegoV2.ApplicationServices.Report.EventHandlers;
using AFT.RegoV2.Core.Brand.Events;
using AFT.RegoV2.Core.Common.Events.Brand;
using AFT.RegoV2.Domain.Brand.Events;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.WinService.Workers
{
    public class VipLevelReportWorker : WorkerBase
    {
        private readonly VipLevelReportEventHandlers _eventHandlers;

        public VipLevelReportWorker(
            IUnityContainer container,
            VipLevelReportEventHandlers eventHandlers
            )
            : base(container)
        {
            _eventHandlers = eventHandlers;
        }

        protected override void RegisterEventHandlers()
        {
            RegisterEventHandler<VipLevelRegistered>(_eventHandlers.Handle);
            RegisterEventHandler<VipLevelUpdated>(_eventHandlers.Handle);
            RegisterEventHandler<VipLevelActivated>(_eventHandlers.Handle);
            RegisterEventHandler<VipLevelDeactivated>(_eventHandlers.Handle);
        }
    }
}
