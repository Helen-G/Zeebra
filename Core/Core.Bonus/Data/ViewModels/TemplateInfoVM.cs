using System;

namespace AFT.RegoV2.Core.Bonus.Data.ViewModels
{
    public class TemplateInfoVM
    {
        public Guid? LicenseeId { get; set; }
        public Guid? BrandId { get; set; }
        public string Name { get; set; }
        public string TemplateType { get; set; }
        public string Description { get; set; }
        public Guid WalletTemplateId { get; set; }
        public bool IsWithdrawable { get; set; }
        public string Mode { get; set; }
    }

    public enum IssuanceMode
    {
        Automatic,
        AutomaticWithCode,
        ManualByPlayer,
        ManualByCs
    }
}