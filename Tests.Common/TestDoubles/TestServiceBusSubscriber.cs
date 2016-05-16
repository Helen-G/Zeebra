using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Tests.Common.TestDoubles;

namespace AFT.RegoV2.Tests.Common.Base
{
    /// <summary>
    /// Emulates message publishing of the EventPublisherWorker for testing purposes,
    /// so that all messages from event store are published into ServiceBus as well.
    /// </summary>
    public class TestServiceBusSubscriber : IBusSubscriber,
        IConsumes<IDomainEvent>
    {
        private readonly FakeServiceBus _fakeServiceBus;

        public TestServiceBusSubscriber(FakeServiceBus serviceBus)
        {
            _fakeServiceBus = serviceBus;
        }

        public void Consume(IDomainEvent message)
        {
            _fakeServiceBus.PublishMessage(message, saveToEventStore: false);
        }
    }
}