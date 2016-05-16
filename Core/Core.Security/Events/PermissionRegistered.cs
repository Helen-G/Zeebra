using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Domain.Security.Events
{
    public class PermissionRegistered : DomainEventBase
    {
        public PermissionRegistered() { } // default constructor is required for publishing event to MQ

        public PermissionRegistered(string name, string category)
        {
            Name = name;
            Category = category;
        }

        public string Name { get; set; }

        public string Category { get; set; }

    }
}
