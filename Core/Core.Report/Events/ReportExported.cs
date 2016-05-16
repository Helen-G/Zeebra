using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Report.Data;

namespace AFT.RegoV2.Core.Report.Events
{
    public class ReportExported : DomainEventBase
    {
        public ReportExported() { }

        public ReportExported(string reportName, ExportFormat format)
        {
            ReportName = reportName;
            Format = format;
        }

        public string ReportName { get; set; }
        public ExportFormat Format { get; set; }
    }
}
