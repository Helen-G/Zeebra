using AFT.RegoV2.BoundedContexts.Event;
using AFT.RegoV2.Core.Common.Events.Notifications;
using AFT.RegoV2.Core.Common.Interfaces;

namespace WinService.Workers.Common
{
    public abstract class NotificationWorkerBase<T> : IWorker where T : class, IMessage, new()
    {
        private readonly IEventRepository _eventRepository;
        protected readonly IServiceBus _serviceBus;

        protected NotificationWorkerBase(IEventRepository eventRepository, IServiceBus serviceBus)
        {
            _eventRepository = eventRepository;
            _serviceBus = serviceBus;
        }

        public void Start()
        {
            InitServiceBusListener();
        }

        public void Stop()
        {
        }

        protected void FinalizeMessage(NotificationSentEvent @event)
        {
            _eventRepository.SaveEvent(@event);
        }

        protected void InitServiceBusListener()
        {
            _serviceBus.Subscribe<T>(ProcessMessage, GetType().Name);
        }

        public abstract void ProcessMessage(T message);
    }
}