using System;
using System.Data.Entity;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Player.Data;

namespace AFT.RegoV2.Core.Player
{
    public interface IPlayerRepository
    {
        IDbSet<Data.Player>     Players { get; }
        IDbSet<PlayerBetStatistics>         PlayerBetStatistics { get; }
        IDbSet<VipLevel>                    VipLevels { get; }
        IDbSet<SecurityQuestion>            SecurityQuestions { get; }
        IDbSet<PlayerActivityLog>           PlayerActivityLog { get; }
        IDbSet<PlayerInfoLog>               PlayerInfoLog { get; }
        IDbSet<Data.Brand>                  Brands { get; }
        IDbSet<IdentificationDocumentSettings> IdentificationDocumentSettings { get; }
        IDbSet<IdentityVerification>        IdentityVerifications { get; }

        int SaveChanges();
    }
}