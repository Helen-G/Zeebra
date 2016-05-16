using System;
using System.Data.Entity;
using AFT.RegoV2.Core.Bonus.Data;
using AFT.RegoV2.Infrastructure.DataAccess.Bonus;

namespace AFT.RegoV2.Tests.Common.TestDoubles
{
    public class FakeBonusRepository : BonusRepository
    {
        private readonly FakeDbSet<Bonus> _bonuses = new FakeDbSet<Bonus>();
        private readonly FakeDbSet<Template> _templates = new FakeDbSet<Template>();
        private readonly FakeDbSet<Player> _players = new FakeDbSet<Player>();
        private readonly FakeDbSet<Brand> _brands = new FakeDbSet<Brand>();
        private readonly FakeDbSet<Game> _games = new FakeDbSet<Game>();

        public override IDbSet<Player> Players { get { return _players; } }
        public override IDbSet<Bonus> Bonuses { get { return _bonuses; } }
        public override IDbSet<Template> Templates { get { return _templates; } }
        public override IDbSet<Brand> Brands { get { return _brands; } }
        public override IDbSet<Game> Games { get { return _games; } }

        protected override void LockBonus(Guid bonusId) { }
        protected override void LockPlayer(Guid playerId) { }

        public override int SaveChanges()
        {
            return 0;
        }
    }
}