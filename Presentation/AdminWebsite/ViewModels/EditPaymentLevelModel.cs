using System;
using System.ComponentModel.DataAnnotations;

namespace AFT.RegoV2.AdminWebsite.ViewModels
{
    public class EditPaymentLevelModel
    {
        public Guid? Id { get; set; }

        public Guid? Brand { get; set; }

        public string Currency { get; set; }

        [Required(ErrorMessage = "{{\"text\": \"app:common.requiredField\"}}"), MaxLength(20, ErrorMessage = "{{\"text\": \"app:common.exceedMaxLength\", \"variables\": {{\"name\": \"{0}\", \"length\": \"{1}\"}}}}")]
        public string Name { get; set; }

        [Required(ErrorMessage = "{{\"text\": \"app:common.requiredField\"}}"), MaxLength(20, ErrorMessage = "{{\"text\": \"app:common.exceedMaxLength\", \"variables\": {{\"name\": \"{0}\", \"length\": \"{1}\"}}}}")]
        public string Code { get; set; }

        public bool EnableOfflineDeposit { get; set; }

        public bool IsDefault { get; set; }

        public string[] BankAccounts { get; set; }
    }
}