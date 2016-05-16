using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AFT.RegoV2.Domain.BoundedContexts.Report.Attributes;

namespace AFT.RegoV2.BoundedContexts.Report.Data
{
    public class BrandRecord
    {
        [Key]
        public Guid BrandId { get; set; }

        [Index, MaxLength(200), Export]
        public string Licensee { get; set; }

        [Index, MaxLength(100), Export("Brand Code")]
        public string BrandCode { get; set; }

        [Index, MaxLength(200), Export("Brand Name")]
        public string Brand { get; set; }

        [Index, MaxLength(100), Export("Brand Type")]
        public string BrandType { get; set; }

        [Index, MaxLength(100), Export("Player Prefix")]
        public string PlayerPrefix { get; set; }

        [Index, Export("Number of Allowed Internal Accounts")]
        public int AllowedInternalAccountsNumber { get; set; }

        [Index, MaxLength(100), Export("Brand Status")]
        public string BrandStatus { get; set; }

        [Index, MaxLength(100), Export("Brand Time Zone")]
        public string BrandTimeZone { get; set; }

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
        
        [Index, MaxLength(1000), Export]
        public string Remarks { get; set; }

        [Index, Export]
        public Guid? DefaultVipLevelId { get; set; }
    }
}