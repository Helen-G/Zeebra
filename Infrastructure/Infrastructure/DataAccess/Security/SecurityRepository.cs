using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using AFT.RegoV2.ApplicationServices.Security;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Core.Security.Data;
using AFT.RegoV2.Core.Security.Helpers;
using AFT.RegoV2.Core.Services.Security.Repository.Mappings;
using AFT.RegoV2.Domain.BoundedContexts.Security.Data;
using AFT.RegoV2.Domain.Security.Interfaces;
using AFT.RegoV2.Infrastructure.Constants;
using AFT.RegoV2.Infrastructure.DataAccess.Brand;
using AFT.RegoV2.Infrastructure.DataAccess.Security.Mappings;
using AFT.RegoV2.Infrastructure.DependencyResolution;
using AFT.RegoV2.Shared;
using AFT.RegoV2.Shared.Constants;
using Microsoft.Practices.Unity;
using ServiceStack.Common.Extensions;
using IpRegulationConstants = AFT.RegoV2.Core.Security.Common.IpRegulationConstants;

namespace AFT.RegoV2.Core.Services.Security
{
    public class SecurityRepository : DbContext, ISecurityRepository, ISeedable
    {
        private const string Schema = "security";

        static SecurityRepository()
        {
            Database.SetInitializer(new RepositoryInitializer());
        }

        public SecurityRepository()
            : base("name=Default")
        {
        }

        public IDbSet<Role> Roles { get; set; }
        public IDbSet<User> Users { get; set; }
        public IDbSet<AdminIpRegulation> AdminIpRegulations { get; set; }
        public IDbSet<AdminIpRegulationSetting> AdminIpRegulationSettings { get; set; }
        public IDbSet<BrandIpRegulation> BrandIpRegulations { get; set; }
        public IDbSet<RolePermission> RolePermissions { get; set; }
        public IDbSet<Error> Errors { get; set; }
        public IDbSet<Session> Sessions { get; set; }
        public IDbSet<Permission> Permissions { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            //modelBuilder.HasDefaultSchema(Schema);
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
            modelBuilder.Conventions.Remove<ManyToManyCascadeDeleteConvention>();
            modelBuilder.Configurations.Add(new RolePermissionMap(Schema));
            modelBuilder.Configurations.Add(new RoleMap(Schema));
            modelBuilder.Configurations.Add(new UserMap(Schema));
            modelBuilder.Configurations.Add(new BrandIdMap(Schema));
            modelBuilder.Configurations.Add(new RoleLicenseeIdMap(Schema));
            modelBuilder.Configurations.Add(new UserLicenseeIdMap(Schema));
            modelBuilder.Configurations.Add(new CurrencyCodeMap(Schema));
            modelBuilder.Configurations.Add(new RoleBrandIdMap(Schema));
            modelBuilder.Configurations.Add(new ErrorMap(Schema));
            modelBuilder.Configurations.Add(new BrandFilterSelectionMap(Schema));
            modelBuilder.Configurations.Add(new LicenseeFilterSelectionMap(Schema));
            modelBuilder.Entity<AdminIpRegulation>().ToTable("AdminIpRegulation", Schema);
            modelBuilder.Entity<BrandIpRegulation>().ToTable("BrandIpRegulation", Schema);
            modelBuilder.Entity<AdminIpRegulationSetting>().ToTable("AdminIpRegulationSettings", Schema);
            modelBuilder.Entity<Session>().ToTable("Session", Schema);
            modelBuilder.Entity<Permission>().ToTable("Permission", Schema);
        }

        public User GetUserById(Guid userId)
        {
            return Users
                .Include(u => u.Role)
                .Include(u => u.Licensees)
                .Include(u => u.AllowedBrands)
                .Include(u => u.BrandFilterSelections)
                .Include(u => u.Currencies)
                .SingleOrDefault(u => u.Id == userId);
        }

        public override int SaveChanges()
        {
            try
            {
                return base.SaveChanges();
            }
            catch (DbEntityValidationException e)
            {
                var dbValidationErrorMessages = e.EntityValidationErrors.ToArray();
                Trace.WriteLine(dbValidationErrorMessages.ToString());
                throw;
            }
        }

