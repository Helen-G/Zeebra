using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AFT.RegoV2.BoundedContexts.Event;
using AFT.RegoV2.Core.Common.Events.Games;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Event
{
    /// <summary>
    /// Stores all domain events in the event store
    /// </summary>
    public class EventStoreSubscriber : IBusSubscriber,
        IConsumes<IDomainEvent>
    {
        private readonly IEventRepository _eventRepository;

        public EventStoreSubscriber(IEventRepository eventRepository)
        {
            _eventRepository = eventRepository;
        }

        public void Consume(IDomainEvent @event)
        {
            _eventRepository.SaveEvent(@event);
        }
    }
}
