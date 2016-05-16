using System;
using System.Collections.ObjectModel;
using System.Data.Entity.Migrations;
using System.Linq;
using AFT.RegoV2.ApplicationServices.Security;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Events.Game;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Game.Data;
using AFT.RegoV2.Core.Game.Events;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.Core.Game.Validations;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Core.Security.Interfaces;
using AFT.RegoV2.Domain.BoundedContexts.Security.Helpers;
using AFT.RegoV2.Domain.Security.Interfaces;

namespace AFT.RegoV2.Core.Game.ApplicationServices
{
    public class GameManagement : MarshalByRefObject, IGameManagement
    {
        private readonly IEventBus _eventBus;
        private readonly ISecurityProvider _securityProvider;
        private readonly IGameRepository _repository;
        private readonly IUserInfoProvider _userInfoProvider;

        public GameManagement(
            IGameRepository repository,
            ISecurityProvider securityProvider,
            IEventBus eventBus,
            IUserInfoProvider userInfoProvider)
        {
            _eventBus = eventBus;
            _securityProvider = securityProvider;
            _repository = repository;
            _userInfoProvider = userInfoProvider;
        }

        
        [Permission(Permissions.Add, Module = Modules.GameManager)]
        public void CreateGame(GameDTO game)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var validationResult = new AddGameValidator(_repository)
                    .Validate(game);

                if (!validationResult.IsValid)
                    throw validationResult.GetValidationError();

                var gameEntity = new Data.Game
                {
                    Id = Guid.NewGuid(),
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = _securityProvider.User.UserName,
                    Name = game.Name,
                    Type = game.Type,
                    GameProviderId = game.ProductId,
                    IsActive = game.Status == "Active",
                    EndpointPath = game.Url
                };

                _repository.Games.Add(gameEntity);
                _repository.SaveChanges();
                _eventBus.Publish(new GameCreated
                {
                    Id = gameEntity.Id,
                    GameProviderId = gameEntity.GameProviderId,
                    Name = gameEntity.Name,
                    Url = gameEntity.EndpointPath,
                    CreatedDate = gameEntity.CreatedDate,
                    CreatedBy = gameEntity.CreatedBy
                });
                scope.Complete();
            }
        }

        [Permission(Permissions.Edit, Module = Modules.GameManager)]
        public void UpdateGame(GameDTO game)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var validationResult =
                    new EditGameValidator()
                        .Validate(game);

                if (!validationResult.IsValid)
                    throw validationResult.GetValidationError();

                var gameEntity = _repository.Games.Single(x => x.Id == game.Id);

                gameEntity.UpdatedDate = DateTime.UtcNow;
                gameEntity.UpdatedBy = _securityProvider.User.UserName;
                gameEntity.Name = game.Name;
                gameEntity.Type = game.Type;
                gameEntity.GameProviderId = game.ProductId;
                gameEntity.IsActive = game.Status == "Active";
                gameEntity.EndpointPath = game.Url;

                _repository.SaveChanges();

                _eventBus.Publish(new GameUpdated
                {
                    Id = gameEntity.Id,
                    GameProviderId = gameEntity.GameProviderId,
                    Name = gameEntity.Name,
                    Url = gameEntity.EndpointPath,
                    UpdatedDate = gameEntity.UpdatedDate.GetValueOrDefault(),
                    UpdatedBy = gameEntity.UpdatedBy
                });
                scope.Complete();
            }
        }

        [Permission(Permissions.Delete, Module = Modules.GameManager)]
        public void DeleteGame(Guid id)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var game = _repository.Games.Single(x => x.Id == id);
                _repository.Games.Remove(game);
                _eventBus.Publish(new GameDeleted
                {
                    Id = game.Id,
                    Name = game.Name,
                    Url = game.EndpointPath,
                    CreatedDate = game.CreatedDate,
                    CreatedBy = game.CreatedBy
                });
                _repository.SaveChanges();
                scope.Complete();
            }
        }


        [Permission(Permissions.Add, Module = Modules.SupportedProducts)]
        [Permission(Permissions.Add, Module = Modules.BetLevels)]
        [Permission(Permissions.Edit, Module = Modules.BetLevels)]
        public void UpdateProductSettings(BrandProductSettingsData viewModel)
        {
            var limitsForBrandProduct =
                    _repository
                        .BetLimits
                        .Where(x => x.BrandId == viewModel.BrandId && x.GameProviderId == viewModel.ProductId);

            if (viewModel.BetLevels != null)
            {
                var existingBetLimits = _repository.BetLimits.ToList().Where(x => viewModel.BetLevels.Any(y => y.Id == x.Id));
                var newBetLimits = viewModel.BetLevels.Where(x => x.Id == Guid.Empty);

                var limitsToDelete =
                    limitsForBrandProduct.ToList().Where(x => !viewModel.BetLevels.Any(y => y.Id == x.Id && x.Id != Guid.Empty));
                limitsToDelete.ToList().ForEach(x =>
                {
                    _eventBus.Publish(new BetLimitDeleted(x));
                    _repository.BetLimits.Remove(x);
                });
                newBetLimits.ToList().ForEach(x =>
                {
                    var limit = new GameProviderBetLimit
                    {
                        Id = Guid.NewGuid(),
                        BrandId = viewModel.BrandId,
                        GameProviderId = viewModel.ProductId,
                        Code = x.Code,
                        Name = x.Name,
                        Description = x.Description,
                        DateCreated = DateTimeOffset.UtcNow,
                        CreatedBy = _userInfoProvider.User.Username
                    };
                    _repository.BetLimits.Add(limit);
                    _eventBus.Publish(new BetLimitCreated(limit));
                });

                existingBetLimits.ToList().ForEach(x =>
                {
                    var newLimit = viewModel.BetLevels.Single(y => y.Id == x.Id);
                    x.Name = newLimit.Name;
                    x.Description = newLimit.Description;
                    x.Code = newLimit.Code;
                    x.DateUpdated = DateTimeOffset.UtcNow;
                    x.UpdatedBy = _userInfoProvider.User.Username;
                });
            }
            else
            {
                limitsForBrandProduct.ToList().ForEach(x => _repository.BetLimits.Remove(x));
            }

            _repository.SaveChanges();
        }

        [Permission(Permissions.Add, Module = Modules.ProductManager)]
        public void CreateGameProvider(GameProvider gameProvider)
        {
            gameProvider.IsActive = false;
            gameProvider.Id = Guid.NewGuid();

            gameProvider.CreatedBy = _securityProvider.User.UserName;
            gameProvider.CreatedDate = DateTimeOffset.Now;

            gameProvider.GameProviderConfigurations = new Collection<GameProviderConfiguration>
            {
                new GameProviderConfiguration
                {
                    Id = Guid.NewGuid(),
                    GameProviderId = gameProvider.Id,
                    Name = "Default Configuration for " + gameProvider.Name
                }
            };
            _repository.GameProviders.Add(gameProvider);

            _repository.SaveChanges();
            _eventBus.Publish(new ProductCreated(gameProvider));
        }

        [Permission(Permissions.Edit, Module = Modules.ProductManager)]
        public void UpdateGameProvider(GameProvider gameProvider)
        {
            gameProvider.UpdatedBy = _securityProvider.User.UserName;
            gameProvider.UpdatedDate = DateTimeOffset.Now;

            _repository.GameProviders.AddOrUpdate(gameProvider);

            _repository.SaveChanges();
            _eventBus.Publish(new ProductUpdated(gameProvider));
        }
    }
}