        public void Seed()
        {
            var service = new SecuritySeed(this);
            var brandRepository = new BrandRepository();
            var licenseeId = brandRepository.Licensees.Single(l => l.Name == "Flycow").Id;
            var brands = brandRepository.Brands.Where(b => (b.Name == "138" || b.Name == "831")).Select(b => b.Id);
            var licenseeIds = new[] { licenseeId };

            service.PopulatePermissions();
            service.RegisterPermissions();

            service.CreateSuperAdmin(licenseeIds, brands);
            service.CreateCustomerServiceOfficerRole(licenseeIds, brands);
            service.CreateMarketingOfficerRole(licenseeIds);
            service.CreatePaymentOfficer(licenseeIds, brands);
            service.CreateDefaultRole(licenseeIds);
            service.CreateLicenseeRole(licenseeIds);
            service.CreateFraudOfficerRole(licenseeIds);
            service.CreateKYCOfficerRole(licenseeIds);

            service.SeedIpRegulationSettings();

            service.SeedAdminIpRegulations();
            service.SeedBrandIpRegulations(licenseeId, brands.First());
        }
    }

    public class SecuritySeed
    {
        internal const string SuperAdminUsername = "SuperAdmin";
        internal const string SuperAdminRolename = "SuperAdmin";
        internal const string SuperAdminPassword = "SuperAdmin";
        internal const string DefaultRolename = "Default";

        private readonly ISecurityRepository _repository;

        public SecuritySeed(ISecurityRepository repository)
        {
            _repository = repository;
        }

        private static IEnumerable<Permission> GetAllPermissions()
        {
            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(x => x.FullName.StartsWith("AFT.RegoV2.") 
                    && !x.FullName.StartsWith("AFT.RegoV2.Infrastructure"));

            return loadedAssemblies.SelectMany(
                x => x.GetLoadableTypes())
                .Where(t => t.IsDescendentOf(typeof(IApplicationService)))
                .SelectMany(
                    service => service.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                        .Select(method => method.GetCustomAttributes(typeof(PermissionAttribute), true))
                        .SelectMany(operationAttrs => operationAttrs.Select(o => (PermissionAttribute)o)
                        .Distinct()
                        .Select(operationAttr => new Permission
                {
                    Id = Guid.NewGuid(),
                    Name = operationAttr.Permission,
                    Module = operationAttr.Module
                })));
        }

        public void PopulatePermissions()
        {
            var permissions = GetAllPermissions();

            foreach (var permission in permissions)
            {
                if (!_repository.Permissions.Local.Any(p => p.Name == permission.Name && p.Module == permission.Module))
                {
                    _repository.Permissions.Add(permission);   
                }
            }

            _repository.SaveChanges();
        }

        internal void CreateSuperAdmin(IEnumerable<Guid> licenseeIds, IEnumerable<Guid> brandIds)
        {
            var userId = RoleIds.SuperAdminId;

            var permissions = new List<RolePermission>();
            foreach (var permission in _repository.Permissions)
            {
                permissions.Add(new RolePermission
                {
                    RoleId = RoleIds.SuperAdminId,
                    PermissionId = permission.Id
                });
            }

            var role = new Role
            {
                Id = RoleIds.SuperAdminId,
                IsSuperAdmin = true,
                Code = SuperAdminRolename,
                Name = SuperAdminRolename,
                CreatedDate = DateTime.UtcNow,
                Permissions = permissions
            };

            role.SetLicensees(licenseeIds);

            var user = new User
            {
                Id = userId,
                Username = SuperAdminUsername,
                FirstName = SuperAdminUsername,
                LastName = SuperAdminUsername,
                Status = UserStatus.Active,
                Description = SuperAdminUsername,
                PasswordEncrypted = PasswordHelper.EncryptPassword(userId, SuperAdminPassword),
                Role = role
            };

            user.SetLicensees(licenseeIds);

            foreach (var licenseeId in licenseeIds)
            {
                user.LicenseeFilterSelections.Add(new LicenseeFilterSelection
                {
                    UserId = user.Id,
                    LicenseeId = licenseeId,
                    User = user
                });
            }

            user.SetAllowedBrands(brandIds);

            foreach (var brandId in brandIds)
            {
                user.BrandFilterSelections.Add(new BrandFilterSelection
                {
                    UserId = user.Id,
                    BrandId = brandId,
                    User = user
                });
            }

            _repository.Users.AddOrUpdate(user);

            _repository.SaveChanges();
        }

