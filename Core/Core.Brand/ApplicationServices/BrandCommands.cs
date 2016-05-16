using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using AFT.RegoV2.ApplicationServices.Security;
using AFT.RegoV2.Core.Brand.ApplicationServices.Data;
using AFT.RegoV2.Core.Brand.Data;
using AFT.RegoV2.Core.Brand.Events;
using AFT.RegoV2.Core.Brand.Validators;
using AFT.RegoV2.Core.Common.Brand.Data;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Brand;
using AFT.RegoV2.Core.Common.Events.Brand;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Core.Security.Interfaces;
using AFT.RegoV2.Domain.BoundedContexts.Security.Helpers;
using AFT.RegoV2.Domain.Brand.Events;
using AFT.RegoV2.Domain.Security.Interfaces;
using AFT.RegoV2.Shared;
using FluentValidation;
using FluentValidation.Results;
using BrandLanguagesAssigned = AFT.RegoV2.Core.Common.Events.Brand.BrandLanguagesAssigned;
using VipLevel = AFT.RegoV2.Core.Brand.Data.VipLevel;

namespace AFT.RegoV2.Core.Brand.ApplicationServices
{
    public interface IBrandCommands : IApplicationService
    {
        Guid AddBrand(AddBrandData addBrandData);
        void EditBrand(EditBrandData editBrandData);
        ValidationResult ValidateThatBrandCanBeActivated(Guid brandId, string remarks);
        ValidationResult ValidateThatBrandCountryCanBeAssigned(AssignBrandCountryData data);
        void ActivateBrand(BrandId brandId, string remarks);
        void DeactivateBrand(BrandId brandId, string remarks);
        void AssignBrandCulture(AssignBrandCultureData assignBrandCultureData);
        void AssignBrandCountry(AssignBrandCountryData assignBrandCountryData);
        void AssignBrandCurrency(AssignBrandCurrencyData assignBrandCurrencyData);
        void AssignBrandProducts(AssignBrandProductsData assignBrandProductsData);
        void ActivateCulture(string code, string remarks);
        void DeactivateCulture(string code, string remarks);
        ValidationResult ValidateThatVipLevelCanBeAdded(VipLevelViewModel model);
        Guid AddVipLevel(VipLevelViewModel model);
        void SetDefaultVipLevel(Brand.Data.Brand brand, Guid vipLevelId);
        void EditVipLevel(VipLevelViewModel model);
        void CreateWalletStructureForBrand(WalletTemplateViewModel viewModel);
        void UpdateWalletStructureForBrand(WalletTemplateViewModel viewModel);
        void ActivateVipLevel(Guid vipLevelId, string remark);
        void DeactivateVipLevel(Guid deactivateVipLevelId, string remark, Guid? newDefaultVipLevelId);
        void CreateCountry(string code, string name);
        void UpdateCountry(string code, string name);
        void DeleteCountry(string code);
    }

    public class BrandCommands : MarshalByRefObject, IBrandCommands
    {
        private readonly IBrandRepository _repository;
        private readonly BrandQueries _queries;
        private readonly IGameQueries _gameQueries;
        private readonly IBasePaymentQueries _paymentQueries;
        private readonly IEventBus _eventBus;
        private readonly ISecurityProvider _securityProvider;
        private readonly IUserInfoProvider _userInfoProvider;
        private readonly IPermissionService _permissionService;

        public BrandCommands(
            IBrandRepository repository,
            BrandQueries queries,
            IGameQueries gameQueries,
            IBasePaymentQueries paymentQueries,
            IEventBus eventBus,
            ISecurityProvider securityProvider,
            IUserInfoProvider userInfoProvider,
            IPermissionService permissionService)
        {
            _repository = repository;
            _queries = queries;
            _gameQueries = gameQueries;
            _paymentQueries = paymentQueries;
            _eventBus = eventBus;
            _securityProvider = securityProvider;
            _userInfoProvider = userInfoProvider;
            _permissionService = permissionService;
        }

