using System;
using AFT.RegoV2.Shared;

namespace AFT.RegoV2.MemberApi.Interface.Security
{
    public class ApplicationErrorRequest 
    {
        public string Message { get; set; }
        public string Source { get; set; }
        public string Detail { get; set; }
        public string User { get; set; }
        public string HostName { get; set; }
        public string Type { get; set; }
        public DateTime Time { get; set; }
    }

    public class ApplicationErrorResponse 
    {
    }
}