        internal RolePermission CreateRolePermission(Guid roleId, string name, string module)
        {
            var permission = _repository.Permissions.First(p => p.Name == name && p.Module == module);
            return new RolePermission
            {
                PermissionId = permission.Id,
                RoleId = roleId
            };
        }

        internal void CreatePaymentOfficer(IEnumerable<Guid> licenseeIds, IEnumerable<Guid> brandIds)
        {
            var roleId = RoleIds.PaymentOfficerId;
            const string paymentOfficer = "PaymentOfficer";
            var role = new Role
            {
                Id = roleId,
                Code = "Payment",
                Name = paymentOfficer,
                CreatedDate = DateTime.UtcNow,
                IsSuperAdmin = true,
                Permissions = new List<RolePermission>
                    {
                        CreateRolePermission(roleId, Permissions.View, Modules.OfflineDepositRequests),
                        CreateRolePermission(roleId, Permissions.Add, Modules.OfflineDepositRequests),
                        CreateRolePermission(roleId, Permissions.View, Modules.OfflineDepositConfirmation),
                        CreateRolePermission(roleId, Permissions.Confirm, Modules.OfflineDepositConfirmation),
                        CreateRolePermission(roleId, Permissions.View, Modules.OfflineDepositVerification),
                        CreateRolePermission(roleId, Permissions.Verify, Modules.OfflineDepositVerification),
                        CreateRolePermission(roleId, Permissions.Unverify, Modules.OfflineDepositVerification),
                        CreateRolePermission(roleId, Permissions.Approve, Modules.OfflineDepositApproval),
                        CreateRolePermission(roleId, Permissions.Reject, Modules.OfflineDepositApproval),
                        CreateRolePermission(roleId, Permissions.View, Modules.OfflineWithdrawalRequest),
                        CreateRolePermission(roleId, Permissions.Add, Modules.OfflineWithdrawalRequest),
                        CreateRolePermission(roleId, Permissions.View, Modules.OfflineWithdrawalVerification),
                        CreateRolePermission(roleId, Permissions.Verify, Modules.OfflineWithdrawalVerification),
                        CreateRolePermission(roleId, Permissions.Unverify, Modules.OfflineWithdrawalVerification),
                        CreateRolePermission(roleId, Permissions.Approve, Modules.OfflineWithdrawalApproval),
                        CreateRolePermission(roleId, Permissions.Reject, Modules.OfflineWithdrawalApproval),
                        CreateRolePermission(roleId, Permissions.View, Modules.OfflineWithdrawalOnHold),

                        CreateRolePermission(roleId, Permissions.Search, Modules.PlayerManager),
                        CreateRolePermission(roleId, Permissions.View, Modules.PlayerManager),

                        CreateRolePermission(roleId, Permissions.View, Modules.PlayerReport),
                        CreateRolePermission(roleId, Permissions.Export, Modules.PlayerReport),
                        CreateRolePermission(roleId, Permissions.View, Modules.PlayerBetHistoryReport),
                        CreateRolePermission(roleId, Permissions.Export, Modules.PlayerBetHistoryReport),

                        CreateRolePermission(roleId, Permissions.View, Modules.PlayerBankAccount),
                        CreateRolePermission(roleId, Permissions.Add, Modules.PlayerBankAccount),
                        CreateRolePermission(roleId, Permissions.Edit, Modules.PlayerBankAccount)
                    }
            };

            role.SetLicensees(licenseeIds);

            _repository.Roles.AddOrUpdate(role);

            _repository.SaveChanges();
        }

