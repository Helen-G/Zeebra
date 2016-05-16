using System;
using System.Collections.Generic;

namespace AFT.RegoV2.AdminWebsite
{
    [Serializable]
    public class AuthUser
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; } // duplicating it here for consistency
        public ICollection<Guid> BrandIds { get; set; }
        public string Token { get; set; }
        public string RefreshToken { get; set; }
    }
}