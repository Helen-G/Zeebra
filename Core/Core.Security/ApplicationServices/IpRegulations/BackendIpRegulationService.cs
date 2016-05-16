using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AFT.RegoV2.ApplicationServices.Security;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Security.ApplicationServices.Data.IpRegulations;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Core.Security.Data;
using AFT.RegoV2.Core.Services.Security;
using AFT.RegoV2.Domain.Security;
using AFT.RegoV2.Domain.Security.Data;
using AFT.RegoV2.Domain.Security.Events;
using AFT.RegoV2.Domain.Security.Interfaces;
using AFT.RegoV2.Shared;
using AutoMapper;

namespace AFT.RegoV2.Core.Security.ApplicationServices
{
    public class BackendIpRegulationService : IpRegulationServiceBase
    {
        private const string DisableIpRegulationsSetting = "disable-ip-regulations";
        private readonly IEventBus _eventBus;

        private bool IpVerificationDisabled
        {
            get
            {
                var isIpRegulationDisabledDb =
                _repository.AdminIpRegulationSettings.FirstOrDefault(s => s.Name == DisableIpRegulationsSetting);

                var isIpRegulationDisabled = true;
                if (isIpRegulationDisabledDb == null) return isIpRegulationDisabled;

                if (!bool.TryParse(isIpRegulationDisabledDb.Value, out isIpRegulationDisabled))
                    throw new RegoException("Flag disable-ip-regulations set to incorrect value");

                return isIpRegulationDisabled;
            }

            set
            {
                var isIpRegulationDisabledDb =
                _repository.AdminIpRegulationSettings.FirstOrDefault(s => s.Name == DisableIpRegulationsSetting);

                if (isIpRegulationDisabledDb == null)
                {
                    isIpRegulationDisabledDb = new AdminIpRegulationSetting
                    {
                        Id = Guid.NewGuid(),
                        Name = DisableIpRegulationsSetting
                    };

                    _repository.AdminIpRegulationSettings.Add(isIpRegulationDisabledDb);
                }

                isIpRegulationDisabledDb.Value = value.ToString();

                _repository.SaveChanges();
            }
        }

        public BackendIpRegulationService(
            ISecurityRepository repository, 
            ISecurityProvider data,
            IEventBus eventBus
            )
            :base(repository, data)
        {
            _eventBus = eventBus;
            
        }

        public void DisableIpVerification()
        {
            IpVerificationDisabled = true;
        }

        public void EnableIpVerification()
        {
            IpVerificationDisabled = false;
        }

        [Permission(Permissions.View, Module = Modules.BackendIpRegulationManager)]
        public IQueryable<AdminIpRegulation> GetIpRegulations()
        {
            return _repository.AdminIpRegulations.Include(ip => ip.CreatedBy).Include(ip => ip.UpdatedBy).AsQueryable();
        }

        public AdminIpRegulation GetIpRegulation(Guid id)
        {
            return GetIpRegulations().SingleOrDefault(ip => ip.Id == id);
        }

        [Permission(Permissions.Add, Module = Modules.BackendIpRegulationManager)]
        public AdminIpRegulation CreateIpRegulation(AddBackendIpRegulationData data)
        {
            var regulation = Mapper.DynamicMap<AdminIpRegulation>(data);

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                regulation.CreatedBy = _repository.Users.SingleOrDefault(u => u.Id == _securityProvider.User.UserId);
                regulation.CreatedDate = DateTime.Now;
                regulation.Id = Guid.NewGuid();
                _repository.AdminIpRegulations.Add(regulation);
                _repository.SaveChanges();

                _eventBus.Publish(new AdminIpRegulationCreated(regulation));
                scope.Complete();
            }

            return regulation;
        }

        [Permission(Permissions.Edit, Module = Modules.BackendIpRegulationManager)]
        public AdminIpRegulation UpdateIpRegulation(EditBackendIpRegulationData data)
        {
            var regulation = _repository.AdminIpRegulations.SingleOrDefault(ip => ip.Id == data.Id);

            if (regulation == null)
            {
                throw new RegoException("User does not exist");
            }

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                regulation.IpAddress = data.IpAddress;
                regulation.Description = data.Description;

                regulation.UpdatedBy = _repository.Users.SingleOrDefault(u => u.Id == _securityProvider.User.UserId);
                regulation.UpdatedDate = DateTime.Now;

                _repository.SaveChanges();

                _eventBus.Publish(new AdminIpRegulationUpdated(regulation));
                scope.Complete();
            }

            return regulation;
        }

        [Permission(Permissions.Delete, Module = Modules.BackendIpRegulationManager)]
        public void DeleteIpRegulation(Guid id)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var ipRegulation = GetIpRegulation(id);
                _repository.AdminIpRegulations.Remove(ipRegulation);

                _repository.SaveChanges();

                _eventBus.Publish(new AdminIpRegulationDeleted(ipRegulation));
                scope.Complete();
            }
        }

        public bool IsIpAddressUnique(string address)
        {
            return !_repository
                .AdminIpRegulations.ToList()
                .Any(ip => IsRangesIntersects(ip.IpAddress, address));
        }

        public bool VerifyIpAddress(string ipAddress)
        {
            if (IpVerificationDisabled || IsLocalhost(ipAddress))
            {
                return true;
            }

            var ipRegulation =
                    _repository.AdminIpRegulations.ToList().FirstOrDefault(
                    ip =>
                        IsRangesIntersects(ip.IpAddress, ipAddress));

            return  ipRegulation != null;
        }
    }
}
