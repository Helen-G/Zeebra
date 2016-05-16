using System;

namespace AFT.RegoV2.Domain.BoundedContexts.Report.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ExportAttribute : Attribute
    {
        public ExportAttribute()
        {
        }

        public ExportAttribute(string columnName)
        {
            ColumnName = columnName;
        }

        public string ColumnName { get; set; }
    }
}
