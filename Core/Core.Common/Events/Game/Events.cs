using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Common.Events.Games
{
    public class BetPlaced : GameActionEventBase { }

    public class BetWon : GameActionEventBase { }

    public class BetLost : GameActionEventBase { }

    public class BetPlacedFree : GameActionEventBase { }

    public class BetCancelled: GameActionEventBase { }

    public class BetAdjusted : GameActionEventBase { }

    public class SessionStarted : DomainEventBase { }
}
