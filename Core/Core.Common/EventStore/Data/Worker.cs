using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AFT.RegoV2.BoundedContexts.Event.Data
{
    public class Worker
    {
        public Guid Id { get; set; }

        [Index(IsUnique = true), MaxLength(200)]
        public string TypeName { get; set; }

        public WorkerState State { get; set; }

        public virtual Event LastReplayedEvent { get; set; }
    }

    public enum WorkerState
    {
        New,
        ReplayEventStore,
        ReplayEventStoreFailed,
        ListenFromMessageQueue
    }
}
