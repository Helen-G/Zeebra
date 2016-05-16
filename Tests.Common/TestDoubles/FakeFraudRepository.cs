using System.Data.Entity;
using System.Linq;
using AFT.RegoV2.Core.Fraud;
using AFT.RegoV2.Core.Fraud.Data;

namespace AFT.RegoV2.Tests.Common.TestDoubles
{
    public class FakeFraudRepository : IFraudRepository
    {
        #region Fields

        private readonly FakeDbSet<Brand> _brands = new FakeDbSet<Brand>();
        private readonly FakeDbSet<RiskLevel> _riskLevels = new FakeDbSet<RiskLevel>();
        private readonly FakeDbSet<PlayerRiskLevel> _playerRiskLevels = new FakeDbSet<PlayerRiskLevel>();
        private readonly FakeDbSet<WagerConfiguration> _wagerConfigurations = new FakeDbSet<WagerConfiguration>();
        private readonly FakeDbSet<AutoVerificationCheckConfiguration> _autoVerificationCheckConfigurations = new FakeDbSet<AutoVerificationCheckConfiguration>();
        private readonly FakeDbSet<WinningRule> _winningRules = new FakeDbSet<WinningRule>();

        #endregion

        #region IFraudRepository Members

        public IDbSet<Brand> Brands
        {
            get { return _brands; }
        }

        public IDbSet<RiskLevel> RiskLevels
        {
            get { return _riskLevels; }
        }

        public IDbSet<PlayerRiskLevel> PlayerRiskLevels
        {
            get { return _playerRiskLevels; }
        }
        
        public IDbSet<AutoVerificationCheckConfiguration> AutoVerificationCheckConfigurations
        {
            get { return _autoVerificationCheckConfigurations; }
        }

        public IDbSet<WagerConfiguration> WagerConfigurations
        {
            get { return _wagerConfigurations; }
        }

        public IDbSet<WinningRule> WinningRules
        {
            get { return _winningRules; }
        }

        public IDbSet<PaymentLevel> PaymentLevels { get; set; }

        public int SaveChanges()
        {
            return 0;
        }

        #endregion
    }
}