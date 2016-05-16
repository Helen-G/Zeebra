using System;
using System.Collections.Generic;
using System.Data.Entity;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.BoundedContexts.Event
{
    public interface IEventRepository
    {
        IDbSet<Data.Event> Events { get; }
        IDbSet<Data.Acknowledgement> Acknowledgements { get; }
        IDbSet<Data.Worker> Workers { get; }

        IEnumerable<T> GetEvents<T>();
        void LockEvent(Guid eventId);
        void LockWorker(string workerTypeName);
        void Reload<T>(T entity) where T : class;
        void SaveEvent(IDomainEvent @event);

        int SaveChanges();

    }
}