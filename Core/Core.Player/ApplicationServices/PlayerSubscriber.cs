using System;
using System.Data.Entity;
using System.Linq;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Events;
using AFT.RegoV2.Core.Common.Events.Brand;
using AFT.RegoV2.Core.Common.Events.Games;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Player;
using AFT.RegoV2.Core.Player.Data;
using AFT.RegoV2.Domain.Brand.Events;

namespace AFT.RegoV2.BoundedContexts.Player.ApplicationServices
{
    public class PlayerSubscriber : IBusSubscriber,
        IConsumes<VipLevelRegistered>,
        IConsumes<BrandDefaultVipLevelChanged>,
        IConsumes<BetWon>,
        IConsumes<BetPlacedFree>,
        IConsumes<BetLost>, 
        IConsumes<BetAdjusted>,
        IConsumes<BrandRegistered>
    {
        private readonly IPlayerRepository  _playerRepository;

        public PlayerSubscriber(IPlayerRepository playerRepository)
        {
            _playerRepository = playerRepository;
        }

        public void Consume(VipLevelRegistered @event)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var vipLevel = new VipLevel
                {
                    Id = @event.Id,
                    BrandId = @event.BrandId,
                    Code = @event.Code,
                    Name = @event.Name,
                    Rank = @event.Rank,
                    Description = @event.Description,
                    ColorCode = @event.ColorCode,
                    Status = @event.Status
                };

                _playerRepository.VipLevels.Add(vipLevel);
                _playerRepository.SaveChanges();

                scope.Complete();
            }
        }

        public void Consume(BrandDefaultVipLevelChanged @event)
        {
            var brand = _playerRepository.Brands.Single(x => x.Id == @event.BrandId);
            brand.DefaultVipLevelId = @event.DefaultVipLevelId;
            _playerRepository.SaveChanges();
        }

        public void Consume(BetWon @event)
        {
            var statistics = GetPlayerBetStatistics(@event.PlayerId);
            statistics.TotalWon += @event.WonAmount;
            _playerRepository.SaveChanges();
        }

        public void Consume(BetPlacedFree @event)
        {
            var statistics = GetPlayerBetStatistics(@event.PlayerId);
            statistics.TotalWon += @event.WonAmount;
            _playerRepository.SaveChanges();
        }

        public void Consume(BetLost @event)
        {
            var statistics = GetPlayerBetStatistics(@event.PlayerId);
            statistics.TotalLoss += @event.Amount;
            _playerRepository.SaveChanges();
        }

        public void Consume(BetAdjusted @event)
        {
            var statistics = GetPlayerBetStatistics(@event.PlayerId);
            statistics.TotlAdjusted += @event.AdjustedAmount;
            _playerRepository.SaveChanges();
        }

        public void Consume(BrandRegistered @event)
        {
            _playerRepository.Brands.Add(new Brand { Id = @event.Id, Name = @event.Name });
            _playerRepository.SaveChanges();
        }

        private PlayerBetStatistics GetPlayerBetStatistics(Guid playerId)
        {
            var statistics = _playerRepository.PlayerBetStatistics
                .FirstOrDefault(s => s.PlayerId == playerId);

            if (statistics != null)
                return statistics;

            statistics = new PlayerBetStatistics
            {
                PlayerId = playerId,
            };
            _playerRepository.PlayerBetStatistics.Add(statistics);

            return statistics;
        }
    }
}