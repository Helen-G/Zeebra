using System;

namespace AFT.RegoV2.Core.Bonus.Data.ViewModels
{
    public class TemplateVM
    {
        public Guid Id { get; set; }
        public int Version { get; set; }

        public TemplateInfoVM Info { get; set; }
        public TemplateAvailabilityVM Availability { get; set; }
        public TemplateRulesVM Rules { get; set; }
        public TemplateWageringVM Wagering { get; set; }
        public TemplateNotification Notification { get; set; }
    }
}