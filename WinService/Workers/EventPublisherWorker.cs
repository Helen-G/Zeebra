using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AFT.RegoV2.BoundedContexts.Event;
using AFT.RegoV2.BoundedContexts.Event.Data;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Event.ApplicationServices;
using AFT.RegoV2.Infrastructure.Synchronization;
using AFT.RegoV2.Shared;
using log4net;
using Microsoft.Practices.Unity;
using Newtonsoft.Json;
using WinService.Workers;

namespace AFT.RegoV2.WinService.Workers
{
    public class EventPublisherWorker : IWorker
    {
        private readonly ISynchronizationService _syncService;
        private readonly IEventRepository _eventRepository;
        private readonly IServiceBus _serviceBus;
        private readonly ILog _logger;
        private readonly Dictionary<string, Type> _eventTypes;
        private Task _publishEventsTask;
        private bool _stopFlag = false;

        public EventPublisherWorker(
            ISynchronizationService synchService,
            IEventRepository eventRepository,
            IServiceBus serviceBus,
            ILog logger)
        {
            _syncService = synchService;
            _eventRepository = eventRepository;
            _serviceBus = serviceBus;
            _logger = logger;
            _eventTypes = new Dictionary<string, Type>();
        }

        public void Start()
        {
            _publishEventsTask = Task.Factory.StartNew(() =>
            {
                while (!_stopFlag)
                {
                    HandleAllButCriticalExceptions(() =>
                    {
                        var events = new Event[] { };

                        //temporarily prevent other instances of this service to publish events
                        _syncService.Execute("EventPublisherWorker", () =>
                        {
                            events = GetNewEvents().Take(50).ToArray();
                            if (events.Length == 0 || _stopFlag)
                                return;

                            foreach (var item in events)
                            {
                                if (_stopFlag) break;

                                var @event = item;
                                HandleAllButCriticalExceptions(@event, () =>
                                {
                                    var message = (dynamic) JsonConvert.DeserializeObject(@event.Data, GetType(@event.DataType));
                                    _serviceBus.PublishMessage(message);
                                    SetEventState(@event, EventState.Published);
                                    _logger.Debug(string.Format("{1}: Event '{0}' published", @event.DataType, this.GetType().Name));
                                });
                            }
                        });

                        //when there are no new events to process - we should wait a bit
                        if (events.Length == 0 && !_stopFlag)
                            Thread.Sleep(TimeSpan.FromSeconds(1));
                    });
                }
            });
        }

        void HandleAllButCriticalExceptions(Action action)
        {
            try
            {
                action();
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                _logger.Error("Error loading events to publish", e);
            }
        }

        void HandleAllButCriticalExceptions(Event @event, Action action)
        {
            try
            {
                action();
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                _logger.Error(string.Format("Error publishing event '{0}'", @event.GetType().Name), e);
            }
        }

        public void Stop()
        {
            _stopFlag = true;
        }

        private IQueryable<Event> GetNewEvents()
        {
            return _eventRepository.Events
                        .OrderBy(e => e.Created)
                        .Where(e =>e.State == EventState.New);
        }

        private void SetEventState(Event @event, EventState state)
        {
            @event.State = state;
            switch (state)
            {
                case EventState.Publishing:
                    @event.ReadyToPublish = DateTimeOffset.UtcNow;
                    break;
                case EventState.Published:
                    @event.Published = DateTimeOffset.UtcNow;
                    break;
            }
            _eventRepository.SaveChanges();
        }

        private Type GetType(string typeName)
        {
            lock (_eventTypes)
            {
                if (_eventTypes.ContainsKey(typeName))
                {
                    return _eventTypes[typeName];
                }
                var types = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(assembly => assembly.FullName.StartsWith("AFT.RegoV2."))
                    .SelectMany(assembly => assembly.GetLoadableTypes())
                    .Where(type => type.IsClass &&
                                   typeof(IDomainEvent).IsAssignableFrom(type) &&
                                   type.Name == typeName)
                    .ToArray();
                if (types.Length == 0)
                {
                    throw new InvalidEventTypeException(String.Format("Event type {0} not found.", typeName));
                }
                if (types.Length > 1)
                {
                    throw new InvalidEventTypeException(String.Format(
                        "Ambigous event type {0}. The follownig matching types found: {1}.", typeName,
                        String.Join("; ", types.Select(type => type.AssemblyQualifiedName))));
                }
                _eventTypes.Add(typeName, types.Single());
                return _eventTypes[typeName];
            }
        }

        private class InvalidEventTypeException : Exception
        {
            public InvalidEventTypeException(string message) : base(message) { }
        }

    }
}
