using System.Data.Entity;
using System.Linq;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Player;
using AFT.RegoV2.Core.Player.Data;

namespace AFT.RegoV2.Tests.Common.TestDoubles
{
    public class FakePlayerRepository : IPlayerRepository
    {
        private readonly FakeDbSet<Player> _players = new FakeDbSet<Player>();
        private readonly FakeDbSet<VipLevel> _vipLevels = new FakeDbSet<VipLevel>();
        private readonly FakeDbSet<SecurityQuestion> _securityQuestions = new FakeDbSet<SecurityQuestion>();
        private readonly FakeDbSet<PlayerBetStatistics> _playerBetStatistics = new FakeDbSet<PlayerBetStatistics>();
        private readonly FakeDbSet<PlayerActivityLog> _playerActivityLog = new FakeDbSet<PlayerActivityLog>();
        private readonly FakeDbSet<PlayerInfoLog> _playerInfoLog = new FakeDbSet<PlayerInfoLog>();
        private readonly FakeDbSet<Brand> _brands = new FakeDbSet<Brand>();
        private readonly FakeDbSet<IdentityVerification> _identityVerifications = new FakeDbSet<IdentityVerification>();

        private readonly IDbSet<IdentificationDocumentSettings> _identificationDocumentSettingses = new FakeDbSet<IdentificationDocumentSettings>();

        public IDbSet<Core.Player.Data.Player> Players
        {
            get { return _players; }
        }

        public IDbSet<IdentityVerification> IdentityVerifications
        {
            get { return _identityVerifications; }
        }

        public IDbSet<PlayerBetStatistics> PlayerBetStatistics
        {
            get { return _playerBetStatistics; }
        }

        public IDbSet<VipLevel> VipLevels
        {
            get { return _vipLevels; }
        }

        public IDbSet<SecurityQuestion> SecurityQuestions
        {
            get { return _securityQuestions; }
        }

        public IDbSet<PlayerActivityLog> PlayerActivityLog
        {
            get { return _playerActivityLog; }
        }

        public IDbSet<PlayerInfoLog> PlayerInfoLog
        {
            get { return _playerInfoLog; }
        }

        public IDbSet<Brand> Brands
        {
            get { return _brands; }
        }

        public IDbSet<IdentificationDocumentSettings> IdentificationDocumentSettings
        {
            get { return _identificationDocumentSettingses; }
        }

        public int SaveChanges()
        {
            //calling ToArray to prevent concurrency issues
            foreach (var brand in _brands.ToArray())
            {
                if (brand.DefaultVipLevelId.HasValue)
                    brand.DefaultVipLevel = _vipLevels.Where(x => x.Id == brand.DefaultVipLevelId).Single();
            }
            foreach (var player in _players.ToArray())
            {
                if (player.VipLevelId.HasValue)
                    player.VipLevel = _vipLevels.Where(x => x.Id == player.VipLevelId).Single();
            }
            return 0;
        }
    }
}
