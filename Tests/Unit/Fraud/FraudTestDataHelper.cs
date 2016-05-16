using System;
using AFT.RegoV2.Core.Fraud.Data;

namespace AFT.RegoV2.Tests.Unit.Fraud
{
    public static class FraudTestDataHelper
    {
        public static WinningRuleDTO GenerateWinningRule(Guid? productId = null)
        {
            return new WinningRuleDTO
            {
                Id = Guid.NewGuid(),
                Period = PeriodEnum.Last7Days,
                Amount = 100,
                ProductId = productId ?? Guid.NewGuid(),
                Comparison = ComparisonEnum.GreaterOrEqual
            };
        }
    }
}
