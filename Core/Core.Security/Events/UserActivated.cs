using System;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Domain.Security.Events
{
    public class UserActivated : DomainEventBase
    {
        public UserActivated() { } // default constructor is required for publishing event to MQ

        public UserActivated(Guid userId)
        {
            Id = userId;
        }

        public Guid Id { get; set; }

    }
}
