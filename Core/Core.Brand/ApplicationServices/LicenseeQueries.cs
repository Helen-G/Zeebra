using System;
using System.Data.Entity;
using System.Linq;
using AFT.RegoV2.ApplicationServices.Security;
using AFT.RegoV2.Core.Brand.Data;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Security.Common;

namespace AFT.RegoV2.Core.Brand.ApplicationServices
{
    public class LicenseeQueries : MarshalByRefObject, IApplicationService
    {
        private readonly IBrandRepository _repository;

        public LicenseeQueries(IBrandRepository repository)
        {
            _repository = repository;
        }

        [Permission(Permissions.RenewContract, Module = Modules.LicenseeManager)]
        public Licensee GetRenewContractData(Guid id)
        {
            var licensee = _repository.Licensees
                .Include(x => x.Contracts)
                .SingleOrDefault(x => x.Id == id);

            return licensee;
        }

        public IQueryable<Licensee> GetLicensees()
        {
            return _repository.Licensees.Include(x => x.Cultures);
        }

        public ContractStatus GetContractStatus(Contract contract)
        {
            if (!contract.IsCurrentContract || contract.EndDate < DateTimeOffset.UtcNow)
                return ContractStatus.Expired;

            return contract.StartDate > DateTimeOffset.UtcNow
                ? ContractStatus.Inactive
                : ContractStatus.Active;
        }
    }
}