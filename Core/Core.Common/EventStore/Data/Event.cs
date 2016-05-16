using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace AFT.RegoV2.BoundedContexts.Event.Data
{
    public class Event
    {
        // Using migration to remove clustered index from Id. PK and non-clustered index will be added automatically by EF
        public Guid Id { get; set; }

        public string DataType { get; set; }

        public string Data { get; set; }

        public EventState State { get; set; }

        [Index(IsClustered = true)]
        public DateTimeOffset Created { get; set; }

        public DateTimeOffset ReadyToPublish { get; set; }

        public DateTimeOffset Published { get; set; }
    }

    public enum EventState
    {
        New,
        Publishing,
        PublishingFailed,
        Published
    }
}