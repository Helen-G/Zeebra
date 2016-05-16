using System;
using System.Collections.Generic;

namespace AFT.RegoV2.Core.Common.Data
{
    public class ReferralData
    {
        public Guid ReferrerId { get; set; }
        public List<string> PhoneNumbers { get; set; }
    }
}
