using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Services.Security;

namespace AFT.RegoV2.Core.Security.Events
{
    public class UserUpdated : DomainEventBase
    {
        public UserUpdated() { } // default constructor is required for publishing event to MQ

        public UserUpdated(Data.User user)
        {
            Id = user.Id;
            Username = user.Username;
            FirstName = user.FirstName;
            LastName = user.LastName;
            Language = user.Language;
            Status = user.Status.ToString();
            Description = user.Description;
            Licensees = user.Licensees.Select(l => l.Id);
            RoleId = user.Role.Id;
            RoleName = user.Role.Name;
        }

        public Guid Id { get; set; }

        public string Username { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Language { get; set; }

        public string Status { get; set; }

        public string Description { get; set; }

        public IEnumerable<Guid> Licensees { get; set; }

        public Guid RoleId { get; set; }

        public string RoleName { get; set; }

    }
}
