using System;
using System.Collections.Generic;

namespace AFT.RegoV2.Core.Bonus.Data.ViewModels
{
    public class TemplateWageringVM
    {
        public TemplateWageringVM()
        {
            GameContributions = new List<GameContributionVM>();
        }

        public bool HasWagering { get; set; }
        public WageringMethod Method { get; set; }
        public decimal Multiplier { get; set; }
        public decimal Threshold { get; set; }
        public List<GameContributionVM> GameContributions { get; set; }
        public bool IsAfterWager { get; set; }
    }

    public class GameContributionVM
    {
        public Guid GameId { get; set; }
        public string Name { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal Contribution { get; set; }
    }
}