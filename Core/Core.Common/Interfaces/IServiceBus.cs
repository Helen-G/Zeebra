using System;

namespace AFT.RegoV2.Core.Common.Interfaces
{
    public interface IServiceBus : IDisposable
    {
        /// <summary>
        /// Asyncronous inter-process events and commands publishing.
        /// It asynchronously triggers either event handlers, which are registered using RegisterEventHandler method of EventWorkerBase class,
        /// or command handlers, which are ProcessMessage method in classes, derived from generic BaseNotificationWorker class of appropriate command type T.
        /// </summary>
        /// <typeparam name="T">Either event type (derived from DomainEventBase), or command type (implemented ICommand interface).</typeparam>
        /// <param name="message">Either event or command to be published via Message Queue.</param>
        void PublishMessage<T>(T message) where T : class, IMessage;

        /// <summary>
        /// Is used for register message handler in Message Queue.
        /// Do not use this method directly if you want to create notification handler or event handler. Instead, create derived class
        /// either from BaseNotificationWorker, and override ProcessMessage method,
        /// or from EventWorkerBase, and use RegisterEventHandler inside overrided RegisterEventHandlers method.
        /// </summary>
        /// <typeparam name="T">Either event type (derived from DomainEventBase), or command type (implemented ICommand interface).</typeparam>
        /// <param name="messageHandler">Either event handler or command handler to be subscribed for triggering from Message Queue.</param>
        /// <param name="queueName">Name of durable queue in MQ. Set null in order to create and use temporary queue.</param>
        void Subscribe<T>(Action<T> messageHandler, string queueName) where T : class, IMessage, new();
    }
}