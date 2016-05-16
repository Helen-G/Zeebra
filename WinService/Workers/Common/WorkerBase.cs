using System;
using System.Collections.Generic;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Domain.BoundedContexts.Security.ApplicationServices;
using AFT.RegoV2.Domain.Security.Interfaces;
using AFT.RegoV2.Shared;
using log4net;
using Microsoft.Practices.Unity;
using WinService.Workers;

namespace AFT.RegoV2.WinService.Workers
{
    public abstract class WorkerBase : IWorker
    {
        private readonly IServiceBus _serviceBus;
        private readonly IUnityContainer _container;
        private readonly ILog _logger;
        private readonly Dictionary<Type, Action<IDomainEvent>> _eventHandlers;

        protected WorkerBase(IUnityContainer container)
        {
            _container = container;
            _serviceBus = _container.Resolve<IServiceBus>();
            _logger = _container.Resolve<ILog>();
            _eventHandlers = new Dictionary<Type, Action<IDomainEvent>>();
        }

        public void Start()
        {
            RegisterEventHandlers();
        }

        public void Stop() { }

        protected abstract void RegisterEventHandlers();

        protected void RegisterEventHandler<TEvent>(Action<TEvent> eventHandler)
            where TEvent : class, IDomainEvent, new()
        {
            SaveHandler(eventHandler);

            _serviceBus.Subscribe(new Action<TEvent>(@event =>
            {
                _logger.Debug(string.Format("{1}: processing '{0}' message ...", @event.GetType().Name, this.GetType().Name));
               
                eventHandler(@event);
                _logger.Debug(string.Format("{1}: processing '{0}' message finished.", @event.GetType().Name, this.GetType().Name));
            }), this.GetType().Name);
        }

        private void SaveHandler<TEvent>(Action<TEvent> eventHandler) where TEvent : class, IDomainEvent, new()
        {
            if (_eventHandlers.ContainsKey(typeof (TEvent)))
            {
                var message = string.Format("{0} is already subscribed to {1} events.", GetType().Name,
                    typeof (TEvent).Name);
                _logger.Error(message);
                throw new RegoException(message);
            }
            _eventHandlers.Add(typeof (TEvent), e => eventHandler(e as TEvent));
        }
    }
}