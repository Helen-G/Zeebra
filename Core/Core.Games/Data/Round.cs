using System;
using System.Collections.Generic;
using System.Linq;

namespace AFT.RegoV2.Core.Game.Data
{
    public class Round
    {
        /// <summary>
        /// Record ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// External Round ID. Identifies all game actions within one round. 
        /// </summary>
        public string ExternalRoundId { get; set; } 

        public Guid PlayerId { get; set; }
        public Game Game { get; set; }
        public Guid GameId { get; set; }
        public Guid BrandId { get; set; }

        
        public RoundStatus Status { get; set; }

        public DateTimeOffset CreatedOn { get; set; }
        public DateTimeOffset? ClosedOn { get; set; }

        public List<GameAction> GameActions { get; set; } 

        public Round()
        {
            ClosedOn = null;
        }
    }

    public enum RoundStatus
    {
        New, Open, Closed
    }
}
