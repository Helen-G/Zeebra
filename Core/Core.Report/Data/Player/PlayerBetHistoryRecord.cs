using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AFT.RegoV2.Domain.BoundedContexts.Report.Attributes;

namespace AFT.RegoV2.BoundedContexts.Report.Data
{
    public class PlayerBetHistoryRecord
    {
        [Key]
        public Guid GameActionId { get; set; }

        [Index, Export("Round ID")]
        public Guid RoundId { get; set; }

        [Index, MaxLength(100), Export]
        public string Licensee { get; set; }

        [Index, MaxLength(100), Export]
        public string Brand { get; set; }

        [Index, MaxLength(100), Export("Username")]
        public string LoginName { get; set; }

        [Index, MaxLength(100), Export("IP Address")]
        public string UserIP { get; set; }

        [Index, MaxLength(100), Export("Product Name")]
        public string GameName { get; set; }

        [Index, Export("Bet Date")]
        public DateTimeOffset DateBet { get; set; }

        [Index, Export("Bet Amount")]
        public decimal BetAmount { get; set; }

        [Index, Export("Win/Loss")]
        public decimal TotalWinLoss { get; set; }

        [Index, MaxLength(100), Export]
        public string Currency { get; set; }
    }
}