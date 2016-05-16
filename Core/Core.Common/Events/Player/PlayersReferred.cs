using System;
using System.Collections.Generic;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Common.Events.Player
{
    public class PlayersReferred : DomainEventBase
    {
        public PlayersReferred() { } // default constructor is required for publishing event to MQ

        public PlayersReferred(Guid referrerId, List<string> phoneNumbers)
        {
            ReferrerId = referrerId;
            PhoneNumbers = phoneNumbers;
        }
        public Guid ReferrerId { get; set; }
        public List<string> PhoneNumbers { get; set; }
    }
}
