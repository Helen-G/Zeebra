using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using AFT.RegoV2.ApplicationServices.Security;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Player;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Domain.Player;
using AFT.RegoV2.Core.Player;
using AFT.RegoV2.Core.Player.Data;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Core.Player.Validators;
using AFT.RegoV2.Core.Services.Security;
using AFT.RegoV2.Domain.BoundedContexts.Security.Helpers;
using AFT.RegoV2.Domain.Player.Resources;
using AFT.RegoV2.Shared;
using VipLevel = AFT.RegoV2.Core.Player.Data.VipLevel;

namespace AFT.RegoV2.ApplicationServices.Player
{
    public interface IPlayerQueries : IApplicationService
    {
        AccountStatus? GetAccountStatus(string username);
        PlayerCommandResult ValidateLogin(string username, string password);
        Core.Player.Data.Player GetPlayer(Guid playerId);
        Task<Core.Player.Data.Player> GetPlayerAsync(PlayerId playerId);
        Core.Player.Data.Player GetPlayerForWithdraw(PlayerId playerId);
        IList<VipLevel> VipLevels { get; }
        IQueryable<Core.Player.Data.Player> GetPlayersByVipLevel(Guid vipLevelId);
        VipLevel GetDefaultVipLevel(Guid brandId);
        Core.Player.Data.Player GetPlayerByUsername(string username);
    }

    public class PlayerQueries : MarshalByRefObject,  IPlayerQueries
    {
        private readonly IPlayerRepository _repository;
        private readonly BrandQueries _brandQueries;

        public PlayerQueries(IPlayerRepository repository, BrandQueries brandQueries)
        {
            _repository = repository;
            _brandQueries = brandQueries;
        }

        public AccountStatus? GetAccountStatus(string username)
        {
            var player = _repository.Players.SingleOrDefault(x => x.Username == username);
            return player == null ? (AccountStatus?)null : player.AccountStatus;
        }

        public PlayerCommandResult ValidateLogin(string username, string password)
        {
            var result = new PlayerCommandResult
            {
                Success = true,
            };

            var loginValidatorResult = new LoginValidator(_repository).Validate(new LoginData
            {
                Username = username,
                Password = password
            });

            if (loginValidatorResult.IsValid == false)
            {
                result.ValidationResult = loginValidatorResult;
                result.Success = false;
                return result;
            }

            var player = _repository.Players.SingleOrDefault(x => x.Username == username);

            var playerValidationResult = new PlayerAccountValidator(_brandQueries, password).Validate(player);
            if (playerValidationResult.IsValid == false)
            {
                result.ValidationResult = playerValidationResult;
                result.Success = false;
            }

            
            result.Player = player;

            return result;
        }

        public IQueryable<Core.Player.Data.Player> GetPlayersByVipLevel(Guid vipLevelId)
        {
            return _repository.Players
                .Include(x => x.VipLevel)
                .Where(o => o.VipLevel.Id == vipLevelId);
        }

        public VipLevel GetDefaultVipLevel(Guid brandId)
        {
            var defaultVipLevel = _repository.Brands.Where(x => x.Id == brandId).Select(x => x.DefaultVipLevel).Single();
            if(defaultVipLevel == null)
                throw new RegoException(string.Format("Default VipLevel was not found for brand '{0}'", brandId));
            return defaultVipLevel;
        }

        public IQueryable<Core.Player.Data.Player> GetPlayersByPaymentLevel(Guid paymentLevelId)
        {
            return _repository.Players.Where(x => x.PaymentLevelId == paymentLevelId);
        }

        public Core.Player.Data.Player GetPlayerByUsername(string username)
        {
            return _repository.Players
                .Include(x => x.VipLevel)
                .SingleOrDefault(x => x.Username == username);
        }
        
        [Permission(Permissions.View, Module = Modules.PlayerManager)]
        public Core.Player.Data.Player GetPlayer(Guid playerId)
        {
            return _repository.Players
                .Include(x => x.VipLevel)
                .SingleOrDefault(x => x.Id == playerId);
        }

        [Permission(Permissions.View, Module = Modules.PlayerManager)]
        public async Task<Core.Player.Data.Player> GetPlayerAsync(PlayerId playerId)
        {
            return await _repository.Players
                .Include(x => x.VipLevel)
                .SingleOrDefaultAsync(x => x.Id == playerId);
        }

        [Permission(Permissions.Add, Module = Modules.OfflineWithdrawalRequest)]
        public Core.Player.Data.Player GetPlayerForWithdraw(PlayerId playerId)
        {
            return GetPlayer(playerId);
        }

        [Permission(Permissions.View, Module = Modules.PlayerManager)]
        public IQueryable<Core.Player.Data.Player> GetPlayers()
        {
            return _repository.Players.Include(x => x.VipLevel).AsNoTracking();
        }

        public int GetPlayersCountWithVipLevel(Guid vipLevelId)
        {
            return _repository.Players
                .Include(x => x.VipLevel)
                .Count(o => o.VipLevel.Id == vipLevelId);
        }

        public IList<VipLevel> VipLevels
        {
            get { return _repository.VipLevels.ToList(); }
        }

        public IEnumerable<PlayerInfoLog> GetPlayerInfoLog(Guid playerId)
        {
            return _repository.PlayerInfoLog
                .Include(p => p.Player)
                .Where(p => p.Player.Id == playerId)
                .OrderByDescending(p => p.RowVersion);
        }

        public IQueryable<IdentityVerification> GetPlayerIdentityVerifications(Guid playerId)
        {
            var player = _repository.Players
                .Include(x => x.IdentityVerifications)
                .SingleOrDefault(x => x.Id == playerId);

            return player == null
                ? null
                : player.IdentityVerifications.AsQueryable();
        }

        public IQueryable<PlayerActivityLog> GetPlayerActivityLog()
        {
            return _repository.PlayerActivityLog;
        }

        public IQueryable<PlayerBetStatistics> GetPlayerBetStatistics()
        {
            return _repository.PlayerBetStatistics;
        }

        public IQueryable<SecurityQuestion> GetSecurityQuestions()
        {
            return _repository.SecurityQuestions;
        }
    }
}