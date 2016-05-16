using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using AFT.RegoV2.BoundedContexts.Event;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Shared;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.Tests.Common.TestDoubles
{
    /// <summary>
    /// Dispatches Domain Events from in-process synchronous serviceBus.
    /// </summary>
    public class FakeServiceBus : IServiceBus
    {
        private readonly IUnityContainer _unityContainer;
        private readonly IEventRepository _eventRepository;

        private readonly Dictionary<Type, List<Action<IMessage>>> _subscriptions;
        private readonly List<IMessage> _undispatchedMessages;

        public int PublishedCommandCount { get; private set; }
        public int PublishedEventCount { get; private set; }

        public FakeServiceBus(
            IUnityContainer container,
            IEventRepository eventRepository
            )
        {
            _unityContainer = container;
            _eventRepository = eventRepository;
            _subscriptions = new Dictionary<Type, List<Action<IMessage>>>();
            _undispatchedMessages = new List<IMessage>();
            MessageOrder = new Queue<Type>();
        }

        public void PublishLocal<T>(T @event) where T : class, IDomainEvent
        {
            var genericOpenType = typeof(IDomainEventHandler<>);
            var constructedType = genericOpenType.MakeGenericType(@event.GetType());
            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies().Where(x => x.FullName.StartsWith("AFT.RegoV2."));

            Type[] types;
            try
            {
                //get interface implementations from calling assembly
                types = loadedAssemblies
                    .SelectMany(x => x.GetLoadableTypes())
                    .Where(p => constructedType.IsAssignableFrom(p) && p.IsClass)
                    .ToArray();
            }
            catch (ReflectionTypeLoadException ex)
            {
                var sb = new StringBuilder();
                foreach (Exception exSub in ex.LoaderExceptions)
                {
                    sb.AppendLine(exSub.Message);
                    var exFileNotFound = exSub as FileNotFoundException;
                    if (exFileNotFound != null)
                    {
                        if (!string.IsNullOrEmpty(exFileNotFound.FusionLog))
                        {
                            sb.AppendLine("Fusion Log:");
                            sb.AppendLine(exFileNotFound.FusionLog);
                        }
                    }
                    sb.AppendLine();
                }
                var errorMessage = sb.ToString();
                throw new RegoException(errorMessage);
            }

            types.ForEach(x =>
            {
                //todo: optimize, when performance becomes an issue
                var instance = (dynamic)_unityContainer.Resolve(x);
                instance.Handle((dynamic)@event);
            });

            PublishMessage(@event);
        }

        /// <summary>
        /// For testing purposes, it allows to delay incoming messages from MessageOrder tail until head message arrive.
        /// For example, let MessageOrder is Queue { TypeA, TypeB, TypeC }, and messages of TypeB and TypeC
        /// are incoming before a message of TypeA. In that case FakeServiceBus waits for a message of TypeA,
        /// then fires the message of TypeA, then the message of TypeB, then the message of TypeC.
        /// If MessageOrder is not set, or incoming message type is not listed in MessageOrder, then the message is fired immediately.
        /// </summary>
        public Queue<Type> MessageOrder { get; set; }

        public void PublishMessage<T>(T message) where T : class, IMessage
        {
            PublishMessage(message, saveToEventStore: true);
        }

        public void PublishMessage<T>(T message, bool saveToEventStore) where T : class, IMessage
        {
            if (message is ICommand)
            {
                PublishedCommandCount++;
            }

            IDomainEvent domainEvent;
            if ((domainEvent = message as IDomainEvent) != null)
            {
                if (saveToEventStore)
                    _eventRepository.SaveEvent(domainEvent);
                PublishedEventCount++;
            }

            _undispatchedMessages.Add(message);
            DispatchMessages();
        }

  

        private bool _dispatching;
        private bool _recurrentDispatching;
        public void DispatchMessages()
        {
            if (_dispatching)
            {
                _recurrentDispatching = true;
                return;
            }
            _dispatching = true;
            int messageCount;
            do
            {
                _recurrentDispatching = false;
                messageCount = _undispatchedMessages.Count;
                DispatchNextMessage();
            } while (_recurrentDispatching || messageCount != _undispatchedMessages.Count);
            _dispatching = false;
        }

        private void DispatchNextMessage()
        {
            if (!_undispatchedMessages.Any())
            {
                return;
            }
            var message = _undispatchedMessages.First();
            if (MessageOrder.Any() && _undispatchedMessages.Any(m => m.GetType() == MessageOrder.Peek()))
            {
                message = _undispatchedMessages.First(m => m.GetType() == MessageOrder.Dequeue());
            }
            else if (MessageOrder.Contains(message.GetType()))
            {
                return;
            }
            _undispatchedMessages.Remove(message);
            DispatchMessage(message);
        }

        private void DispatchMessage(IMessage message)
        {
            if (_subscriptions.ContainsKey(message.GetType()))
            {
                _subscriptions[message.GetType()].ForEach(messageHandler => messageHandler(message));
            }
        }

        public void Subscribe<T>(Action<T> messageHandler, string queueName) where T : class, IMessage, new()
        {
            Action<IMessage> subscription = (message) => messageHandler((T) message);
            if (!_subscriptions.ContainsKey(typeof(T)))
            {
                _subscriptions.Add(typeof(T), new List<Action<IMessage>>());
            }
            _subscriptions[typeof (T)].Add(subscription);
        }

        public void Dispose()
        {
        }
    }
}