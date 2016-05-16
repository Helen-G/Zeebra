using AFT.RegoV2.ApplicationServices.Report.EventHandlers;
using AFT.RegoV2.Core.Security.Events;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.WinService.Workers
{
    public class AuthenticationLogWorker : WorkerBase
    {
        private readonly AuthenticationLogEventHandlers _eventHandlers;

        public AuthenticationLogWorker(
            IUnityContainer container,
            AuthenticationLogEventHandlers eventHandlers
            )
            : base(container)
        {
            _eventHandlers = eventHandlers;
        }

        protected override void RegisterEventHandlers()
        {
            RegisterEventHandler<AdminAuthenticated>(_eventHandlers.Handle);
            RegisterEventHandler<MemberAuthenticationSucceded>(_eventHandlers.Handle);
            RegisterEventHandler<MemberAuthenticationFailed>(_eventHandlers.Handle);
        }
    }
}
