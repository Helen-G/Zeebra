using AFT.RegoV2.ApplicationServices.Report.EventHandlers;
using AFT.RegoV2.Core.Brand.Events;
using AFT.RegoV2.Core.Common.Brand.Events;
using AFT.RegoV2.Core.Common.Events.Brand;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.WinService.Workers
{
    public class LanguageReportWorker : WorkerBase
    {
        private readonly IUnityContainer _container;
        private readonly LanguageReportEventHandlers _eventHanlers;

        public LanguageReportWorker(
            IUnityContainer container,
            LanguageReportEventHandlers eventHanlers
            ) : base(container)
        {
            _container = container;
            _eventHanlers = eventHanlers;
        }

        protected override void RegisterEventHandlers()
        {
            RegisterEventHandler<LanguageCreated>(_eventHanlers.Handle);
            RegisterEventHandler<LanguageUpdated>(_eventHanlers.Handle);
            RegisterEventHandler<LanguageStatusChanged>(_eventHanlers.Handle);
            RegisterEventHandler<LicenseeCreated>(_eventHanlers.Handle);
            RegisterEventHandler<LicenseeUpdated>(_eventHanlers.Handle);
            RegisterEventHandler<BrandLanguagesAssigned>(_eventHanlers.Handle);
        }
    }
}
