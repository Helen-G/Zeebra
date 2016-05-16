using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Shared;

namespace AFT.RegoV2.Infrastructure
{
    public class Bus : IBus
    {
        private readonly Dictionary<string, SubscriberFactories> _routes;

        public Bus()
        {
            _routes = new Dictionary<string, SubscriberFactories>();
        }

        public void Subscribe<TSubscriber>() where TSubscriber : IBusSubscriber, new()
        {
            Subscribe(() => new TSubscriber());
        }

        public void Subscribe<TSubscriber>(Func<TSubscriber> subscriberFactory) where TSubscriber : IBusSubscriber
        {
            var consumableMessageTypes = GetConsumableMessageTypes(typeof(TSubscriber));
            if (!consumableMessageTypes.Any())
                throw new BusException("Subscriber does not consume any reachable type. Have you forgot to implement IConsumes<> interface?");

            foreach (var consumableMessageType in consumableMessageTypes)
            {
                if (consumableMessageType.IsInterface)
                {
                    //get all concrete messages available which are inherited from this interface
                    var inheritedMessages = GetMessageTypesAvailable().Where(x => x.IsClass && consumableMessageType.IsAssignableFrom(x));
                    foreach (var inheritedMessage in inheritedMessages)
                    {
                        AddConsumableMessageType(inheritedMessage, subscriberFactory);
                    }
                }
                else
                    AddConsumableMessageType(consumableMessageType, subscriberFactory);
            }
        }

        void AddConsumableMessageType<TSubscriber>(Type consumableMessageType, Func<TSubscriber> subscriberFactory)
            where TSubscriber : IBusSubscriber
        {
            SubscriberFactories factories;
            if (_routes.ContainsKey(consumableMessageType.FullName))
            {
                factories = _routes[consumableMessageType.FullName];
                factories.Add(() => subscriberFactory());
            }
            else
            {
                factories = new SubscriberFactories(() => subscriberFactory());
                _routes.Add(consumableMessageType.FullName, factories);
            }
        }

        public void Publish(IMessage message)
        {
            var messageType = message.GetType();
            ValidateMessageType(messageType);

            var methodInfo = GetConsumeMethodInfo(messageType);

            //pipeline is simple: we're synchronously notifying each subscriber about published message
            //todo: abstract out this algorithm, so we can use database or rabbitmq for storage and delivery

            var subscribers = GetMessageSubscribers(messageType);
            foreach (var subscriber in subscribers)
            {
                methodInfo.Invoke(subscriber, new object[] { message });
            }
        }


        private void ValidateMessageType(Type messageType)
        {
            if (!_routes.Any()) throw new BusException("No routes were registered. Forgot to call Subscribe?");

            if (_routes.ContainsKey(messageType.FullName)) return; //has at least one explicit consumer

            var interfaces = messageType.GetInterfaces();
            foreach (var item in interfaces)
                if(_routes.ContainsKey(item.FullName)) return; //has at least one consumer of parent interface

            throw new BusException(string.Format("No subscribers has been found for the message of type {0}.",
                                                 messageType.FullName));
        }

        // returns "Consume" method info for specified message type
        private MethodInfo GetConsumeMethodInfo(Type messageType)
        {
            var consumerType = typeof(IConsumes<>).MakeGenericType(messageType);
            return consumerType.GetMethod("Consume");
        }

        private IEnumerable<IBusSubscriber> GetMessageSubscribers(Type messageType)
        {
            var subscribersFactories = _routes[messageType.FullName];
            foreach (var factory in subscribersFactories)
            {
                var subscriber = factory();
                yield return subscriber;
            }
        }

        
        private Type[] GetConsumableMessageTypes(Type subscriberType)
        {
            var messageTypesAvailable = GetMessageTypesAvailable();

            //todo: implement caching of the pair <subscriberType, result array>
            var result = new List<Type>();
            foreach (var messageType in messageTypesAvailable)
            {
                var consumesMessageInterface = typeof(IConsumes<>).MakeGenericType(messageType);
                if (subscriberType.GetInterfaces().Contains(consumesMessageInterface))
                {
                    result.Add(messageType);
                }
            }
            return result.ToArray();
        }

        private static IEnumerable<Type> GetMessageTypesAvailable()
        {
            var messageType = typeof(IMessage);
            return AppDomain.CurrentDomain.GetAssemblies()
                //we can narrow down the scope by specifying namespace for types
                .Where(x => x.FullName.StartsWith("AFT.RegoV2.")) 
                .SelectMany(assembly => assembly.GetLoadableTypes())
                .Where(type => (type.IsClass || type.IsInterface)  && messageType.IsAssignableFrom(type))
                .Memoize();
        }

        class SubscriberFactories : List<Func<IBusSubscriber>>
        {
            public SubscriberFactories(Func<IBusSubscriber> factory) : base(new[] { factory }) { }
        }
    }

    public class BusException : RegoException
    {
        public BusException(string message) : base(message) { }
    }
}