using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using AFT.RegoV2.BoundedContexts.Event;
using AFT.RegoV2.BoundedContexts.Event.Data;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Common.Utils;
using Newtonsoft.Json;
using EventData = AFT.RegoV2.BoundedContexts.Event.Data.Event;

namespace AFT.RegoV2.Core.Event.ApplicationServices
{
    /// <summary>
    /// Please, avoid putting anything into this service, as this class is deprecated 
    /// and should be removed with the entire Core.Event assembly
    /// </summary>
    public class EventService
    {
        private readonly IEventRepository _repository;

        public EventService(IEventRepository repository)
        {
            _repository = repository;
        }

        

        public bool SaveWorker(string workerTypeName)
        {
            _repository.Workers.Add(new Worker
            {
                Id = Guid.NewGuid(),
                TypeName = workerTypeName,
                State = WorkerState.New
            });
            try
            {
                _repository.SaveChanges();
                return true;
            }
            catch (DbUpdateException ex)
            {
                if (!ex.HasDuplicatedUniqueValues())
                {
                    throw;
                }
                ex.Entries.Single().State = EntityState.Detached;
                return false;
            }
        }

        public bool SaveAcknoledgement(Guid eventId, string workerTypeName)
        {
            var @event = GetEvent(eventId);
            var worker = GetWorker(workerTypeName);
            _repository.Acknowledgements.Add(new Acknowledgement
            {
                Id = Identifier.NewSequentialGuid(),
                Event = @event,
                Worker = worker,
                State = AcknowledgementState.New,
                Created = DateTimeOffset.Now
            });
            try
            {
                _repository.SaveChanges();
                return true;
            }
            catch (DbUpdateException ex)
            {
                if (!ex.HasDuplicatedUniqueValues())
                {
                    throw;
                }
                ex.Entries.Single().State = EntityState.Detached;
                return false;
            }
        }


        public Worker ReloadWorker(string workerTypeName)
        {
            var worker = GetWorker(workerTypeName);
            if (worker != null)
            {
                _repository.Reload(worker);
            }
            return worker;
        }

        public void SetWorkerState(string workerTypeName, WorkerState state)
        {
            GetWorker(workerTypeName).State = state;
            _repository.SaveChanges();
        }

        public void SetLastReplayedEvent(string workerTypeName, EventData @event)
        {
            var worker = GetWorker(workerTypeName);
            if (@event != null && worker.LastReplayedEvent != null &&
                @event.Created < worker.LastReplayedEvent.Created)
            {
                return;
            }
            worker.LastReplayedEvent = @event != null
                ? GetEvent(@event.Id)
                : null;
            _repository.SaveChanges();
        }

        public Worker GetLockedWorker(string workerTypeName)
        {
            _repository.LockWorker(workerTypeName);
            return GetWorker(workerTypeName);
        }

        public Worker GetWorker(string workerTypeName)
        {
            return _repository.Workers
                .Include(w => w.LastReplayedEvent)
                .SingleOrDefault(w => w.TypeName == workerTypeName);
        }

        public void SetAcknoledmentState(Guid eventId, string workerTypeName, AcknowledgementState state)
        {
            var acknoledgement =
                _repository.Acknowledgements
                .Include(a => a.Event)
                .Include(a => a.Worker)
                .Single(a => a.Event.Id == eventId && a.Worker.TypeName == workerTypeName);
            acknoledgement.State = state;
            acknoledgement.Updated = DateTimeOffset.Now;
            _repository.SaveChanges();
        }

        public Acknowledgement GetAcknoledgement(Guid eventId, string workerTypeName)
        {
            return _repository.Acknowledgements
                .Include(a => a.Event)
                .Include(a => a.Worker)
                .SingleOrDefault(a => a.Event.Id == eventId && a.Worker.TypeName == workerTypeName);
        }

        public EventData LockNewEventForPublishing()
        {
            for (var attemptCount = 0; attemptCount < 10; attemptCount++)
            {
                var newEvent = GetNewEventOrNull();
                if (newEvent == null)
                {
                    return null;
                }
                var eventId = newEvent.Id;
                using (var scope = CustomTransactionScope.GetTransactionScope())
                {
                    var @event = GetLockedEvent(eventId);
                    _repository.Reload(@event);
                    if (@event.State != EventState.New && @event.State != EventState.PublishingFailed)
                    {
                        continue;
                    }
                    SetEventState(@event, EventState.Publishing);
                    scope.Complete();
                }
                return GetEvent(eventId);
            }
            return null;
        }

        public EventData GetNewEventOrNull()
        {
            return _repository.Events.OrderBy(e => e.Created).FirstOrDefault(e =>
                e.State == EventState.New || e.State == EventState.PublishingFailed);
        }

        private EventData GetLockedEvent(Guid eventId)
        {
            _repository.LockEvent(eventId);
            return GetEvent(eventId);
        }

        private EventData GetEvent(Guid eventId)
        {
            return _repository.Events.Single(e => e.Id == eventId);
        }

        public void SetEventState(EventData @event, EventState state)
        {
            @event.State = state;
            switch (state)
            {
                case EventState.Publishing:
                    @event.ReadyToPublish = DateTimeOffset.UtcNow;
                    break;
                case EventState.Published:
                    @event.Published = DateTimeOffset.UtcNow;
                    break;
            }
            _repository.SaveChanges();
        }

    }

    public static class DbUpdateExceptionExtensions
    {
        public static bool HasDuplicatedUniqueValues(this DbUpdateException exception)
        {
            if (exception.InnerException == null || exception.InnerException.InnerException == null)
            {
                return false;
            }

            var errorNumber = ((SqlException)exception.InnerException.InnerException).Number;
            return errorNumber == 2601 || errorNumber == 2627;
        }
    }
}
