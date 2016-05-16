using System.ComponentModel.DataAnnotations;
using AFT.RegoV2.Core.Common.Interfaces.Admin;

namespace AFT.RegoV2.Core.Common.Data.Admin
{
    public class EditCurrencyData : IEditCurrencyData
    {
        public string OldCode { get; set; }
        public string OldName { get; set; }

        [Required(ErrorMessage = "{{\"text\": \"app:common.requiredField\"}}")]
        [MaxLength(3, ErrorMessage = "{{\"text\": \"app:common.exceedMaxLength\", \"variables\": {{\"length\": \"{1}\"}}}}")]
        [RegularExpression(@"^[a-zA-Z-]+$", ErrorMessage = "{{\"text\": \"app:language.codeCharError\"}}")]
        public string Code { get; set; }

        [Required(ErrorMessage = "{{\"text\": \"app:common.requiredField\"}}")]
        [MaxLength(20, ErrorMessage = "{{\"text\": \"app:common.exceedMaxLength\", \"variables\": {{\"length\": \"{1}\"}}}}")]
        [RegularExpression(@"^[a-zA-Z- ]+$", ErrorMessage = "{{\"text\": \"app:language.nameCharError\"}}")]
        public string Name { get; set; }

        public string Remarks { get; set; }
    }
}