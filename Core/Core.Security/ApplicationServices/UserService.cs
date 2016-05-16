using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using AFT.RegoV2.ApplicationServices.Security;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Security.ApplicationServices.Data.Users;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Core.Security.Events;
using AFT.RegoV2.Core.Security.Validators;
using AFT.RegoV2.Core.Services.Security;
using AFT.RegoV2.Domain.Security.Events;
using AFT.RegoV2.Domain.Security.Interfaces;
using AFT.RegoV2.Shared;
using AFT.RegoV2.Core.Security.Data;
using AFT.RegoV2.Core.Security.Helpers;
using AutoMapper;

namespace AFT.RegoV2.Core.Security.ApplicationServices
{
    public class UserService : MarshalByRefObject, IApplicationService
    {
        private readonly ISecurityRepository _repository;
        private readonly ISecurityProvider _securityProvider;
        private readonly IEventBus _eventBus;

        public UserService(
            ISecurityRepository repository,  
            ISecurityProvider securityProvider,
            IEventBus eventBus
            )
        {
            _repository = repository;
            _securityProvider = securityProvider;
            _eventBus = eventBus;
        }

        #region Queries

        [Permission(Permissions.View, Module = Modules.AdminManager)]
        public IQueryable<User> GetUsers()
        {
            return _repository.Users.Include(u => u.Role).AsQueryable();
        }

        public User GetUserById(Guid userId)
        {
            return _repository.Users
                .Include(u => u.Role)
                .Include(u => u.Role.Permissions)
                .Include(u => u.Licensees)
                .Include(u => u.AllowedBrands)
                .Include(u => u.BrandFilterSelections)
                .Include(u => u.Currencies)
                .SingleOrDefault(u => u.Id == userId);
        }

        public User GetUserByName(string username)
        {
            return _repository.Users
                .Include(u => u.Role)
                .Include(u => u.Licensees)
                .Include(u => u.AllowedBrands)
                .Include(u => u.Currencies)
                .SingleOrDefault(u => u.Username == username);
        }

        #endregion

        #region Commands

        [Permission(Permissions.Add, Module = Modules.AdminManager)]
        public User CreateUser(AddUserData data)
        {
            var validationResult = new AddUserValidator().Validate(data);

            if (!validationResult.IsValid)
                throw new RegoValidationException(validationResult);

            var role = _repository.Roles.SingleOrDefault(r => r.Id == (data.RoleId ?? new Guid("00000000-0000-0000-0000-000000000002")));

            var user = Mapper.DynamicMap<User>(data);

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                user.Id = Guid.NewGuid();

                user.PasswordEncrypted = PasswordHelper.EncryptPassword(user.Id, data.Password);

                user.Role = role;

                user.SetLicensees(data.AssignedLicensees);

                user.SetAllowedBrands(data.AllowedBrands);

                user.SetCurrencies(data.Currencies);

                if (data.AllowedBrands != null)
                {
                    foreach (var allowedBrand in data.AllowedBrands)
                    {
                        user.BrandFilterSelections.Add(new BrandFilterSelection
                        {
                            UserId = user.Id,
                            BrandId = allowedBrand,
                            User = user
                        });
                    }   
                }

                _repository.Users.Add(user);
                _repository.SaveChanges();

                _eventBus.Publish(new UserCreated(user));

                scope.Complete();
            }

            return user;
        }

        [Permission(Permissions.Edit, Module = Modules.AdminManager)]
        public User UpdateUser(EditUserData data)
        {
            var validationResult = new EditUserValidator().Validate(data);

            if (!validationResult.IsValid)
                throw new RegoValidationException(validationResult);

            var user = GetUserById(data.Id);

            if (user == null)
            {
                throw new RegoException(String.Format("User with id: {0} not found", data.Id));
            }

            var role = _repository.Roles.SingleOrDefault(r => r.Id == data.RoleId);

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                user.Username = data.Username;
                user.FirstName = data.FirstName;
                user.LastName = data.LastName;
                user.Status = data.Status;
                user.Language = data.Language;
                user.Description = data.Description;

                if (role != null)
                {
                    user.Role = role;
                }

                user.SetLicensees(data.AssignedLicensees);

                user.SetAllowedBrands(data.AllowedBrands);

                user.SetCurrencies(data.Currencies);

                _repository.SaveChanges();

                _eventBus.Publish(new UserUpdated(user));

                scope.Complete();
            }

