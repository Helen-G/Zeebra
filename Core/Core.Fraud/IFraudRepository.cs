using System.Data.Entity;
using AFT.RegoV2.Core.Fraud.Data;

namespace AFT.RegoV2.Core.Fraud
{
    public interface IFraudRepository
    {
        IDbSet<Data.Brand> Brands { get; }
        IDbSet<RiskLevel> RiskLevels { get; }
        IDbSet<PlayerRiskLevel> PlayerRiskLevels { get; }
        IDbSet<WagerConfiguration> WagerConfigurations { get; }
        IDbSet<AutoVerificationCheckConfiguration> AutoVerificationCheckConfigurations { get; }
        IDbSet<WinningRule> WinningRules { get; }
        IDbSet<PaymentLevel> PaymentLevels { get; set; } 
        int SaveChanges();
    }
}