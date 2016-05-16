using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AFT.RegoV2.ApplicationServices.Report.EventHandlers;
using AFT.RegoV2.Core.Common.Events;
using AFT.RegoV2.Core.Common.Events.Wallet;
using AFT.RegoV2.Core.Domain.Player.Events;
using AFT.RegoV2.Core.Game.Events;
using AFT.RegoV2.Core.Player.Events;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.WinService.Workers
{
    public class PlayerTransactionReportWorker : WorkerBase
    {
        private readonly PlayerTransactionReportEventHandlers _eventHandlers;

        public PlayerTransactionReportWorker(
            IUnityContainer container,
            PlayerTransactionReportEventHandlers eventHandlers
            )
            : base(container)
        {
            _eventHandlers = eventHandlers;
        }

        protected override void RegisterEventHandlers()
        {
            RegisterEventHandler<TransactionProcessed>(_eventHandlers.Handle);
            RegisterEventHandler<LockApplied>(_eventHandlers.Handle);
        }
    }
}
