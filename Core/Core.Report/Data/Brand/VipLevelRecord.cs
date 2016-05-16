using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AFT.RegoV2.Domain.BoundedContexts.Report.Attributes;

namespace AFT.RegoV2.BoundedContexts.Report.Data
{
    public class VipLevelRecord
    {
        public Guid Id { get; set; }

        [Index]
        public Guid VipLevelId { get; set; }

        [Index]
        public Guid? VipLevelLimitId { get; set; }

        [Index, MaxLength(200), Export]
        public string Licensee { get; set; }

        [Index, MaxLength(200), Export]
        public string Brand { get; set; }

        [Index, MaxLength(100), Export("VIP Level")]
        public string Code { get; set; }

        [Index, Export]
        public int Rank { get; set; }

        [Index, MaxLength(100), Export]
        public string Status { get; set; }

        [Index, MaxLength(200), Export("Game Provider")]
        public string GameProvider { get; set; }

        [Index, MaxLength(100), Export]
        public string Currency { get; set; }

        [Index, MaxLength(200), Export("Bet Limit")]
        public string BetLevel { get; set; }

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