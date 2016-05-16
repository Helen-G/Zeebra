using System;
using System.Linq;
using AFT.RegoV2.ApplicationServices.Security;
using AFT.RegoV2.Core.Brand.Data;
using AFT.RegoV2.Core.Brand.Events.ContentTranslation;
using AFT.RegoV2.Core.Brand.Validators.ContentTranslations;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Core.Security.Interfaces;
using AFT.RegoV2.Shared;
using FluentValidation.Results;


namespace AFT.RegoV2.Core.Brand.ApplicationServices
{
    public class ContentTranslationCommands : MarshalByRefObject, IApplicationService
    {
        private readonly IBrandRepository _repository;
        private readonly IEventBus _eventBus;
        private readonly IUserInfoProvider _userInfoProvider;

        public ContentTranslationCommands(
            IBrandRepository repository, 
            IEventBus eventBus,
            IUserInfoProvider userInfoProvider) 
        {
            _repository = repository;
            _eventBus = eventBus;
            _userInfoProvider = userInfoProvider;
        }

        public ValidationResult ValidateThatContentTranslationCanBeAdded(AddContentTranslationData addContentTranslationData)
        {
            var validator = new AddContentTranslationValidator(_repository);
            return validator.Validate(addContentTranslationData);
        }

        public ValidationResult ValidateThatContentTranslationCanBeEdited(EditContentTranslationData editContentTranslationData)
        {
            var validator = new EditContentTranslationValidator(_repository);
            return validator.Validate(editContentTranslationData);
        }

        [Permission(Permissions.Add, Module = Modules.TranslationManager)]
        public void CreateContentTranslation(AddContentTranslationData addContentTranslationData)
        {
            if (_repository.ContentTranslations.Any(
                        t =>
                            t.Name == addContentTranslationData.ContentName &&
                            t.Source == addContentTranslationData.ContentSource &&
                            t.Language == addContentTranslationData.Language))
            {
                throw new RegoException("Translation already exist");
            }

            var validationResult = new AddContentTranslationValidator(_repository).Validate(addContentTranslationData);

            if (!validationResult.IsValid)
                throw new RegoValidationException(validationResult);

            var language = _repository.Cultures.SingleOrDefault(
                cc => 
                    cc.Code == addContentTranslationData.Language);

            if (language == null)
            {
                throw new RegoException("Language not found");
            }

            var contentTranslation = new ContentTranslation
            {
                Id = Guid.NewGuid(),
                Name = addContentTranslationData.ContentName,
                Source = addContentTranslationData.ContentSource,
                Language = language.Code,
                Translation = addContentTranslationData.Translation,
                Status = TranslationStatus.Disabled,
                Created = DateTimeOffset.Now,
                CreatedBy = _userInfoProvider.User.Username
            };

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                _repository.ContentTranslations.Add(contentTranslation);
                _eventBus.Publish(new ContentTranslationCreated(contentTranslation));
                _repository.SaveChanges();
                scope.Complete();
            }
        }

        [Permission(Permissions.Edit, Module = Modules.TranslationManager)]
        public void UpdateContentTranslation(EditContentTranslationData editContentTranslationDataData)
        {
            if (_repository.ContentTranslations.Any(
                        t =>
                            t.Name == editContentTranslationDataData.ContentName &&
                            t.Source == editContentTranslationDataData.ContentSource &&
                            t.Language == editContentTranslationDataData.Language &&
                            t.Id != editContentTranslationDataData.Id))
            {
                throw new RegoException("Translation already exist");
            }

            var translation = _repository.ContentTranslations.Single(x => x.Id == editContentTranslationDataData.Id);

            var language = _repository.Cultures.SingleOrDefault(
                cc =>
                    cc.Code == editContentTranslationDataData.Language);

            if (language == null)
            {
                throw new RegoException("Language not found");
            }

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {

                translation.Name = editContentTranslationDataData.ContentName;
                translation.Source = editContentTranslationDataData.ContentSource;
                translation.Translation = editContentTranslationDataData.Translation;
                translation.Language = language.Code;
                translation.Remark = editContentTranslationDataData.Remark;
                translation.Updated = DateTimeOffset.UtcNow;
                translation.UpdatedBy = _userInfoProvider.User.Username;

                _repository.SaveChanges();

                _eventBus.Publish(new ContentTranslationUpdated(translation));
                scope.Complete();
            }
        }

        [Permission(Permissions.Activate, Module = Modules.TranslationManager)]
        public void ActivateContentTranslation(Guid id, string remarks)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var translation = _repository.ContentTranslations.SingleOrDefault(x => x.Id == id);

                if (translation == null)
                {
                    throw new RegoException("Content translation not found");
                }

                translation.Status = TranslationStatus.Enabled;
                translation.Remark = remarks;
                translation.ActivatedBy = _userInfoProvider.User.Username;
                translation.Activated = DateTimeOffset.UtcNow;

                _repository.SaveChanges();

                _eventBus.Publish(new ContentTranslationStatusChanged(translation));

                scope.Complete();
            }
        }

        [Permission(Permissions.Deactivate, Module = Modules.TranslationManager)]
        public void DeactivateContentTranslation(Guid id, string remarks)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var translation = _repository.ContentTranslations.SingleOrDefault(x => x.Id == id);

                if (translation == null)
                {
                    throw new RegoException("Content translation not found");
                }

                translation.Status = TranslationStatus.Disabled;
                translation.Remark = remarks;
                translation.DeactivatedBy = _userInfoProvider.User.Username;
                translation.Deactivated = DateTimeOffset.UtcNow;

                _repository.SaveChanges();

                _eventBus.Publish(new ContentTranslationStatusChanged(translation));

                scope.Complete();
            }
        }

        [Permission(Permissions.Delete, Module = Modules.TranslationManager)]
        public void DeleteContentTranslation(Guid id)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var contentTranslation = _repository.ContentTranslations.SingleOrDefault(ct => ct.Id == id);
                _repository.ContentTranslations.Remove(contentTranslation);

                _repository.SaveChanges();

                _eventBus.Publish(new ContentTranslationDeleted(contentTranslation));
                scope.Complete();
            }
        }
    }
}