        [Permission(Permissions.Add, Module = Modules.BrandManager)]
        public Guid AddBrand(AddBrandData addBrandData)
        {
            var validationResult = new AddBrandValidator(_repository).Validate(addBrandData);

            if (!validationResult.IsValid)
                throw new RegoValidationException(validationResult);

            var brand = new Brand.Data.Brand
            {
                Id = Guid.NewGuid(),
                Licensee = _repository.Licensees.Single(x => x.Id == addBrandData.Licensee),
                Code = addBrandData.Code,
                Name = addBrandData.Name,
                Type = addBrandData.Type,
                TimezoneId = addBrandData.TimeZoneId,
                EnablePlayerPrefix = addBrandData.EnablePlayerPrefix,
                PlayerPrefix = addBrandData.PlayerPrefix,
                PlayerActivationMethod = addBrandData.PlayerActivationMethod,
                Status = BrandStatus.Inactive,
                CreatedBy = _userInfoProvider.User.Username,
                DateCreated = DateTimeOffset.UtcNow
            };

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                _repository.Brands.Add(brand);
                _repository.SaveChanges();

                _permissionService.AddBrandToUser(_userInfoProvider.User.UserId, brand.Id);
                _eventBus.Publish(new BrandRegistered
                {
                    Id = brand.Id,
                    Code = brand.Code,
                    Name = brand.Name,
                    LicenseeId = brand.Licensee.Id,
                    LicenseeName = brand.Licensee.Name,
                    TimeZoneId = brand.TimezoneId,
                    BrandType = brand.Type,
                    Status = brand.Status,
                    PlayerPrefix = brand.PlayerPrefix,
                    InternalAccountsNumber = brand.InternalAccountsNumber,
                    DateCreated = brand.DateCreated,
                    CreatedBy = brand.CreatedBy
                });
                scope.Complete();
            }

            return brand.Id;
        }

