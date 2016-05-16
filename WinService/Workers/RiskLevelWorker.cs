using AFT.RegoV2.Core.Brand.Events;
using AFT.RegoV2.Core.Fraud.ApplicationServices;
using AFT.RegoV2.Domain.Brand.Events;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.WinService.Workers
{
    public class RiskLevelWorker : WorkerBase
    {
        private readonly RiskLevelSubscriber _eventHanlers;

        public RiskLevelWorker(IUnityContainer container, RiskLevelSubscriber handlers)
            : base(container)
        {
            this._eventHanlers = handlers;
        }

        protected override void RegisterEventHandlers()
        {
            RegisterEventHandler<BrandRegistered>(this._eventHanlers.Consume);
            RegisterEventHandler<BrandUpdated>(this._eventHanlers.Consume);
        }
    }
}
