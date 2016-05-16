using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AFT.RegoV2.Domain.BoundedContexts.Report.Attributes;

namespace AFT.RegoV2.BoundedContexts.Report.Data
{
    public class PlayerRecord
    {
        [Key]
        public Guid PlayerId { get; set; }

        [Index, MaxLength(100), Export]
        public string Licensee { get; set; }

        [Index, MaxLength(100), Export]
        public string Brand { get; set; }

        [Index, MaxLength(100), Export]
        public string Username { get; set; }

        [Index, MaxLength(100), Export]
        public string Mobile { get; set; }

        [Index, MaxLength(100), Export]
        public string Email { get; set; }
        
        [Index, Export]
        public DateTimeOffset Birthday { get; set; }
        
        [Index, Export("Internal Account")]
        public bool IsInternalAccount { get; set; }
        
        [Index, Export("Registration Date")]
        public DateTimeOffset RegistrationDate { get; set; }

        [Index, MaxLength(100), Export("Player Status")]
        public string PlayerStatus { get; set; }

        [Index, MaxLength(100), Export]
        public string Language { get; set; }

        [Index, MaxLength(100), Export]
        public string Currency { get; set; }

        [Index, MaxLength(100), Export("Sign Up IP")]
        public string SignUpIP { get; set; }

        [Index, MaxLength(100), Export("VIP Level")]
        public string VipLevel { get; set; }

        [Index, MaxLength(100), Export]
        public string Country { get; set; }

        [Index, MaxLength(100), Export("Player Name")]
        public string PlayerName { get; set; }

        [Index, MaxLength(100), Export]
        public string Title { get; set; }

        [Index, MaxLength(100), Export]
        public string Gender { get; set; }

        [Index, MaxLength(100), Export("Street Address")]
        public string StreetAddress { get; set; }

        [Index, MaxLength(100), Export("Post Code")]
        public string PostCode { get; set; }
        
        [Index, Export("Date Created")]
        public DateTimeOffset Created { get; set; }

        [Index, MaxLength(100), Export("Created By")]
        public string CreatedBy { get; set; }
        
        [Index, Export("Date Updated")]
        public DateTimeOffset? Updated { get; set; }

        [Index, MaxLength(100), Export("Updated By")]
        public string UpdatedBy { get; set; }
        
        [Index, Export("Date Activated")]
        public DateTimeOffset? Activated { get; set; }

        [Index, MaxLength(100), Export("Activated By")]
        public string ActivatedBy { get; set; }
        
        [Index, Export("Date Deactivated")]
        public DateTimeOffset? Deactivated { get; set; }

        [Index, MaxLength(100), Export("Deactivated By")]
        public string DeactivatedBy { get; set; }
    }
}