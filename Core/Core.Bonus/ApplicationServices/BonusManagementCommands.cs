using System;
using System.Linq;
using AFT.RegoV2.ApplicationServices.Security;
using AFT.RegoV2.Core.Bonus.Data;
using AFT.RegoV2.Core.Bonus.Data.ViewModels;
using AFT.RegoV2.Core.Bonus.DomainServices;
using AFT.RegoV2.Core.Common.Events.Bonus;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Domain.Security.Interfaces;
using AFT.RegoV2.Shared;

namespace AFT.RegoV2.Core.Bonus.ApplicationServices
{
    public class BonusManagementCommands : MarshalByRefObject, IApplicationService
    {
        private readonly IBonusRepository _repository;
        private readonly BonusQueries _bonusQueries;
        private readonly ISecurityProvider _security;
        private readonly IEventBus _eventBus;
        private readonly BonusValidator _bonusValidator;
        private readonly TemplateValidator _templateValidator;

        public BonusManagementCommands(
            IBonusRepository repository,
            BonusQueries bonusQueries,
            ISecurityProvider security,
            IEventBus eventBus)
        {
            _repository = repository;
            _bonusQueries = bonusQueries;
            _security = security;
            _eventBus = eventBus;

            _bonusValidator = new BonusValidator(repository);
            _templateValidator = new TemplateValidator(repository, bonusQueries);
        }

        [Permission(Permissions.Add, Module = Modules.BonusManager)]
        public void AddBonus(Data.Bonus bonus)
        {
            var validationResult = _bonusValidator.Validate(bonus);
            if (validationResult.IsValid == false)
                throw new RegoException(string.Join("/n", validationResult.Errors.Select(failure => failure.ErrorMessage)));

            bonus.Id = Guid.NewGuid();
            bonus.Template = _repository.Templates.Single(a => a.Id == bonus.Template.Id && a.Version == bonus.Template.Version);
            bonus.CreatedOn = DateTimeOffset.Now.ToBrandOffset(bonus.Template.Info.Brand.TimezoneId);
            bonus.CreatedBy = _security.User.UserName;
            bonus.Statistic = new BonusStatistic();

            _repository.Bonuses.Add(bonus);
            _eventBus.Publish(new BonusCreated
            {
                Id = bonus.Id,
                Description = bonus.Description
            });
            _repository.SaveChanges();
        }

        [Permission(Permissions.Edit, Module = Modules.BonusManager)]
        public void UpdateBonus(Data.Bonus updatedBonus)
        {
            var validationResult = _bonusValidator.Validate(updatedBonus);
            if (validationResult.IsValid == false)
                throw new RegoException(string.Join("/n", validationResult.Errors.Select(failure => failure.ErrorMessage)));

            updatedBonus.Template = _repository.Templates.Single(a => a.Id == updatedBonus.Template.Id && a.Version == updatedBonus.Template.Version);

            //to persist bonus statistic
            updatedBonus.Statistic = null;
            updatedBonus.StatisticId = _bonusQueries.GetCurrentVersionBonuses().Single(a => a.Id == updatedBonus.Id).StatisticId;
            updatedBonus.Version++;

            var firstBonusVersion = _repository.Bonuses.Where(bonus => bonus.Id == updatedBonus.Id).Single(t => t.Version == 0);
            updatedBonus.CreatedOn = firstBonusVersion.CreatedOn;
            updatedBonus.CreatedBy = firstBonusVersion.CreatedBy;
            updatedBonus.UpdatedOn = DateTimeOffset.Now.ToBrandOffset(updatedBonus.Template.Info.Brand.TimezoneId);
            updatedBonus.UpdatedBy = _security.User.UserName;

            _repository.Bonuses.Add(updatedBonus);
            _eventBus.Publish(new BonusUpdated
            {
                Id = updatedBonus.Id,
                Description = updatedBonus.Description
            });
            _repository.SaveChanges();
        }

