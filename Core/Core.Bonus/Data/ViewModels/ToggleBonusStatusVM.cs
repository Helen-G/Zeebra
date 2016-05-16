using System;

namespace AFT.RegoV2.Core.Bonus.Data.ViewModels
{
    public class ToggleBonusStatusVM
    {
        public Guid Id { get; set; }
        public bool IsActive { get; set; }
        public string Description { get; set; }
    }
}
