using System;

namespace AFT.RegoV2.Core.Fraud.Data
{
    public class WinningRule
    {
        public Guid Id { get; set; }

        public Guid ProductId { get; set; }
        public ComparisonEnum Comparison { get; set; }
        public decimal Amount { get; set; }

        public PeriodEnum Period { get; set; }
        public DateTimeOffset? StartDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }

        public Guid AutoVerificationCheckConfigurationId { get; set; }
        public AutoVerificationCheckConfiguration AutoVerificationCheckConfiguration { get; set; }
    }
}