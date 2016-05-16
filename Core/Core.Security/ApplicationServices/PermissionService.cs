using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using AFT.RegoV2.ApplicationServices.Security;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Security.Data;
using AFT.RegoV2.Core.Security.Helpers;
using AFT.RegoV2.Core.Services.Security;
using AFT.RegoV2.Domain.Security.Events;
using AFT.RegoV2.Domain.Security.Interfaces;

namespace AFT.RegoV2.Core.Security.ApplicationServices
{
    public class PermissionService : MarshalByRefObject, IPermissionService
    {
        private readonly ISecurityRepository _securityRepository;
        private readonly IEventBus _eventBus;

        public PermissionService(
            ISecurityRepository securityRepository,
            IEventBus eventBus
            )
        {
            _securityRepository = securityRepository;
            _eventBus = eventBus;
        }

        public bool VerifyPermission(Guid userId, string permissionName, string module = null)
        {
            if (userId == Guid.Empty)
                return true;

            var user = _securityRepository.Users
                .Include(u => u.Role)
                .Include(u => u.Role.Permissions)
                .SingleOrDefault(u => u.Id == userId);

            if (user == null)
            {
                throw new SecurityException(string.Format("User with id: {0} not found", userId));
            }

            var permission =
                _securityRepository.Permissions.FirstOrDefault(p => p.Name == permissionName && p.Module == module);

            return user.Role.IsSuperAdmin || 
                (permission != null 
                && user.Role.Permissions.Any(p => p.PermissionId == permission.Id));
        }

        public void AddBrandToUser(Guid userId, Guid brandId)
        {
            var user = _securityRepository.GetUserById(userId);

            if (user == null)
            {
                throw new SecurityException(string.Format("User with id: {0} not found", userId));
            }

            user.AddAllowedBrand(brandId);

            _securityRepository.SaveChanges();
        }

        public IEnumerable<Permission> GetPermissions()
        {
            return _securityRepository.Permissions;
        }

        public Permission GetPermission(string permission, string module)
        {
            return _securityRepository.Permissions.FirstOrDefault(p => p.Name == permission && p.Module == module);
        }
    }
}