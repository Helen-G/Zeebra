using AFT.RegoV2.ApplicationServices.Report.EventHandlers;
using AFT.RegoV2.Core.Brand.Events;
using AFT.RegoV2.Domain.Brand.Events;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.WinService.Workers
{
    public class BrandReportWorker : WorkerBase
    {
        private readonly BrandReportEventHandlers _eventHanlers;

        public BrandReportWorker(
            IUnityContainer container,
            BrandReportEventHandlers eventHanlers
            ) : base(container)
        {
            _eventHanlers = eventHanlers;
        }

        protected override void RegisterEventHandlers()
        {
            RegisterEventHandler<BrandRegistered>(_eventHanlers.Handle);
            RegisterEventHandler<BrandUpdated>(_eventHanlers.Handle);
            RegisterEventHandler<BrandActivated>(_eventHanlers.Handle);
            RegisterEventHandler<BrandDeactivated>(_eventHanlers.Handle);
        }
    }
}