        internal void CreateDefaultRole(IEnumerable<Guid> licenseeIds)
        {
            var role = new Role
            {
                Id = RoleIds.DefaultId,
                Name = DefaultRolename,
                CreatedDate = DateTime.UtcNow
            };

            role.SetLicensees(licenseeIds);

            _repository.Roles.AddOrUpdate(role);

            _repository.SaveChanges();
        }

        internal void CreateMarketingOfficerRole(IEnumerable<Guid> licenseeIds)
        {
            var roleId = RoleIds.MarketingOfficerId;
            var role = new Role
            {
                Id = roleId,
                Code = "Marketing",
                Name = "MarketingOfficer",
                CreatedDate = DateTime.UtcNow,
                Permissions = new List<RolePermission>
                    {
                        CreateRolePermission(roleId, Permissions.Add, Modules.BonusManager),
                        CreateRolePermission(roleId, Permissions.Edit, Modules.BonusManager),
                        CreateRolePermission(roleId, Permissions.View, Modules.BonusManager),
                        CreateRolePermission(roleId, Permissions.Activate, Modules.BonusManager),
                        CreateRolePermission(roleId, Permissions.Deactivate, Modules.BonusManager),
                        CreateRolePermission(roleId, Permissions.Add, Modules.BonusTemplateManager),
                        CreateRolePermission(roleId, Permissions.Edit, Modules.BonusTemplateManager),
                        CreateRolePermission(roleId, Permissions.View, Modules.BonusTemplateManager),
                        CreateRolePermission(roleId, Permissions.Delete, Modules.BonusTemplateManager),
                        CreateRolePermission(roleId, Permissions.View, Modules.PlayerManager),
                        CreateRolePermission(roleId, Permissions.Search, Modules.PlayerManager),

                        CreateRolePermission(roleId, Permissions.View, Modules.PlayerReport),
                        CreateRolePermission(roleId, Permissions.Export, Modules.PlayerReport),
                        CreateRolePermission(roleId, Permissions.View, Modules.PlayerBetHistoryReport),
                        CreateRolePermission(roleId, Permissions.Export, Modules.PlayerBetHistoryReport)
                    }
            };

            role.SetLicensees(licenseeIds);

            _repository.Roles.AddOrUpdate(role);

            _repository.SaveChanges();
        }

        internal void CreateCustomerServiceOfficerRole(IEnumerable<Guid> licenseeIds, IEnumerable<Guid> brandIds)
        {
            var roleId = RoleIds.CSOfficerId;
            var role = new Role
            {
                Id = roleId,
                Code = "CSOfficer",
                Name = "CSOfficer",
                CreatedDate = DateTime.UtcNow,
                Permissions = new List<RolePermission>
                    {
                        CreateRolePermission(roleId, Permissions.View, Modules.BonusManager),
                        CreateRolePermission(roleId, Permissions.View, Modules.BonusTemplateManager),
                        CreateRolePermission(roleId, Permissions.View, Modules.OfflineDepositRequests),
                        CreateRolePermission(roleId, Permissions.Add, Modules.OfflineDepositRequests),
                        CreateRolePermission(roleId, Permissions.View, Modules.OfflineWithdrawalRequest),
                        CreateRolePermission(roleId, Permissions.Add, Modules.OfflineWithdrawalRequest),

                        CreateRolePermission(roleId, Permissions.View, Modules.PlayerManager),
                        CreateRolePermission(roleId, Permissions.Search, Modules.PlayerManager),
                        CreateRolePermission(roleId, Permissions.Add, Modules.PlayerManager),
                        CreateRolePermission(roleId, Permissions.Edit, Modules.PlayerManager),
                        CreateRolePermission(roleId, Permissions.Activate, Modules.PlayerManager),
                        CreateRolePermission(roleId, Permissions.Deactivate, Modules.PlayerManager),
                        CreateRolePermission(roleId, Permissions.AssignVipLevel, Modules.PlayerManager),

                        CreateRolePermission(roleId, Permissions.View, Modules.PlayerReport),
                        CreateRolePermission(roleId, Permissions.Export, Modules.PlayerReport),
                        CreateRolePermission(roleId, Permissions.View, Modules.PlayerBetHistoryReport),
                        CreateRolePermission(roleId, Permissions.Export, Modules.PlayerBetHistoryReport),

                        CreateRolePermission(roleId, Permissions.View, Modules.PlayerBankAccount),
                        CreateRolePermission(roleId, Permissions.Add, Modules.PlayerBankAccount),
                        CreateRolePermission(roleId, Permissions.Edit, Modules.PlayerBankAccount)
                    }
            };

            role.SetLicensees(licenseeIds);

            _repository.Roles.AddOrUpdate(role);

            _repository.SaveChanges();
        }

