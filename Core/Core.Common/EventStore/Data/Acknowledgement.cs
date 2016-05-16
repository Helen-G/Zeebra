using System;

namespace AFT.RegoV2.BoundedContexts.Event.Data
{
    public class Acknowledgement
    {
        public Guid Id { get; set; }

        public AcknowledgementState State { get; set; }

        public DateTimeOffset Created { get; set; }

        public DateTimeOffset? Updated { get; set; }

        // Using migration to set one unique index for Event and Worker
        public virtual Event Event { get; set; }

        public virtual Worker Worker { get; set; }

    }

    public enum AcknowledgementState
    {
        New,
        Failed,
        Success,
        OutOfOrder
    }
}
