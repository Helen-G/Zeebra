using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AFT.RegoV2.Core.Report.Data.Admin
{
    public class AdminAuthenticationLog
    {
        public Guid Id { get; set; }
        [Index, MaxLength(100)]
        public string PerformedBy { get; set; }
        [Index]
        public DateTimeOffset DatePerformed { get; set; }
        [Index, MaxLength(100)]
        public string IPAddress { get; set; }
        public string Headers { get; set; }
        [Index, MaxLength(200)]
        public string FailReason { get; set; }
        public bool Success
        {
            get { return FailReason == null; }
        }

    }
}