        internal void CreateLicenseeRole(IEnumerable<Guid> licenseeIds)
        {
            var roleId = RoleIds.LicenseeId;

            var role = new Role
            {
                Id = roleId,
                Code = "Licensee",
                Name = "Licensee",
                CreatedDate = DateTime.UtcNow,
                Permissions = new List<RolePermission>
                    {
                        CreateRolePermission(roleId, Permissions.View, Modules.LanguageManager),
                        CreateRolePermission(roleId, Permissions.Activate, Modules.LanguageManager),
                        CreateRolePermission(roleId, Permissions.Deactivate, Modules.LanguageManager),
                        CreateRolePermission(roleId, Permissions.View, Modules.OfflineDepositRequests),
                        CreateRolePermission(roleId, Permissions.Add, Modules.OfflineDepositRequests),
                        
                        CreateRolePermission(roleId, Permissions.View, Modules.OfflineDepositConfirmation),
                        CreateRolePermission(roleId, Permissions.Confirm, Modules.OfflineDepositConfirmation),
                        CreateRolePermission(roleId, Permissions.View, Modules.OfflineDepositVerification),
                        CreateRolePermission(roleId, Permissions.Verify, Modules.OfflineDepositVerification),
                        CreateRolePermission(roleId, Permissions.Unverify, Modules.OfflineDepositVerification),
                        CreateRolePermission(roleId, Permissions.Approve, Modules.OfflineDepositApproval),
                        CreateRolePermission(roleId, Permissions.Reject, Modules.OfflineDepositApproval),

                        CreateRolePermission(roleId, Permissions.View, Modules.FraudManager),
                        CreateRolePermission(roleId, Permissions.View, Modules.WagerConfiguration),
                        CreateRolePermission(roleId, Permissions.View, Modules.AutoVerificationConfiguration),
                        
                        CreateRolePermission(roleId, Permissions.View, Modules.OfflineWithdrawalRequest),
                        CreateRolePermission(roleId, Permissions.Add, Modules.OfflineWithdrawalRequest),
                        CreateRolePermission(roleId, Permissions.Exempt, Modules.OfflineWithdrawalExemption),
                        CreateRolePermission(roleId, Permissions.Pass, Modules.OfflineWithdrawalWagerCheck),
                        CreateRolePermission(roleId, Permissions.Fail, Modules.OfflineWithdrawalWagerCheck),
                        CreateRolePermission(roleId, Permissions.View, Modules.OfflineWithdrawalVerification),
                        CreateRolePermission(roleId, Permissions.Verify, Modules.OfflineWithdrawalVerification),
                        CreateRolePermission(roleId, Permissions.Unverify, Modules.OfflineWithdrawalVerification),
                        CreateRolePermission(roleId, Permissions.Approve, Modules.OfflineWithdrawalApproval),
                        CreateRolePermission(roleId, Permissions.Reject, Modules.OfflineWithdrawalApproval),
                        CreateRolePermission(roleId, Permissions.View, Modules.OfflineWithdrawalOnHold),
                        CreateRolePermission(roleId, Permissions.Pass, Modules.OfflineWithdrawalInvestigation),
                        CreateRolePermission(roleId, Permissions.Fail, Modules.OfflineWithdrawalInvestigation),

                        CreateRolePermission(roleId, Permissions.Add, Modules.PlayerManager),
                        CreateRolePermission(roleId, Permissions.Edit, Modules.PlayerManager),
                        CreateRolePermission(roleId, Permissions.View, Modules.PlayerManager),
                        CreateRolePermission(roleId, Permissions.Activate, Modules.PlayerManager),
                        CreateRolePermission(roleId, Permissions.Deactivate, Modules.PlayerManager),
                        CreateRolePermission(roleId, Permissions.AssignVipLevel, Modules.PlayerManager),

                        CreateRolePermission(roleId, Permissions.View, Modules.PlayerReport),
                        CreateRolePermission(roleId, Permissions.Export, Modules.PlayerReport),
                        CreateRolePermission(roleId, Permissions.View, Modules.PlayerBetHistoryReport),
                        CreateRolePermission(roleId, Permissions.Export, Modules.PlayerBetHistoryReport),

                        CreateRolePermission(roleId, Permissions.Add, Modules.BrandManager),
                        CreateRolePermission(roleId, Permissions.Edit, Modules.BrandManager),
                        CreateRolePermission(roleId, Permissions.View, Modules.BrandManager),
                        CreateRolePermission(roleId, Permissions.Activate, Modules.BrandManager),
                        CreateRolePermission(roleId, Permissions.Deactivate, Modules.BrandManager),

                        CreateRolePermission(roleId, Permissions.Add, Modules.BonusManager),
                        CreateRolePermission(roleId, Permissions.Edit, Modules.BonusManager),
                        CreateRolePermission(roleId, Permissions.View, Modules.BonusManager),
                        CreateRolePermission(roleId, Permissions.Activate, Modules.BonusManager),
                        CreateRolePermission(roleId, Permissions.Deactivate, Modules.BonusManager),
                        CreateRolePermission(roleId, Permissions.Add, Modules.BonusTemplateManager),
                        CreateRolePermission(roleId, Permissions.Edit, Modules.BonusTemplateManager),
                        CreateRolePermission(roleId, Permissions.View, Modules.BonusTemplateManager),
                        CreateRolePermission(roleId, Permissions.Delete, Modules.BonusTemplateManager),

                        CreateRolePermission(roleId, Permissions.View, Modules.VipLevelManager),
                        CreateRolePermission(roleId, Permissions.Add, Modules.VipLevelManager),
                        CreateRolePermission(roleId, Permissions.Edit, Modules.VipLevelManager),
                        CreateRolePermission(roleId, Permissions.Activate, Modules.VipLevelManager),
                        CreateRolePermission(roleId, Permissions.Deactivate, Modules.VipLevelManager),

                        CreateRolePermission(roleId, Permissions.View, Modules.PlayerBankAccount),
                        CreateRolePermission(roleId, Permissions.Add, Modules.PlayerBankAccount),
                        CreateRolePermission(roleId, Permissions.Edit, Modules.PlayerBankAccount)
                    }
            };

            role.SetLicensees(licenseeIds);

            _repository.Roles.AddOrUpdate(role);

            _repository.SaveChanges();
        }

