using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Common.Events.Games
{
    public class BetPlaced : BetEvent { }

    public class BetWon : BetEvent { }

    public class BetLost : BetEvent { }

    public class BetPlacedFree : BetEvent { }

    public class BetCancelled: BetEvent { }

    public class BetAdjusted : BetEvent { }

    public class SessionStarted : DomainEventBase { }
}
