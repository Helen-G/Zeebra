using System;
using System.ComponentModel.DataAnnotations;

namespace AFT.RegoV2.Core.Brand.Data
{
    public class ContentTranslation
    {
        public Guid Id { get; set; }

        [MaxLength(50), Required]
        public string Name { get; set; }

        [MaxLength(200), Required]
        public string Source { get; set; }

        [MaxLength(200),Required]
        public string Translation { get; set; }

        public string Language { get; set; }

        public TranslationStatus Status { get; set; }

        public string Remark { get; set; }

        [MaxLength(200)]
        public string CreatedBy { get; set; }
       
        public DateTimeOffset Created { get; set; }
     
        public string UpdatedBy { get; set; }
      
        public DateTimeOffset? Updated { get; set; }

        public string ActivatedBy { get; set; }

        public DateTimeOffset? Activated { get; set; }
     
        public string DeactivatedBy { get; set; }

        public DateTimeOffset? Deactivated { get; set; }

        public override bool Equals(object obj)
        {
            var contentTranslation = obj as ContentTranslation;
            return contentTranslation != null && Name.Equals(contentTranslation.Name);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }

    public enum TranslationStatus
    {
        Enabled,
        Disabled
    }
}
