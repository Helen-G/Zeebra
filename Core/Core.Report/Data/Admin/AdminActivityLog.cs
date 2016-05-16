using System;

namespace AFT.RegoV2.Core.Report.Data.Admin
{
    public class AdminActivityLog
    {
        public Guid Id { get; set; }
        public AdminActivityLogCategory Category { get; set; }
        public string ActivityDone { get; set; }
        public string PerformedBy { get; set; }
        public DateTimeOffset DatePerformed { get; set; }
        public string Remarks { get; set; }
    }
}
