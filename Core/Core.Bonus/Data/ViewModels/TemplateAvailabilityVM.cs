using System;
using System.Collections.Generic;

namespace AFT.RegoV2.Core.Bonus.Data.ViewModels
{
    public class TemplateAvailabilityVM
    {
        public TemplateAvailabilityVM()
        {
            VipLevels = new List<string>();
            ExcludeBonuses = new List<Guid>();
            ExcludeRiskLevels = new List<Guid>();
        }

        public Guid? ParentBonusId { get; set; }
        public string PlayerRegistrationDateFrom { get; set; }
        public string PlayerRegistrationDateTo { get; set; }
        public int WithinRegistrationDays { get; set; }
        public List<string> VipLevels { get; set; }
        public Operation ExcludeOperation { get; set; }
        public List<Guid> ExcludeBonuses { get; set; }
        public List<Guid> ExcludeRiskLevels { get; set; }
        public int PlayerRedemptionsLimit { get; set; }
        public BonusPlayerRedemptionsLimitType PlayerRedemptionsLimitType { get; set; }
        public int RedemptionsLimit { get; set; }
    }
}