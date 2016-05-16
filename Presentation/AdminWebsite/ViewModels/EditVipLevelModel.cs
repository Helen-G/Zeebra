using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AFT.RegoV2.AdminWebsite.ViewModels
{
    public class EditVipLevelLimitModel
    {
        public Guid GameId { get; set; }
        public string CurrencyCode { get; set; }
        public decimal? Minimum { get; set; }
        public decimal? Maximum { get; set; }
    }

    public class EditVipLevelModel
    {
        public EditVipLevelModel()
        {
            Limits = new List<EditVipLevelLimitModel>();
        }

        public Guid? Id { get; set; }

        [Required(ErrorMessage = "{{\"text\": \"app:common.requiredField\"}}")]
        public Guid Brand { get; set; }

        public bool IsDefault { get; set; }

        [Required(ErrorMessage = "{{\"text\": \"app:common.requiredField\"}}")]
        public string Code { get; set; }

        [Required(ErrorMessage = "{{\"text\": \"app:common.requiredField\"}}")]
        public string Name { get; set; }

        [MaxLength(200, ErrorMessage = "{{\"text\": \"app:common.exceedMaxLength\", \"variables\": {{\"name\": \"{0}\", \"length\": \"{1}\"}}}}")]
        public string Description { get; set; }

        [MaxLength(7, ErrorMessage = "{{\"text\": \"app:common.exceedMaxLength\", \"variables\": {{\"name\": \"{0}\", \"length\": \"{1}\"}}}}")]
        public string Color { get; set; }

        public IEnumerable<EditVipLevelLimitModel> Limits { get; set; }
    }
}