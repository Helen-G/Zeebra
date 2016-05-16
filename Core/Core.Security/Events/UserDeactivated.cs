using System;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Domain.Security.Events
{
    public class UserDeactivated : DomainEventBase
    {
        public UserDeactivated() { } // default constructor is required for publishing event to MQ

        public UserDeactivated(Guid userId)
        {
            Id = userId;
        }

        public Guid Id { get; set; }

    }
}
