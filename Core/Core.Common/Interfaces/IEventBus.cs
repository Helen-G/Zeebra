using System;

namespace AFT.RegoV2.Core.Common.Interfaces
{
    /// <summary>
    /// Mechanism for synchronous in-process message publishing
    /// </summary>
    public interface IBus
    {
        /// <summary>
        /// Publishes message synchronously, so that all inheritors of IBusSubscriber interface are notified
        /// </summary>
        /// <param name="message">The message to be published synchronously.</param>
        void Publish(IMessage message);
        void Subscribe<TSubscriber>() where TSubscriber : IBusSubscriber, new();
        void Subscribe<TSubscriber>(Func<TSubscriber> subscriberFactory) where TSubscriber : IBusSubscriber;
    }
    
    /// <summary>
    /// Synchronous in-process events publishing mechanism
    /// </summary>
    public interface IEventBus
    {
        /// <summary>
        /// Publishes event synchronously, so that all inheritors of IBusSubscriber interface are notified
        /// </summary>
        /// <param name="event">The event to be published synchronously. Typically happens within same transaction scope, if any</param>
        void Publish(IDomainEvent @event);
        void Subscribe<TSubscriber>() where TSubscriber : IBusSubscriber, new();
        void Subscribe<TSubscriber>(Func<TSubscriber> subscriberFactory) where TSubscriber : IBusSubscriber;
    }

    /// <summary>
    /// Marker interface to identify subscribers
    /// </summary>
    public interface IBusSubscriber { }

    /// <summary>
    /// Defines which message type subscriber(s) can handle. 
    /// May be inherited multiple times with different TMessage specified.
    /// TMessage may be of an abstract type, like interface
    /// </summary>
    /// <typeparam name="TMessage">Message type to be handled. When abstract type specified - all concrete inheritors will be used</typeparam>
    public interface IConsumes<in TMessage> : IBusSubscriber where TMessage : IMessage
    {
        void Consume(TMessage message);
    }
}