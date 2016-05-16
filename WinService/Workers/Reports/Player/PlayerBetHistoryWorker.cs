using AFT.RegoV2.ApplicationServices.Report.EventHandlers;
using AFT.RegoV2.Core.Common.Events.Games;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.WinService.Workers
{
    public class PlayerBetHistoryReportWorker : WorkerBase
    {
        private readonly PlayerBetHistoryReportEventHandlers _eventHandlers;

        public PlayerBetHistoryReportWorker(
            IUnityContainer container,
            PlayerBetHistoryReportEventHandlers eventHandlers
            ) : base(container)
        {
            _eventHandlers = eventHandlers;
        }

        protected override void RegisterEventHandlers()
        {
            RegisterEventHandler<BetPlaced>(_eventHandlers.Handle);
            RegisterEventHandler<BetWon>(_eventHandlers.Handle);
            RegisterEventHandler<BetAdjusted>(_eventHandlers.Handle);
            RegisterEventHandler<BetCancelled>(_eventHandlers.Handle);
            
            //todo: removed until Maxim investigation. 
            //todo: Was causing exceptions due to inconsistency of implementation with business logic.
            //RegisterEventHandler<BetPlacedFree>(_eventHandlers.Handle);
        }
    }
}