        [Permission(Permissions.Edit, Module = Modules.BrandManager)]
        public void EditBrand(EditBrandData editBrandData)
        {
            var validationResult = new EditBrandValidator(_repository).Validate(editBrandData);

            if (!validationResult.IsValid)
                throw new RegoValidationException(validationResult);

            var brand = _repository.Brands.Single(x => x.Id == editBrandData.Brand);

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                brand.Licensee = _repository.Licensees.Single(x => x.Id == editBrandData.Licensee);
                brand.Type = editBrandData.Type;
                brand.Name = editBrandData.Name;
                brand.Code = editBrandData.Code;
                brand.EnablePlayerPrefix = editBrandData.EnablePlayerPrefix;
                brand.PlayerPrefix = editBrandData.PlayerPrefix;
                brand.PlayerActivationMethod = editBrandData.PlayerActivationMethod;
                brand.InternalAccountsNumber = editBrandData.InternalAccounts;
                brand.TimezoneId = editBrandData.TimeZoneId;
                brand.Remarks = editBrandData.Remarks;
                brand.DateUpdated = DateTimeOffset.UtcNow;
                brand.UpdatedBy = _userInfoProvider.User.Username;

                _repository.SaveChanges();

                _eventBus.Publish(new BrandUpdated
                {
                    Id = brand.Id,
                    Code = brand.Code,
                    Name = brand.Name,
                    LicenseeId = brand.Licensee.Id,
                    LicenseeName = brand.Licensee.Name,
                    TypeName = brand.Type.ToString(),
                    Remarks = brand.Remarks,
                    PlayerPrefix = brand.PlayerPrefix,
                    TimeZoneId = brand.TimezoneId,
                    InternalAccountCount = brand.InternalAccountsNumber,
                    DateUpdated = brand.DateUpdated.Value,
                    UpdatedBy = brand.UpdatedBy
                });
                scope.Complete();
            }
        }

        public ValidationResult ValidateThatBrandCanBeActivated(Guid brandId, string remarks)
        {
            var brand = GetBrandForActivation(brandId);
            var activateBrandData = new ActivateBrandValidationData
            {
                Brand = brand,
                BrandPaymentLevels = _paymentQueries.GetPaymentLevels().Where(x => x.BrandId == brandId),
                BrandRiskLevels = _queries.GetRiskLevels(brandId),
                Remarks = remarks
            };
            var validator = new ActivateBrandValidator();
            return validator.Validate(activateBrandData);
        }

        public ValidationResult ValidateThatBrandCountryCanBeAssigned(AssignBrandCountryData data)
        {
            var validator = new AssignBrandCountryValidator(_repository);
            return validator.Validate(data);
        }

        public ValidationResult ValidateThatBrandCultureCanBeAssigned(AssignBrandCultureData data)
        {
            var validator = new AssignBrandCultureValidator(_repository);
            return validator.Validate(data);
        }

        public ValidationResult ValidateThatBrandProductsCanBeAssigned(AssignBrandProductsData data)
        {
            var validator = new AssignBrandProductValidator(_repository, 
                _queries.GetAllowedProductsByBrand(data.BrandId));
            return validator.Validate(data);
        }

        private Brand.Data.Brand GetBrandForActivation(Guid brandId)
        {
            var brand = _repository.Brands
                .Include(b => b.BrandCountries.Select(x => x.Country))
                .Include(b => b.BrandCultures.Select(x => x.Culture))
                .Include(b => b.BrandCurrencies.Select(x => x.Currency))
                .Include(b => b.VipLevels)
                .Include(b => b.WalletTemplates)
                .Include(b => b.Licensee.Brands)
                .Include(b => b.Products)
                .Include(b => b.DefaultVipLevel)
                .Single(b => b.Id == brandId);

            return brand;
        }

        [Permission(Permissions.Activate, Module = Modules.BrandManager)]
        public void ActivateBrand(BrandId brandId, string remarks)
        {
            var brand = GetBrandForActivation(brandId);

            var validationResult = ValidateThatBrandCanBeActivated(brandId, remarks);

            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                brand.Status = BrandStatus.Active;
                brand.UpdatedBy = brand.ActivatedBy = _userInfoProvider.User.Username;
                brand.DateUpdated = brand.DateActivated = DateTimeOffset.UtcNow;
                brand.Remarks = remarks;

                _repository.SaveChanges();

                _eventBus.Publish(new BrandActivated
                {
                    Id = brand.Id,
                    Name = brand.Name,
                    TimezoneId = brand.TimezoneId,
                    LicenseeId = brand.Licensee.Id,
                    LicenseeName = brand.Licensee.Name,
                    Currencies = brand.BrandCurrencies.Select(x => x.Currency).ToDictionary(currency => new KeyValuePair<string, string>(currency.Code, currency.Name)),
                    Countries = brand.BrandCountries.Select(x => x.Country).ToDictionary(country => new KeyValuePair<string, string>(country.Code, country.Name)),
                    VipLevels = brand.VipLevels.ToDictionary(vipLevel => new KeyValuePair<string, string>(vipLevel.Code, vipLevel.Name)),
                    WalletTemplates = brand.WalletTemplates.Select(wt => new WalletTemplateData
                    {
                        Id = wt.Id,
                        Name = wt.Name,
                        ProductIds = wt.WalletTemplateProducts.Select(wtp => wtp.ProductId).ToList()
                    }).ToList(),
                    DateActivated = brand.DateActivated.Value,
                    ActivatedBy = brand.ActivatedBy
                });
                scope.Complete();
            }
        }

        [Permission(Permissions.Deactivate, Module = Modules.BrandManager)]
        public void DeactivateBrand(BrandId brandId, string remarks)
        {
            var brand = _repository.Brands.SingleOrDefault(x => x.Id == brandId);

            if (brand != null)
                brand.Remarks = remarks;

            var deactivateBrandData = new DeactivateBrandValidationData { Brand = brand };

            var validationResult = new DeactivateBrandValidator().Validate(deactivateBrandData);

            if (!validationResult.IsValid)
                throw new RegoValidationException(validationResult);

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                brand.Status = BrandStatus.Deactivated;
                brand.UpdatedBy = brand.DeactivatedBy = _userInfoProvider.User.Username;
                brand.DateUpdated = brand.DateDeactivated = DateTimeOffset.UtcNow;

                _repository.SaveChanges();

                _eventBus.Publish(new BrandDeactivated(brand));
                scope.Complete();
            }
        }

        [Permission(Permissions.Add, Module = Modules.SupportedLanguages)]
        public void AssignBrandCulture(AssignBrandCultureData assignBrandCultureData)
        {
            var validationResult = new AssignBrandCultureValidator(_repository).Validate(assignBrandCultureData);

            if (!validationResult.IsValid)
                throw new RegoValidationException(validationResult);

            var brand = _repository.Brands
                .Include(x => x.BrandCultures)
                .Single(x => x.Id == assignBrandCultureData.Brand);

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var oldCultures = brand.BrandCultures
                    .Where(x => !assignBrandCultureData.Cultures.Contains(x.CultureCode))
                    .ToArray();

                foreach (var oldCulture in oldCultures)
                {
                    brand.BrandCultures.Remove(oldCulture);
                }

                var newCultures = assignBrandCultureData.Cultures
                    .Where(x => brand.BrandCultures.All(y => y.CultureCode != x))
                    .ToArray();

                foreach (var culture in newCultures)
                {
                    var cultureToAdd = _repository.Cultures.Single(x => x.Code == culture);

                    brand.BrandCultures.Add(new BrandCulture
                    {
                        BrandId = brand.Id,
                        Brand = brand,
                        CultureCode = cultureToAdd.Code,
                        Culture = cultureToAdd,
                        DateAdded = DateTimeOffset.UtcNow,
                        AddedBy = _userInfoProvider.User.Username
                    });
                }

                brand.DefaultCulture = assignBrandCultureData.DefaultCulture;
                brand.DefaultCulture = assignBrandCultureData.DefaultCulture;
                brand.DateUpdated = DateTimeOffset.UtcNow;
                brand.UpdatedBy = _userInfoProvider.User.Username;

                _repository.SaveChanges();

                _eventBus.Publish(new BrandLanguagesAssigned(
                    brand.Id, 
                    brand.Name, 
                    brand.BrandCultures.Select(x => x.Culture)));

                scope.Complete();
            }
        }

        [Permission(Permissions.Add, Module = Modules.SupportedCountries)]
        public void AssignBrandCountry(AssignBrandCountryData assignBrandCountryData)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var validationResult = new AssignBrandCountryValidator(_repository).Validate(assignBrandCountryData);

                if (!validationResult.IsValid)
                    throw new RegoValidationException(validationResult);

                var brand = _repository.Brands
                    .Include(x => x.BrandCountries)
                    .Single(x => x.Id == assignBrandCountryData.Brand);

                var oldCountries = brand.BrandCountries
                    .Where(x => !assignBrandCountryData.Countries.Contains(x.CountryCode))
                    .ToArray();

                foreach (var oldCountry in oldCountries)
                {
                    brand.BrandCountries.Remove(oldCountry);
                }

                var newCountries = assignBrandCountryData.Countries
                    .Where(x => brand.BrandCountries.All(y => y.CountryCode != x))
                    .ToArray();

                foreach (var country in newCountries)
                {
                    var countryToAdd = _repository.Countries.Single(x => x.Code == country);

                    brand.BrandCountries.Add(new BrandCountry
                    {
                        BrandId = brand.Id,
                        Brand = brand,
                        CountryCode = countryToAdd.Code,
                        Country = countryToAdd,
                        DateAdded = DateTimeOffset.UtcNow,
                        AddedBy = _userInfoProvider.User.Username
                    });
                }

                brand.DateUpdated = DateTimeOffset.UtcNow;
                brand.UpdatedBy = _userInfoProvider.User.Username;

                _repository.SaveChanges();

                _eventBus.Publish(new BrandCountriesAssigned(brand));

                scope.Complete();
            }
        }

        [Permission(Permissions.Add, Module = Modules.BrandCurrencyManager)]
        public void AssignBrandCurrency(AssignBrandCurrencyData assignBrandCurrencyData)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var brand = _repository.Brands
                    .Include(x => x.BrandCurrencies)
                    .Single(x => x.Id == assignBrandCurrencyData.Brand);

                if (brand.BrandCurrencies.Count == 0)
                {
                    brand.CurrencySetCreated = DateTime.Now;
                    brand.CurrencySetCreatedBy = Thread.CurrentPrincipal.Identity.Name;
                }
                else
                {
                    var oldCurrencies = brand.BrandCurrencies
                        .Where(x => !assignBrandCurrencyData.Currencies.Contains(x.CurrencyCode))
                        .ToArray();

                    foreach (var oldCurrency in oldCurrencies)
                    {
                        brand.BrandCurrencies.Remove(oldCurrency);
                    }

                    brand.CurrencySetUpdated = DateTime.Now;
                    brand.CurrencySetUpdatedBy = Thread.CurrentPrincipal.Identity.Name;
                }

                brand.DefaultCurrency = assignBrandCurrencyData.DefaultCurrency;
                brand.BaseCurrency = assignBrandCurrencyData.BaseCurrency;

                var newCurrencies =
                    assignBrandCurrencyData.Currencies.Where(x => brand.BrandCurrencies.All(y => y.CurrencyCode != x));

                foreach (var currency in newCurrencies
                    .Select(newCurrency => _repository.Currencies.Single(c => c.Code == newCurrency)))
                {
                    brand.BrandCurrencies.Add(new BrandCurrency
                    {
                        BrandId = brand.Id,
                        Brand = brand,
                        CurrencyCode = currency.Code,
                        Currency = currency,
                        DateAdded = DateTimeOffset.UtcNow,
                        AddedBy = _userInfoProvider.User.Username
                    });
                }

                brand.DateUpdated = DateTimeOffset.UtcNow;
                brand.UpdatedBy = _userInfoProvider.User.Username;

                _repository.SaveChanges();
                _eventBus.Publish(new BrandCurrenciesAssigned
                {
                    BrandId = brand.Id,
                    Currencies = brand.BrandCurrencies.Select(bc => bc.CurrencyCode).ToArray(),
                    DefaultCurrency = brand.DefaultCurrency,
                    BaseCurrency = brand.BaseCurrency,
                });

                scope.Complete();
            }
        }

        public void AssignBrandProducts(AssignBrandProductsData assignBrandProductsData)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var validator = new AssignBrandProductValidator(_repository,
                    _queries.GetAllowedProductsByBrand(assignBrandProductsData.BrandId));

                var validationResult = validator.Validate(assignBrandProductsData);

                if (!validationResult.IsValid)
                    throw new RegoValidationException(validationResult);

                var brand = _repository.Brands
                    .Include(x => x.Products)
                    .Single(x => x.Id == assignBrandProductsData.BrandId);

                brand.Products.Clear();

                foreach (var product in assignBrandProductsData.ProductsIds)
                {
                    brand.Products.Add(new BrandProduct
                    {
                        Brand = brand,
                        ProductId = product
                    });
                }

                brand.DateUpdated = DateTimeOffset.UtcNow;
                brand.UpdatedBy = _userInfoProvider.User.Username;

                _eventBus.Publish(new BrandProductsAssigned
                {
                    BrandId = brand.Id,
                    ProductsIds = assignBrandProductsData.ProductsIds
                });

                _repository.SaveChanges();
                scope.Complete();
            }
        }

        [Permission(Permissions.Activate, Module = Modules.LanguageManager)]
        public void ActivateCulture(string code, string remarks)
        {
            UpdateCultureStatus(code, CultureStatus.Active, remarks);
        }

        [Permission(Permissions.Deactivate, Module = Modules.LanguageManager)]
        public void DeactivateCulture(string code, string remarks)
        {
            UpdateCultureStatus(code, CultureStatus.Inactive, remarks);
        }

        private void UpdateCultureStatus(string code, CultureStatus status, string remarks)
        {
            var culture = _repository.Cultures.First(x => x.Code == code);

            if (culture.Status == status)
                return;

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var user = _userInfoProvider.User.Username;
                culture.Status = status;
                culture.UpdatedBy = user;
                culture.DateUpdated = DateTimeOffset.UtcNow;

                if (status == CultureStatus.Active)
                {
                    culture.ActivatedBy = user;
                    culture.DateActivated = culture.DateUpdated;
                }
                else
                {
                    culture.DeactivatedBy = user;
                    culture.DateDeactivated = culture.DateUpdated;
                }

                _repository.SaveChanges();

                var languageStatusChanged = new LanguageStatusChanged(culture)
                {
                    Remarks = remarks
                };
                _eventBus.Publish(languageStatusChanged);

                scope.Complete();
            }
        }


        public ValidationResult ValidateThatVipLevelCanBeAdded(VipLevelViewModel model)
        {
            var validator = new AddVipLevelValidator(
                _repository,
                _gameQueries.GetGameDtos());
            return validator.Validate(model);
        }

        [Permission(Permissions.Add, Module = Modules.VipLevelManager)]
        public Guid AddVipLevel(VipLevelViewModel model)
        {
            var validationResult = ValidateThatVipLevelCanBeAdded(model);
            if (!validationResult.IsValid)
                throw new RegoValidationException(validationResult);

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var brand = _repository.Brands.Include(x => x.VipLevels).Single(x => x.Id == model.Brand);

                var vipLevel = new VipLevel
                {
                    Id = Guid.NewGuid(),
                    BrandId = brand.Id,
                    Code = model.Code,
                    Name = model.Name,
                    Rank = model.Rank,
                    Description = model.Description,
                    ColorCode = model.Color,
                    CreatedBy = _securityProvider.User.UserName,
                    DateCreated = DateTimeOffset.UtcNow
                };

                var vipLevelLimits = model.Limits.Select(x => new VipLevelGameProviderBetLimit
                {
                    Id = Guid.NewGuid(),
                    VipLevel = vipLevel,
                    GameProviderId = x.GameProviderId.Value,
                    Currency = _repository.Currencies.Single(y => y.Code == x.CurrencyCode),
                    BetLimitId = x.BetLimitId.Value
                }).ToList();

                vipLevel.VipLevelLimits = vipLevelLimits;

                _repository.VipLevels.Add(vipLevel);
                brand.VipLevels.Add(vipLevel);
                _repository.SaveChanges();

                _eventBus.Publish(new VipLevelRegistered
                {
                    Id = vipLevel.Id,
                    BrandId = vipLevel.BrandId,
                    Code = vipLevel.Code,
                    Name = vipLevel.Name,
                    Rank = vipLevel.Rank,
                    Description = vipLevel.Description,
                    ColorCode = vipLevel.ColorCode,
                    Status = vipLevel.Status,
                    CreatedBy = vipLevel.CreatedBy,
                    DateCreated = vipLevel.DateCreated,

                    VipLevelLimits = vipLevel.VipLevelLimits.Select(x => new VipLevelLimitData
                    {
                        Id = x.Id,
                        VipLevelId = vipLevel.Id,
                        CurrencyCode = x.Currency.Code,
                        GameProviderId = x.GameProviderId,
                        BetLimitId = x.BetLimitId

                    }).ToArray()
                });

                if (model.IsDefault)
                    SetDefaultVipLevel(brand, vipLevel.Id);

                scope.Complete();

                return vipLevel.Id;
            }
        }

        public void SetDefaultVipLevel(Brand.Data.Brand brand, Guid vipLevelId)
        {
            var oldVipLevelId = brand.DefaultVipLevelId;
            brand.DefaultVipLevelId = vipLevelId;

            _eventBus.Publish(new BrandDefaultVipLevelChanged
            {
                BrandId = brand.Id,
                OldVipLevelId = oldVipLevelId,
                DefaultVipLevelId = vipLevelId
            });
            _repository.SaveChanges();
        }

        [Permission(Permissions.Edit, Module = Modules.VipLevelManager)]
        public void EditVipLevel(VipLevelViewModel model)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var validationResult = new EditVipLevelValidator(
                    _repository,
                    _gameQueries.GetGameDtos()).Validate(model);

                if (!validationResult.IsValid)
                    throw new RegoValidationException(validationResult);

                var existingVipLevel = _repository
                    .VipLevels
                    .Include(x => x.VipLevelLimits)
                    .Single(x => x.Id == model.Id);

                //update viplevel
                var brand = _repository.Brands.Single(x => x.Id == model.Brand);
                existingVipLevel.Brand = brand;
                existingVipLevel.Code = model.Code;
                existingVipLevel.Name = model.Name;
                existingVipLevel.Rank = model.Rank;
                existingVipLevel.Description = model.Description;
                existingVipLevel.ColorCode = model.Color;
                existingVipLevel.UpdatedBy = _securityProvider.User.UserName;
                existingVipLevel.DateUpdated = DateTimeOffset.UtcNow;
                existingVipLevel.UpdatedRemark = model.Remark;
                //remove removed limits
                var removedLimits = existingVipLevel
                    .VipLevelLimits
                    .Where(x => model.Limits.All(lvm => lvm.Id != x.Id))
                    .ToArray();

                removedLimits.ForEach(x => existingVipLevel.VipLevelLimits.Remove(x));

                //updating viplimits
                foreach (var limitViewModel in model.Limits)
                {
                    var limit = existingVipLevel.
                        VipLevelLimits
                        .SingleOrDefault(x => x.Id == limitViewModel.Id);


                    if (limit == null)
                    {
                        limit = new VipLevelGameProviderBetLimit()
                        {
                            Id = Guid.NewGuid(),
                            VipLevel = existingVipLevel,
                            GameProviderId = limitViewModel.GameProviderId.Value,
                            Currency = _repository.Currencies.Single(y => y.Code == limitViewModel.CurrencyCode),
                            BetLimitId = limitViewModel.BetLimitId.Value
                        };
                        existingVipLevel.VipLevelLimits.Add(limit);
                    }
                    else
                    {
                        limit.VipLevel = existingVipLevel;
                        limit.GameProviderId = limitViewModel.GameProviderId.Value;
                        limit.Currency = _repository.Currencies.Single(y => y.Code == limitViewModel.CurrencyCode);
                        limit.BetLimitId = limitViewModel.BetLimitId.Value;
                    }
                }

                //save and publish
                _repository.SaveChanges();
                _eventBus.Publish(new VipLevelUpdated
                {
                    Id = existingVipLevel.Id,
                    BrandId = existingVipLevel.Brand.Id,
                    Code = existingVipLevel.Code,
                    Name = existingVipLevel.Name,
                    Rank = existingVipLevel.Rank,
                    Description = existingVipLevel.Description,
                    ColorCode = existingVipLevel.ColorCode,
                    Remark = model.Remark,
                    UpdatedBy = existingVipLevel.UpdatedBy,
                    DateUpdated = existingVipLevel.DateUpdated,

                    VipLevelLimits = existingVipLevel.VipLevelLimits.Select(x => new VipLevelLimitData
                    {
                        Id = x.Id,
                        VipLevelId = existingVipLevel.Id,
                        CurrencyCode = x.Currency.Code,
                        GameProviderId = x.GameProviderId,
                        BetLimitId = x.BetLimitId
                    }).ToArray()
                });

                scope.Complete();
            }
        }

        [Permission(Permissions.Add, Module = Modules.WalletManager)]
        public void CreateWalletStructureForBrand(WalletTemplateViewModel viewModel)
        {
            var validationResult = new AddWalletValidator()
                .Validate(viewModel);

            if (!validationResult.IsValid)
                throw new RegoValidationException(validationResult);

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var wallets = new List<WalletTemplate>();
                wallets.Add(CreateWalletTemplate(
                    viewModel.BrandId,
                    viewModel.MainWallet.Name,
                    viewModel.MainWallet.ProductIds,
                    true));

                foreach (var walletViewModel in viewModel.ProductWallets)
                {
                    wallets.Add(CreateWalletTemplate(
                        viewModel.BrandId,
                        walletViewModel.Name,
                        walletViewModel.ProductIds,
                        false));
                }
                _repository.SaveChanges();

                _eventBus.Publish(new WalletTemplateCreated
                {
                    BrandId = viewModel.BrandId,
                    WalletTemplates = wallets.Select(x => new WalletTemplateDto
                    {
                        Id = x.Id,
                        IsMain = x.IsMain,
                        Name = x.Name,
                        ProductIds = x.WalletTemplateProducts.Select(wt => wt.ProductId).ToArray()
                    }).ToArray()
                });
                scope.Complete();
            }
        }

        private WalletTemplate CreateWalletTemplate(
            Guid brandId,
            string name,
            IEnumerable<Guid> productIds,
            bool isMain)
        {
            var brand = _repository.Brands.Single(x => x.Id == brandId);
            var walletTemplate = new WalletTemplate
            {
                Id = Guid.NewGuid(),
                DateCreated = DateTime.UtcNow,
                CreatedBy = _securityProvider.User.UserId,
                Name = name,
                CurrencyCode = brand.DefaultCurrency,
                IsMain = isMain
            };

            foreach (var productId in productIds)
            {
                var walletProduct = new WalletTemplateProduct { Id = Guid.NewGuid(), ProductId = productId };
                walletTemplate.WalletTemplateProducts.Add(walletProduct);
            }

            brand.WalletTemplates.Add(walletTemplate);

            return walletTemplate;
        }

        [Permission(Permissions.Edit, Module = Modules.WalletManager)]
        public void UpdateWalletStructureForBrand(WalletTemplateViewModel viewModel)
        {
            var validationResult = new EditWalletValidator().Validate(viewModel);

            if (!validationResult.IsValid)
                throw new RegoValidationException(validationResult);

            var existingWallets = _repository
                .WalletTemplates
                .Include(x => x.Brand)
                .Where(x => x.Brand.Id == viewModel.BrandId && !x.IsMain)
                .ToArray();
            var walletTemplatesToDelete =
                existingWallets.Where(x => viewModel.ProductWallets.All(y => y.Id != x.Id)).ToArray();
            walletTemplatesToDelete.ForEach(x => _repository.WalletTemplates.Remove(x));

            var remainedWallets = viewModel.ProductWallets.Where(x => x.Id != null && x.Id != Guid.Empty).ToArray();
            remainedWallets = remainedWallets.Concat(new[] { viewModel.MainWallet }).ToArray();
            remainedWallets.ForEach(rw => UpdateWalletTemplate(viewModel.BrandId, rw));

            var newWallets = viewModel.ProductWallets.Where(x => x.Id == null || x.Id == Guid.Empty).ToArray();
            newWallets.ForEach(x =>
            {
                var walleTemplate = CreateWalletTemplate(viewModel.BrandId, x.Name, x.ProductIds, false);
                x.Id = walleTemplate.Id;
            });

            _repository.SaveChanges();

            _eventBus.Publish(new WalletTemplateUpdated
            {
                BrandId = viewModel.BrandId,
                RemovedWalletTemplateIds = walletTemplatesToDelete.Select(x => x.Id).ToArray(),
                RemainedWalletTemplates = remainedWallets.Select(x => new WalletTemplateDto
                {
                    Id = x.Id.Value,
                    IsMain = x.IsMain,
                    Name = x.Name,
                    ProductIds = x.ProductIds.ToArray()
                }).ToArray(),
                NewWalletTemplates = newWallets.Select(x => new WalletTemplateDto
                {
                    Id = x.Id.Value,
                    IsMain = x.IsMain,
                    Name = x.Name,
                    ProductIds = x.ProductIds.ToArray()
                }).ToArray()
            });
        }

        public void MakePaymentLevelDefault(Guid newPaymentLevelId, Guid brandId, string currencyCode)
        {
            var brand = _repository.Brands
                .Include(o => o.BrandCurrencies)
                .Single(o => o.Id == brandId);

            var brandCurrency = brand.BrandCurrencies
                .SingleOrDefault(o => o.CurrencyCode == currencyCode && o.BrandId == brandId);

            if (brandCurrency == null)
                throw new RegoValidationException("{\"text\": \"app:brand.activation.noAssignedCurrency\"}");

            brandCurrency.DefaultPaymentLevelId = newPaymentLevelId;

            _repository.SaveChanges();
        }

        private void UpdateWalletTemplate(Guid brandId, WalletViewModel walletModel)
        {
            var wallet = _repository.Brands
                .Include(x => x.WalletTemplates.Select(wt => wt.WalletTemplateProducts))
                .Single(x => x.Id == brandId)
                .WalletTemplates
                .Single(wt => wt.Id == walletModel.Id);

            wallet.DateUpdated = DateTimeOffset.UtcNow;
            wallet.UpdatedBy = _securityProvider.User.UserId;
            wallet.Name = walletModel.Name;

            var walletTemplateProductsToRemove =
                wallet
                    .WalletTemplateProducts
                    .Where(x => !walletModel.ProductIds.Contains(x.ProductId))
                    .ToList();

            var walletTemplateProductsToAdd =
                walletModel
                    .ProductIds
                    .Where(x => !wallet.WalletTemplateProducts.Select(y => y.ProductId).Contains(x))
                    .ToList();

            if (walletTemplateProductsToRemove.Any() && wallet.Brand.Status == BrandStatus.Active)
                throw new RegoValidationException("It is not allowed to remove products from active brand's wallet.");

            walletTemplateProductsToRemove.ForEach(product => wallet.WalletTemplateProducts.Remove(product));
            walletTemplateProductsToAdd.ForEach(productId =>
            {
                var walletTemplateProduct = new WalletTemplateProduct
                {
                    Id = Guid.NewGuid(),
                    WalletTemplateId = wallet.Id,
                    WalletTemplate = wallet,
                    ProductId = productId
                };
                wallet.WalletTemplateProducts.Add(walletTemplateProduct);
            });
        }


        [Permission(Permissions.Activate, Module = Modules.VipLevelManager)]
        public void ActivateVipLevel(Guid vipLevelId, string remark)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var vipLevel = _repository.VipLevels
                    .Include(v => v.Brand).Include(v => v.VipLevelLimits)
                    .Single(v => v.Id == vipLevelId);

                vipLevel.Status = VipLevelStatus.Active;
                vipLevel.UpdatedRemark = remark;
                vipLevel.UpdatedBy = _securityProvider.User.UserName;
                vipLevel.DateUpdated = DateTimeOffset.Now;
                _repository.SaveChanges();

                _eventBus.Publish(new VipLevelActivated { VipLevelId = vipLevelId });
                scope.Complete();
            }
        }

        [Permission(Permissions.Deactivate, Module = Modules.VipLevelManager)]
        public void DeactivateVipLevel(Guid deactivateVipLevelId, string remark, Guid? newDefaultVipLevelId)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var vipLevelToDeactivate = _repository.VipLevels
                    .Include(v => v.Brand)
                    .Include(v => v.VipLevelLimits)
                    .Single(v => v.Id == deactivateVipLevelId);

                var isDefaultForItsBrand = vipLevelToDeactivate.Brand.DefaultVipLevelId == vipLevelToDeactivate.Id;

                if (isDefaultForItsBrand && !newDefaultVipLevelId.HasValue)
                    throw new RegoException(
                        "Unable to deactivate default vip level. Please specify new default vip level");

                if (isDefaultForItsBrand)
                {
                    var newVipLevel = _repository.VipLevels
                        .Include(o => o.Brand)
                        .Single(o => o.Id == newDefaultVipLevelId.Value);

                    SetDefaultVipLevel(newVipLevel.Brand, newVipLevel.Id);

                    _eventBus.Publish(new BrandDefaultVipLevelChanged
                    {
                        BrandId = vipLevelToDeactivate.BrandId,
                        OldVipLevelId = deactivateVipLevelId,
                        DefaultVipLevelId = newVipLevel.Id
                    });
                }

                vipLevelToDeactivate.Status = VipLevelStatus.Inactive;
                vipLevelToDeactivate.UpdatedRemark = remark;
                vipLevelToDeactivate.UpdatedBy = _securityProvider.User.UserName;
                vipLevelToDeactivate.DateUpdated = DateTimeOffset.Now;
                _repository.SaveChanges();

                _eventBus.Publish(new VipLevelDeactivated { VipLevelId = deactivateVipLevelId });

                scope.Complete();
            }
        }

        [Permission(Permissions.Add, Module = Modules.CountryManager)]
        public void CreateCountry(string code, string name)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var country = new Country
                {
                    Code = code,
                    Name = name
                };
                _repository.Countries.Add(country);
                _repository.SaveChanges();
                _eventBus.Publish(new CountryCreated(country));

                scope.Complete();
            }
        }

        [Permission(Permissions.Edit, Module = Modules.CountryManager)]
        public void UpdateCountry(string code, string name)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var country = _repository.Countries.SingleOrDefault(c => c.Code == code);

                if (country == null)
                {
                    throw new RegoException("Country not found");
                }

                country.Name = name;

                _repository.SaveChanges();
                _eventBus.Publish(new CountryUpdated(country));

                scope.Complete();
            }
        }

        [Permission(Permissions.Delete, Module = Modules.CountryManager)]
        public void DeleteCountry(string code)
        {
            var country = _repository.Countries.Single(c => c.Code == code);

            _repository.Countries.Remove(country);
            _repository.SaveChanges();
            _eventBus.Publish(new CountryRemoved(country));
        }

    }
}