        [Permission(Permissions.Activate, Module = Modules.BonusManager)]
        [Permission(Permissions.Deactivate, Module = Modules.BonusManager)]
        public void ChangeBonusStatus(ToggleBonusStatusVM model)
        {
            var validationResult = _bonusQueries.GetValidationResult(model);
            if (validationResult.IsValid == false)
                throw new RegoException(validationResult.Errors.First().ErrorMessage);

            var bonusToUpdate = _bonusQueries.GetCurrentVersionBonuses().Single(b => b.Id == model.Id);
            var isActive = model.IsActive;
            if (bonusToUpdate.IsActive == isActive)
                return;

            bonusToUpdate.IsActive = isActive;
            bonusToUpdate.Description = model.Description;
            UpdateBonus(bonusToUpdate);

            if (isActive)
                _eventBus.Publish(new BonusActivated
                {
                    Id = bonusToUpdate.Id,
                    Description = bonusToUpdate.Description
                });
            else
                _eventBus.Publish(new BonusDeactivated
                {
                    Id = bonusToUpdate.Id,
                    Description = bonusToUpdate.Description
                });
        }

        [Permission(Permissions.Add, Module = Modules.BonusTemplateManager)]
        [Permission(Permissions.Edit, Module = Modules.BonusTemplateManager)]
        public void AddUpdateTemplate(Template template)
        {
            var validationResult = _templateValidator.Validate(template);
            if (validationResult.IsValid == false)
                throw new RegoException(string.Join("/n", validationResult.Errors.Select(failure => failure.ErrorMessage)));

            template.Info.Brand = _repository.Brands.Single(brand => brand.Id == template.Info.Brand.Id);

            if (template.Id == Guid.Empty)
            {
                template.Id = Guid.NewGuid();
                template.CreatedOn = DateTimeOffset.Now.ToBrandOffset(template.Info.Brand.TimezoneId);
                template.CreatedBy = _security.User.UserName;
            }
            else
            {
                if (template.Status == TemplateStatus.Complete)
                {
                    var bonusesUsingThisTemplate = _bonusQueries.GetBonusesUsingTemplate(template);
                    foreach (var bonus in bonusesUsingThisTemplate)
                    {
                        bonus.Statistic = null;
                        bonus.Template = template;
                        bonus.Version++;
                        _repository.Bonuses.Add(bonus);
                    }
                }

                var firstTemplateVersion = _repository.Templates.Where(t => t.Id == template.Id).Single(t => t.Version == 0);
                template.CreatedOn = firstTemplateVersion.CreatedOn;
                template.CreatedBy = firstTemplateVersion.CreatedBy;
                template.UpdatedOn = DateTimeOffset.Now.ToBrandOffset(template.Info.Brand.TimezoneId);
                template.UpdatedBy = _security.User.UserName;
                if (template.Status == TemplateStatus.Complete)
                {
                    template.Version++;
                    _eventBus.Publish(new BonusTemplateUpdated
                    {
                        Id = template.Id,
                        Description = template.Info.Description
                    });
                }
                else
                {
                    if (template.Notification != null)
                    {
                        template.Status = TemplateStatus.Complete;
                        _eventBus.Publish(new BonusTemplateCreated
                        {
                            Id = template.Id,
                            Description = template.Info.Description
                        });
                    }
                    _repository.Templates.Remove(firstTemplateVersion);
                }
            }

            _repository.Templates.Add(template);
            _repository.SaveChanges();
        }

        [Permission(Permissions.Delete, Module = Modules.BonusTemplateManager)]
        public void DeleteTemplate(Guid templateId)
        {
            var validationResult = _bonusQueries.GetValidationResult(templateId);
            if (validationResult.IsValid == false)
                throw new RegoException(validationResult.Errors.First().ErrorMessage);

            var templates = _repository.Templates.Where(t => t.Id == templateId);
            templates.ToList().ForEach(template => template.Status = TemplateStatus.Deleted);
            var lastVersion = templates.Max(t => t.Version);
            var lastTemplate = templates.Single(t => t.Version == lastVersion);
            lastTemplate.UpdatedOn = DateTimeOffset.Now.ToBrandOffset(lastTemplate.Info.Brand.TimezoneId);
            lastTemplate.UpdatedBy = _security.User.UserName;

            _repository.SaveChanges();
        }
    }
}