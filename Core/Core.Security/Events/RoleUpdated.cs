using System;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Security.Data;

namespace AFT.RegoV2.Domain.Security.Events
{
    public class RoleUpdated : DomainEventBase
    {
        public RoleUpdated() { } // default constructor is required for publishing event to MQ

        public RoleUpdated(Role role)
        {
            Id = role.Id;
            Code = role.Code;
            Name = role.Name;
            Description = role.Description;
        }

        public Guid Id { get; set; }

        public string Code { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

    }
}
