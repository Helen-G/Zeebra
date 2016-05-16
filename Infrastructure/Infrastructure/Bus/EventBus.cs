using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Infrastructure
{
    public class EventBus : Bus, IEventBus
    {
        public void Publish(IDomainEvent @event)
        {
            base.Publish(@event);
        }
    }
}