        internal void CreateFraudOfficerRole(IEnumerable<Guid> licenseeIds)
        {
            var customerSeviceOfficerRoleId = RoleIds.FraudOfficerId;

            var role = new Role
            {
                Id = customerSeviceOfficerRoleId,
                Code = "FraudOfficer",
                Name = "FraudOfficer",
                CreatedDate = DateTime.UtcNow,
                Permissions = new List<RolePermission>
                {
                    CreateRolePermission(customerSeviceOfficerRoleId, Permissions.View, Modules.OfflineDepositRequests),
                    CreateRolePermission(customerSeviceOfficerRoleId, Permissions.View, Modules.OfflineDepositConfirmation),
                    CreateRolePermission(customerSeviceOfficerRoleId, Permissions.View, Modules.OfflineDepositVerification),
                    CreateRolePermission(customerSeviceOfficerRoleId, Permissions.View, Modules.OfflineWithdrawalRequest),
                    CreateRolePermission(customerSeviceOfficerRoleId, Permissions.Exempt, Modules.OfflineWithdrawalExemption),
                    CreateRolePermission(customerSeviceOfficerRoleId, Permissions.Pass, Modules.OfflineWithdrawalWagerCheck),
                    CreateRolePermission(customerSeviceOfficerRoleId, Permissions.Fail, Modules.OfflineWithdrawalWagerCheck),
                    CreateRolePermission(customerSeviceOfficerRoleId, Permissions.View, Modules.OfflineWithdrawalVerification),
                    CreateRolePermission(customerSeviceOfficerRoleId, Permissions.Verify, Modules.OfflineWithdrawalVerification),
                    CreateRolePermission(customerSeviceOfficerRoleId, Permissions.Unverify, Modules.OfflineWithdrawalVerification),
                    CreateRolePermission(customerSeviceOfficerRoleId, Permissions.Reject, Modules.OfflineWithdrawalVerification),
                    CreateRolePermission(customerSeviceOfficerRoleId, Permissions.View, Modules.OfflineWithdrawalOnHold),
                    CreateRolePermission(customerSeviceOfficerRoleId, Permissions.Pass, Modules.OfflineWithdrawalInvestigation),
                    CreateRolePermission(customerSeviceOfficerRoleId, Permissions.Fail, Modules.OfflineWithdrawalInvestigation),
                    
                    CreateRolePermission(customerSeviceOfficerRoleId, Permissions.View, Modules.FraudManager),
                    CreateRolePermission(customerSeviceOfficerRoleId, Permissions.View, Modules.WagerConfiguration),
                    CreateRolePermission(customerSeviceOfficerRoleId, Permissions.View, Modules.AutoVerificationConfiguration),

                    CreateRolePermission(customerSeviceOfficerRoleId, Permissions.View, Modules.PlayerManager),
                    CreateRolePermission(customerSeviceOfficerRoleId, Permissions.Search, Modules.PlayerManager),
                    CreateRolePermission(customerSeviceOfficerRoleId, Permissions.Activate, Modules.PlayerManager),
                    CreateRolePermission(customerSeviceOfficerRoleId, Permissions.Deactivate, Modules.PlayerManager),

                    CreateRolePermission(customerSeviceOfficerRoleId, Permissions.View, Modules.PlayerReport),
                    CreateRolePermission(customerSeviceOfficerRoleId, Permissions.Export, Modules.PlayerReport),
                    CreateRolePermission(customerSeviceOfficerRoleId, Permissions.View, Modules.PlayerBetHistoryReport),
                    CreateRolePermission(customerSeviceOfficerRoleId, Permissions.Export, Modules.PlayerBetHistoryReport),

                    CreateRolePermission(customerSeviceOfficerRoleId, Permissions.View, Modules.PlayerBankAccount),
                    CreateRolePermission(customerSeviceOfficerRoleId, Permissions.Add, Modules.PlayerBankAccount),
                    CreateRolePermission(customerSeviceOfficerRoleId, Permissions.Edit, Modules.PlayerBankAccount)
                }
            };

            role.SetLicensees(licenseeIds);

            _repository.Roles.AddOrUpdate(role);

            _repository.SaveChanges();
        }

