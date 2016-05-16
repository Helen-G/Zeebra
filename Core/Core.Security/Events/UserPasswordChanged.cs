using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Security.Events
{
    public class UserPasswordChanged :DomainEventBase
    {
        public Guid Id { get; set; }

        public UserPasswordChanged()
        {
            
        }

        public UserPasswordChanged(Guid userId)
        {
            Id = userId;
        }
    }
}
