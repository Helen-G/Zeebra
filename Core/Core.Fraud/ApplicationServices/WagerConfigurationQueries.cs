using System;
using System.Linq;
using AFT.RegoV2.ApplicationServices.Security;
using AFT.RegoV2.Core.Fraud.ApplicationServices.Data;
using AFT.RegoV2.Core.Fraud.Data;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Core.Services.Security;

namespace AFT.RegoV2.Core.Fraud.ApplicationServices
{
    public class WagerConfigurationQueries : MarshalByRefObject, IWagerConfigurationQueries
    {
        #region Fields

        private readonly IFraudRepository _fraudRepository;
        private readonly ISecurityRepository _securityRepository;

        #endregion

        #region Constructors

        public WagerConfigurationQueries(IFraudRepository fraudRepository, ISecurityRepository securityRepository)
        {
            _fraudRepository = fraudRepository;
            _securityRepository = securityRepository;
        }

        #endregion

        #region Methods

        private WagerConfigurationDTO GetWagerConfigurationDTO(WagerConfiguration configuration)
        {
            return new WagerConfigurationDTO()
            {
                Id = configuration.Id,
                BrandId = configuration.BrandId,
                Currency = configuration.IsServeAllCurrencies ? "All" : configuration.CurrencyCode,
                IsActive = configuration.IsActive ? "Active" : "Inactive",
                CreatedBy =
                    configuration.CreatedBy == Guid.Empty ? "" : _securityRepository.Users.First(u => u.Id == configuration.CreatedBy).FirstName,
                DateCreated = configuration.DateCreated,
                DateUpdated = configuration.DateUpdated,
                UpdatedBy =
                    configuration.UpdatedBy == Guid.Empty ? "" : _securityRepository.Users.First(u => u.Id == configuration.UpdatedBy).FirstName,
                IsDepositWageringCheck = configuration.IsDepositWageringCheck,
                IsManualAdjustmentWageringCheck = configuration.IsManualAdjustmentWageringCheck,
                IsRebateWageringCheck = configuration.IsRebateWageringCheck,
                ActivatedBy =
                    configuration.ActivatedBy == null ? "" : _securityRepository.Users.First(u => u.Id == configuration.ActivatedBy).FirstName,
                DeactivatedBy =
                    configuration.DeactivatedBy == null
                        ? ""
                        : _securityRepository.Users.First(u => u.Id == configuration.DeactivatedBy).FirstName,
                DateActivated = configuration.DateActivated,
                DateDeactivated = configuration.DateDeactivated
            };
        }

        #endregion

        #region IWagerConfigurationQueries Members

        [Permission(Permissions.View, Module = Modules.WagerConfiguration)]
        public IQueryable<WagerConfigurationDTO> GetWagerConfigurations()
        {
            var wConfigurations = _fraudRepository
                .WagerConfigurations
                .ToList()
                .Select(GetWagerConfigurationDTO);
            return wConfigurations.AsQueryable();
        }

        public WagerConfigurationDTO GetWagerConfiguration(Guid id)
        {
            var wagerConfiguration = _fraudRepository.WagerConfigurations.FirstOrDefault(x => x.Id == id);

            if (wagerConfiguration == null)
                return null;

            return GetWagerConfigurationDTO(wagerConfiguration);
        }

        #endregion
    }
}