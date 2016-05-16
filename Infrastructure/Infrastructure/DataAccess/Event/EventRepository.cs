using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using AFT.RegoV2.BoundedContexts.Event;
using AFT.RegoV2.BoundedContexts.Event.Data;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Event.ApplicationServices;
using Newtonsoft.Json;
using EventData = AFT.RegoV2.BoundedContexts.Event.Data.Event;

namespace AFT.RegoV2.Infrastructure.DataAccess.Event
{
    public class EventRepository : DbContext, IEventRepository, ISeedable
    {
        public const string Schema = "event";
        protected const string EventsTableName = "Events";
        protected const string WorkersTableName = "Workers";

        static EventRepository()
        {
            Database.SetInitializer(new EventRepositoryInitializer());
        }

        public EventRepository()
            : base("name=Default")
        {
        }

        public void Initialize()
        {
            Database.Initialize(false);
        }

        public IDbSet<EventData> Events { get; set; }
        public IDbSet<Acknowledgement> Acknowledgements { get; set; }
        public IDbSet<Worker> Workers { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<EventData>().ToTable(EventsTableName, Schema);
            modelBuilder.Entity<Worker>().ToTable(WorkersTableName, Schema);
            modelBuilder.Entity<Acknowledgement>().ToTable("Acknowledgements", Schema);
        }

        public EventData AddEvent<T>(T data) where T : class, IDomainEvent
        {
            var e = new EventData
            {
                Id = data.EventId,
                DataType = data.GetType().Name,
                Data = JsonConvert.SerializeObject(data, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    ReferenceLoopHandling = ReferenceLoopHandling.Serialize
                }),
                Created = data.EventCreated
            };

            Events.Add(e);

            return e;
        }

        public IEnumerable<T> GetEvents<T>()
        {
            var typeName = typeof (T).Name;
            var notificationEvents = Events
                .Where(x => x.DataType == typeName);
            return
                notificationEvents
                    .ToList()
                    .Select(x => JsonConvert.DeserializeObject<T>(x.Data));
        }

        public void LockEvent(Guid eventId)
        {
            Database.ExecuteSqlCommand(
                String.Format("SELECT * FROM {0}.{1} WITH (ROWLOCK, XLOCK) WHERE Id = @Id", Schema, EventsTableName),
                new SqlParameter("@Id", eventId));
        }

        public void LockWorker(string workerTypeName)
        {
            Database.ExecuteSqlCommand(
                String.Format("SELECT * FROM {0}.{1} WITH (ROWLOCK, XLOCK) WHERE TypeName = @TypeName", Schema, WorkersTableName),
                new SqlParameter("@TypeName", workerTypeName));
        }

        public void Reload<T>(T entity) where T : class
        {
            if (entity == null)
            {
                return;
            }
            Entry(entity).Reload();
        }

        public void SaveEvent(IDomainEvent @event)
        {
            Events.Add(CreateEventData(@event));

            try
            {
                SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                if (!ex.HasDuplicatedUniqueValues())
                {
                    throw;
                }
                ex.Entries.Single().State = EntityState.Detached;
            }
        }

        public static EventData CreateEventData (IDomainEvent @event)
        {
            return new EventData
            {
                Id = @event.EventId,
                DataType = @event.GetType().Name,
                Data = JsonConvert.SerializeObject(@event, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    ReferenceLoopHandling = ReferenceLoopHandling.Serialize
                }),
                Created = @event.EventCreated
            };
        }

        public void Seed() { }

        public override int SaveChanges()
        {
            try
            {
                return base.SaveChanges();
            }
            catch (DbEntityValidationException e)
            {
                var dbValidationErrorMessages = e.EntityValidationErrors.ToArray();
                Trace.WriteLine(dbValidationErrorMessages.ToString());
                throw;
            }
        }
    }
}