        internal void CreateKYCOfficerRole(IEnumerable<Guid> licenseeIds)
        {
            var KYCSeviceOfficerRoleId = RoleIds.KYCOfficerId;
            var role = new Role
            {
                Id = KYCSeviceOfficerRoleId,
                Code = "KYCOfficer",
                Name = "KYCOfficer",
                CreatedDate = DateTime.UtcNow,
                Permissions = new List<RolePermission>
                    {
                        CreateRolePermission(KYCSeviceOfficerRoleId, Permissions.Search, Modules.PlayerManager),
                        CreateRolePermission(KYCSeviceOfficerRoleId, Permissions.View, Modules.PlayerManager),

                        CreateRolePermission(KYCSeviceOfficerRoleId, Permissions.View, Modules.PlayerReport),
                        CreateRolePermission(KYCSeviceOfficerRoleId, Permissions.Export, Modules.PlayerReport),
                        CreateRolePermission(KYCSeviceOfficerRoleId, Permissions.View, Modules.PlayerBetHistoryReport),
                        CreateRolePermission(KYCSeviceOfficerRoleId, Permissions.Export, Modules.PlayerBetHistoryReport),

                        CreateRolePermission(KYCSeviceOfficerRoleId, Permissions.View, Modules.PlayerBankAccount),
                        CreateRolePermission(KYCSeviceOfficerRoleId, Permissions.Add, Modules.PlayerBankAccount),
                        CreateRolePermission(KYCSeviceOfficerRoleId, Permissions.Edit, Modules.PlayerBankAccount),
                        CreateRolePermission(KYCSeviceOfficerRoleId, Permissions.Verify, Modules.PlayerBankAccount),
                        CreateRolePermission(KYCSeviceOfficerRoleId, Permissions.Reject, Modules.PlayerBankAccount)
                    }
            };

            role.SetLicensees(licenseeIds);

            _repository.Roles.AddOrUpdate(role);

            _repository.SaveChanges();
        }

