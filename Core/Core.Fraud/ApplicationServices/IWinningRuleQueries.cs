using System;
using System.Collections.Generic;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Fraud.Data;

namespace AFT.RegoV2.Core.Fraud.ApplicationServices
{
    public interface IWinningRuleQueries : IApplicationService
    {
        IEnumerable<WinningRule> GetWinningRules(Guid avcId);
    }
}