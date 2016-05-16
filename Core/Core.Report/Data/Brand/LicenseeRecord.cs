using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AFT.RegoV2.Domain.BoundedContexts.Report.Attributes;

namespace AFT.RegoV2.BoundedContexts.Report.Data
{
    public class LicenseeRecord
    {
        [Key]
        public Guid LicenseeId { get; set; }

        [Index, MaxLength(200), Export("Licensee Name")]
        public string Name { get; set; }

        [Index, MaxLength(200), Export("Company Name")]
        public string CompanyName { get; set; }

        [Index, MaxLength(200), Export("Email Address")]
        public string EmailAddress { get; set; }

        [Index, Export("Affiliate System")]
        public bool AffiliateSystem { get; set; }

        [Index, MaxLength(100), Export]
        public string Status { get; set; }

        [Index, Export("Contract Start")]
        public DateTimeOffset ContractStart { get; set; }

        [Index, Export("Contract End")]
        public DateTimeOffset? ContractEnd { get; set; }

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