        internal void SeedIpRegulationSettings()
        {
            const string disableIpRegulationName = "disable-ip-regulations";

            var regulation =
                _repository.AdminIpRegulationSettings.FirstOrDefault(a => a.Name == disableIpRegulationName);

            if (regulation == null)
            {
                _repository.AdminIpRegulationSettings.AddOrUpdate(new AdminIpRegulationSetting
                {
                    Id = Guid.NewGuid(),
                    Name = disableIpRegulationName,
                    Value = "true"
                });
            }

            _repository.SaveChanges();
        }

        internal void SeedBrandIpRegulations(Guid licenseeId, Guid brandId)
        {
            const string ipAddress = "192.168.1.1";
            const string redirectionUrl = "http://test.com";

            var brandIpRegulation = new BrandIpRegulation
            {
                Id = RoleIds.SuperAdminId,
                IpAddress = ipAddress,
                LicenseeId = licenseeId,
                BrandId = brandId,
                // Ip address is blocked with redirection to specified Url
                BlockingType = IpRegulationConstants.BlockingTypes.Redirection,
                RedirectionUrl = redirectionUrl,
                Description = "test",
                CreatedDate = DateTimeOffset.Now,
                CreatedBy = _repository.Users.SingleOrDefault(u => u.Id == RoleIds.SuperAdminId)
            };

            _repository.BrandIpRegulations.AddOrUpdate(brandIpRegulation);
            _repository.SaveChanges();
        }

        internal void SeedAdminIpRegulations()
        {
            const string ipAddress = "192.168.1.1";
            var adminIpRegulation = new AdminIpRegulation
            {
                Id = RoleIds.SuperAdminId,
                IpAddress = ipAddress,
                Description = "test",
                CreatedDate = DateTimeOffset.Now,
                CreatedBy = _repository.Users.SingleOrDefault(u => u.Id == RoleIds.SuperAdminId)
            };

            _repository.AdminIpRegulations.AddOrUpdate(adminIpRegulation);
            _repository.SaveChanges();
        }

        private void RegisterPermissionUnlessExists(string name, string module)
        {
            if (!_repository.Permissions.Any(p => p.Name == name && p.Module == module))
                _repository.Permissions.Add(new Permission
                {
                    Id = Guid.NewGuid(),
                    Name = name,
                    Module = module
                });
        }

        public void RegisterPermissions()
        {
            RegisterPermissionUnlessExists(Permissions.View, Modules.CurrencyManager);
            RegisterPermissionUnlessExists(Permissions.Edit, Modules.CurrencyManager);
            RegisterPermissionUnlessExists(Permissions.Add, Modules.CurrencyManager);
            RegisterPermissionUnlessExists(Permissions.Activate, Modules.CurrencyManager);
            RegisterPermissionUnlessExists(Permissions.Deactivate, Modules.CurrencyManager);

            RegisterPermissionUnlessExists(Permissions.View, Modules.BrandCurrencyManager);

            RegisterPermissionUnlessExists(Permissions.View, Modules.CurrencyExchangeManager);
            RegisterPermissionUnlessExists(Permissions.Edit, Modules.CurrencyExchangeManager);
            RegisterPermissionUnlessExists(Permissions.Add, Modules.CurrencyExchangeManager);

            RegisterPermissionUnlessExists(Permissions.Search, Modules.PlayerManager);

            RegisterPermissionUnlessExists(Permissions.Reject, Modules.OfflineWithdrawalVerification);

            _repository.SaveChanges();
        }
    }
}
