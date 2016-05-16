using AFT.RegoV2.ApplicationServices.Report.EventHandlers;
using AFT.RegoV2.Core.Common.Events.Payment;
using AFT.RegoV2.Domain.Payment.Events;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.WinService.Workers
{
    public class DepositReportWorker : WorkerBase
    {
        private readonly DepositReportEventHandlers _eventHandlers;

        public DepositReportWorker(
            IUnityContainer container,
            DepositReportEventHandlers eventHandlers
            ) : base(container)
        {
            _eventHandlers = eventHandlers;
        }

        protected override void RegisterEventHandlers()
        {
            RegisterEventHandler<DepositSubmitted>(_eventHandlers.Handle);
            RegisterEventHandler<DepositConfirmed>(_eventHandlers.Handle);
            RegisterEventHandler<DepositVerified>(_eventHandlers.Handle);
            RegisterEventHandler<DepositApproved>(_eventHandlers.Handle);
            RegisterEventHandler<DepositUnverified>(_eventHandlers.Handle);
        }
    }
}
