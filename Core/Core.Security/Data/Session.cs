using System;

namespace AFT.RegoV2.Core.Security.Data
{
    public class Session
    {
        public Guid Id { get; set; }
        public string SessionId { get; set; }
        public string ApplicationName { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public DateTimeOffset ExpireDate { get; set; }
        public DateTimeOffset LockDate { get; set; }
        public int LockId { get; set; }
        public int Timeout { get; set; }
        public bool Locked { get; set; }
        public string SessionItems { get; set; }
        public int Flags { get; set; }
    }
}
