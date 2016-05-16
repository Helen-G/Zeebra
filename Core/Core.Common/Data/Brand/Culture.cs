using System;
using System.ComponentModel.DataAnnotations;

namespace AFT.RegoV2.Core.Common.Brand.Data
{
    public class Culture
    {
        [Required, StringLength(10)]
        public string Code { get; set; }

        [Required, StringLength(50)]
        public string Name { get; set; }

        [Required, StringLength(50)]
        public string NativeName { get; set; }

        public CultureStatus Status { get; set; }
        public string CreatedBy { get; set; }
        public DateTimeOffset DateCreated { get; set; }
        public string UpdatedBy { get; set; }
        public DateTimeOffset? DateUpdated { get; set; }

        public string ActivatedBy { get; set; }
        public DateTimeOffset? DateActivated { get; set; }

        public string DeactivatedBy { get; set; }
        public DateTimeOffset? DateDeactivated { get; set; }

        public override bool Equals(object obj)
        {
            var culture = obj as Culture;
            return culture != null && Code.Equals(culture.Code);
        }

        public override int GetHashCode()
        {
            return Code.GetHashCode();
        }
    }

    public enum CultureStatus
    {
        Active,
        Inactive
    }
}