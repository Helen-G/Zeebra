using System;
using AFT.RegoV2.Core.Services.Security;

namespace AFT.RegoV2.Core.Security.Data
{
    public class UserLicenseeId
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }

        public User User { get; set; }
    }
}
