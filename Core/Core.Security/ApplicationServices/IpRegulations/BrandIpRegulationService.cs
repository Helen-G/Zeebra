using System;
using System.Collections.Generic;
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
    public class BrandIpRegulationService : IpRegulationServiceBase
    {
        private readonly IEventBus _eventBus;

        public BrandIpRegulationService(
            ISecurityRepository repository, 
            ISecurityProvider securityProvider,
            IEventBus eventBus
            )
            :base(repository, securityProvider)
        {
            _eventBus = eventBus;
        }

        #region Queries

        [Permission(Permissions.View, Module = Modules.BrandIpRegulationManager)]
        public IQueryable<BrandIpRegulation> GetIpRegulations()
        {
            return _repository.BrandIpRegulations.Include(ip => ip.CreatedBy).Include(ip => ip.UpdatedBy).AsQueryable();
        }

        public BrandIpRegulation GetIpRegulation(Guid id)
        {
            return GetIpRegulations().SingleOrDefault(ip => ip.Id == id);
        }

        #endregion

        [Permission(Permissions.Add, Module = Modules.BrandIpRegulationManager)]
        public BrandIpRegulation CreateIpRegulation(AddBrandIpRegulationData data)
        {
            var regulation = Mapper.DynamicMap<BrandIpRegulation>(data);

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                regulation.Id = Guid.NewGuid();
                regulation.CreatedBy = _repository.Users.SingleOrDefault(u => u.Id == _securityProvider.User.UserId);
                regulation.CreatedDate = DateTime.Now;
                regulation.Id = Guid.NewGuid();
                _repository.BrandIpRegulations.Add(regulation);
                _repository.SaveChanges();

                _eventBus.Publish(new BrandIpRegulationCreated(regulation));
                scope.Complete();
            }

            return regulation;
        }

        [Permission(Permissions.Edit, Module = Modules.BrandIpRegulationManager)]
        public BrandIpRegulation UpdateIpRegulation(EditBrandIpRegulationData data)
        {
            var regulation = _repository.BrandIpRegulations.SingleOrDefault(ip => ip.Id == data.Id);

            if (regulation == null)
            {
                throw new RegoException("User does not exist");
            }

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                regulation.LicenseeId = data.LicenseeId;
                regulation.BrandId = data.BrandId;
                regulation.IpAddress = data.IpAddress;
                regulation.Description = data.Description;
                regulation.BlockingType = data.BlockingType;
                regulation.RedirectionUrl = data.RedirectionUrl;

                regulation.UpdatedBy = _repository.Users.SingleOrDefault(u => u.Id == _securityProvider.User.UserId);
                regulation.UpdatedDate = DateTime.Now;

                _repository.SaveChanges();

                _eventBus.Publish(new BrandIpRegulationUpdated(regulation));
                scope.Complete();
            }

            return regulation;
        }

        [Permission(Permissions.Delete, Module = Modules.BrandIpRegulationManager)]
        public void DeleteIpRegulation(BrandIpRegulationId id)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var ipRegulation = GetIpRegulation(id);
                _repository.BrandIpRegulations.Remove(ipRegulation);

                _repository.SaveChanges();

                _eventBus.Publish(new BrandIpRegulationDeleted(ipRegulation));
                scope.Complete();
            }
        }

        public bool IsIpAddressUnique(string address)
        {
            return !_repository
                .BrandIpRegulations.ToList()
                .Any(ip => IsRangesIntersects(ip.IpAddress, address));
        }

        public VerifyIpResult VerifyIpAddress(string ipAddress, Guid? brandId = null)
        {
            var result = new VerifyIpResult();

            if (IsLocalhost(ipAddress))
            {
                result.Allowed = true;
                return result;
            }

            var ipRegulation =
                    _repository.BrandIpRegulations.ToList().FirstOrDefault(
                    ip =>
                        IsRangesIntersects(ip.IpAddress, ipAddress));

            result.Allowed = !(ipRegulation != null && (!brandId.HasValue || ipRegulation.BrandId == brandId.Value));

            if (result.Allowed) return result;

            result.BlockingType = ipRegulation.BlockingType;
            result.RedirectionUrl = ipRegulation.RedirectionUrl;

            return result;
        }
    }
}
