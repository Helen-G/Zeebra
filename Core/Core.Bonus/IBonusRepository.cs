using System;
using System.Data.Entity;
using System.Linq;
using AFT.RegoV2.Core.Bonus.Data;
using BonusRedemption = AFT.RegoV2.Core.Bonus.Entities.BonusRedemption;

namespace AFT.RegoV2.Core.Bonus
{
    public interface IBonusRepository
    {
        IDbSet<Template>                Templates { get; }
        IDbSet<Data.Bonus>              Bonuses { get; }
        IDbSet<Player>                  Players { get; set; }
        IDbSet<Data.Brand>              Brands { get; set; }
        IDbSet<Game>                    Games { get; set; } 

        Entities.Player                 GetLockedPlayer(Guid playerId);
        Entities.Bonus                  GetLockedBonus(Guid bonusId);
        Entities.Bonus                  GetLockedBonus(string bonusCode);
        Entities.Bonus                  GetLockedBonusOrNull(string bonusCode);
        BonusRedemption                 GetBonusRedemption(Guid playerId, Guid redemptionId);
        IQueryable<Data.Bonus>          GetCurrentVersionBonuses();
        void                            RemoveGameContributionsForGame(Guid gameId);

        int SaveChanges();
    }
}