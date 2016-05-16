using System;

namespace AFT.RegoV2.Core.Security.Data
{
    public class Error
    {
        public Guid Id { get; set; }
        public string Message { get; set; }
        public string Source { get; set; }
        public string Detail { get; set; }
        public string User { get; set; }
        public string HostName { get; set; }
        public string Type { get; set; }
        public DateTime Time { get; set; }
    }
}
