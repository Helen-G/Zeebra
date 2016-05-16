using System;
using AFT.RegoV2.Core.Common.Interfaces;
using MassTransit;

namespace AFT.RegoV2.Infrastructure.Consumers
{
    public class Consumer<T> : Consumes<T>.Context where T : class, IMessage, new()
    {
        private readonly Action<T> _messageHandler;

        public Consumer(Action<T> messageHandler)
        {
            _messageHandler = messageHandler;
        }

        public void Consume(IConsumeContext<T> message)
        {
            if (_messageHandler != null)
                _messageHandler(message.Message);
        }
    }
}