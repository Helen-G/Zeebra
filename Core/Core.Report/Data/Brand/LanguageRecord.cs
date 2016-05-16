using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AFT.RegoV2.Domain.BoundedContexts.Report.Attributes;

namespace AFT.RegoV2.Core.Report.Data.Brand
{
    public class LanguageRecord
    {
        public Guid Id { get; set; }

        [Index, MaxLength(100), Export("Language Code")]
        public string Code { get; set; }

        [Index, MaxLength(200), Export("Language Name")]
        public string Name { get; set; }

        [Index, MaxLength(200), Export("Native Name")]
        public string NativeName { get; set; }

        [Index, MaxLength(100), Export]
        public string Status { get; set; }

        [Index, MaxLength(200), Export("Assigned in Licensee")]
        public string Licensee { get; set; }

        [Index, MaxLength(200), Export("Assigned in Brand")]
        public string Brand { get; set; }

        [Index, MaxLength(200), Export("Created By")]
        public string CreatedBy { get; set; }

        [Index, Export("Date Created")]
        public DateTimeOffset Created { get; set; }

        [Index, MaxLength(200), Export("Updated By")]
        public string UpdatedBy { get; set; }

        [Index, Export("Date Updated")]
        public DateTimeOffset? Updated { get; set; }

        [Index, MaxLength(200), Export("Activated By")]
        public string ActivatedBy { get; set; }

        [Index, Export("Date Activated")]
        public DateTimeOffset? Activated { get; set; }

        [Index, MaxLength(200), Export("Deactivated By")]
        public string DeactivatedBy { get; set; }

        [Index, Export("Date Deactivated")]
        public DateTimeOffset? Deactivated { get; set; }

    }
}
