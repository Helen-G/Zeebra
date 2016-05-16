using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using AFT.RegoV2.BoundedContexts.Event;
using AFT.RegoV2.BoundedContexts.Event.Data;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Infrastructure.DataAccess.Event;
using Newtonsoft.Json;

namespace AFT.RegoV2.Tests.Common.TestDoubles
{

    public class FakeEventRepository : IEventRepository
    {
        private readonly FakeDbSet<Event> _events = new FakeDbSet<Event>();
        private readonly FakeDbSet<Acknowledgement> _acknowledgements = new FakeDbSet<Acknowledgement>();
        private readonly FakeDbSet<Worker> _workers = new FakeDbSet<Worker>();

     
        public IDbSet<Event> Events { get { return _events; } }
        public IDbSet<Acknowledgement> Acknowledgements { get { return _acknowledgements; } }
        public IDbSet<Worker> Workers { get { return _workers; } }

        public IEnumerable<T> GetEvents<T>()
        {
            return Events
                .Where(x => x.DataType == typeof(T).Name)
                .Select(x => JsonConvert.DeserializeObject<T>(x.Data));
        }

        public void LockEvent(Guid eventId)
        {
        }

        public void LockWorker(string workerTypeName)
        {
        }

        public void Reload<T>(T entity) where T : class
        {
        }

        public void SaveEvent(IDomainEvent @event)
        {
            Events.Add(EventRepository.CreateEventData(@event));
        }

        public int SaveChanges()
        {
            return 0;
        }
    }
}