            return user;
        }

        [Permission(Permissions.Edit, Module = Modules.AdminManager)]
        public User ChangePassword(UserId userId, string password)
        {
            var user = GetUserById(userId);
            if (user != null)
            {
                user.PasswordEncrypted = PasswordHelper.EncryptPassword(user.Id, password);
                _repository.SaveChanges();
            }
            _eventBus.Publish(new UserPasswordChanged(user.Id));

            return user;
        }

        [Permission(Permissions.Activate, Module = Modules.AdminManager)]
        public void Activate(UserId userId)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                ChangeUserStatus(userId, UserStatus.Active);
                _eventBus.Publish(new UserActivated(userId));
                scope.Complete();
            }
        }

        [Permission(Permissions.Deactivate, Module = Modules.AdminManager)]
        public void Deactivate(UserId userId)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                ChangeUserStatus(userId, UserStatus.Inactive);
                _eventBus.Publish(new UserDeactivated(userId));
                scope.Complete();
            }
        }

        private void ChangeUserStatus(Guid userId, UserStatus status)
        {
            var user = _repository.Users.SingleOrDefault(u => u.Id == userId);

            if (user == null)
                throw new RegoException(string.Format("User with id: {0} not found", userId));

            user.Status = status;

            _repository.SaveChanges();
        }

        #endregion

        public bool ValidateLogin(string username, string password)
        {
            var user = _repository.Users.SingleOrDefault(x => x.Username == username);
            if (user == null)
                return false;

            var isValid = user.Status == UserStatus.Active && user.PasswordEncrypted == PasswordHelper.EncryptPassword(user.Id, password);

            return isValid;
        }

        public void SignInUser(User user)
        {
            _securityProvider.Session.SetUser(user);
        }

        public void SignInDefaultUser()
        {
            SignInUser(new User());
        }

        public void SignOutUser()
        {
            _securityProvider.Session.ClearUser();
        }

        public void SetLicenseeFilterSelections(Guid userId, IEnumerable<Guid> selectedLicensees)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var user = _repository.Users
                    .Include(x => x.LicenseeFilterSelections)
                    .Single(x => x.Id == userId);

                user.LicenseeFilterSelections.Clear();

                if (selectedLicensees != null)
                {
                    selectedLicensees.ForEach(x => user.LicenseeFilterSelections.Add(new LicenseeFilterSelection
                    {
                        UserId = user.Id,
                        LicenseeId = x,
                        User = user
                    }));
                }

                _repository.SaveChanges();

                scope.Complete();
            }
        }
        
        public void SetBrandFilterSelections(Guid userId, IEnumerable<Guid> selectedBrands)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var user = _repository.Users
                    .Include(x => x.BrandFilterSelections)
                    .Single(x => x.Id == userId);

                user.BrandFilterSelections.Clear();

                if (selectedBrands != null)
                {
                    selectedBrands.ForEach(x => user.BrandFilterSelections.Add(new BrandFilterSelection
                    {
                        UserId = user.Id,
                        BrandId = x,
                        User = user
                    }));
                }

                _repository.SaveChanges();

                scope.Complete();
            }
        }

        public IEnumerable<Guid> GetLicenseeFilterSelections(Guid userId)
        {
            return _repository.Users
                .Include(x => x.LicenseeFilterSelections)
                .Single(x => x.Id == userId)
                .LicenseeFilterSelections
                .Select(x => x.LicenseeId);
        }

        public IEnumerable<Guid> GetBrandFilterSelections(Guid userId)
        {
            return _repository.Users
                .Include(x => x.BrandFilterSelections)
                .Single(x => x.Id == userId)
                .BrandFilterSelections
                .Select(x => x.BrandId);
        }
    }
}
