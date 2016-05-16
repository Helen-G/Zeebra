using System;
using System.Linq;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Fraud.ApplicationServices.Data;

namespace AFT.RegoV2.Core.Fraud.ApplicationServices
{
    public interface IWagerConfigurationQueries : IApplicationService
    {
        IQueryable<WagerConfigurationDTO> GetWagerConfigurations();
        WagerConfigurationDTO GetWagerConfiguration(Guid id);
    }
}