using System;
using AFT.RegoV2.Core.Common.Brand.Data;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Common.Brand.Events
{
    public class LanguageCreated : DomainEventBase
    {
        public LanguageCreated() { }

        public LanguageCreated(Culture culture)
        {
            Code = culture.Code;
            Name = culture.Name;
            NativeName = culture.NativeName;
            Status = culture.Status;
            CreatedBy = culture.CreatedBy;
            DateCreated = culture.DateCreated;
        }

        public string Code { get; set; }
        public string Name { get; set; }
        public string NativeName { get; set; }
        public CultureStatus Status { get; set; }
        public string CreatedBy { get; set; }
        public DateTimeOffset DateCreated { get; set; }
    }
}