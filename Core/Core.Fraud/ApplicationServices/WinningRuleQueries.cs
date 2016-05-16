using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using AFT.RegoV2.Core.Fraud.Data;

namespace AFT.RegoV2.Core.Fraud.ApplicationServices
{
    public class WinningRuleQueries : IWinningRuleQueries
    {
        private readonly IFraudRepository _repository;

        public WinningRuleQueries(IFraudRepository repository)
        {
            _repository = repository;
        }

        public IEnumerable<WinningRule> GetWinningRules(Guid avcId)
        {
            return _repository.AutoVerificationCheckConfigurations
                .Include(o => o.WinningRules)
                .Single(o => o.Id == avcId)
                .WinningRules;
        }
    }
}
