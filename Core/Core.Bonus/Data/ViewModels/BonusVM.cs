using System;

namespace AFT.RegoV2.Core.Bonus.Data.ViewModels
{
    /// <summary>
    /// This DTO class will contain properties that are configurable by UI elements on Bonus Creation screen
    /// </summary>
    public class BonusVM
    {
        public Guid Id { get; set; }
        public int Version { get; set; }
        public Guid? TemplateId { get; set; }
        public int? TemplateVersion { get; set; }
        public string LicenseeName { get; set; }
        public string BrandName { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string ActiveFrom { get; set; }
        public string ActiveTo { get; set; }
        public string Description { get; set; }
        public string DurationType { get; set; }
        public string DurationStart { get; set; }
        public string DurationEnd { get; set; }
        public int DaysToClaim { get; set; }

        public bool IsActive { get; set; }
        public string Type { get; set; }

        public int DurationDays { get; set; }
        public int DurationHours { get; set; }
        public int DurationMinutes { get; set; }
    }

    public enum BonusType
    {
        ReloadDeposit,
        FirstDeposit,
        HighDeposit,
        ReferFriend,
        MobilePlusEmailVerification,
        FundIn
    }
}