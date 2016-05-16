using System;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Security.Events
{
    public class ErrorRaised : DomainEventBase
    {
        public ErrorRaised() { } // default constructor is required for publishing event to MQ

        public Guid Id { get; set; }
        public string Message { get; set; }
        public string Source { get; set; }
        public string Detail { get; set; }
        public string User { get; set; }
        public string HostName { get; set; }
        public string Type { get; set; }
        public DateTime Time { get; set; }
    }
}
