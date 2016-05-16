namespace AFT.RegoV2.Core.Common.Interfaces
{
    public interface IDomainEventHandler<in TEvent>
    {
        void Handle(TEvent @event);
    }
}