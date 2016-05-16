﻿using System;

namespace AFT.RegoV2.Core.Player.Data
{
    public class PlayerActivityLog
    {
        public Guid Id { get; set; }
        public Guid PlayerId { get; set; }
        public string Category { get; set; }
        public string ActivityDone { get; set; }
        public string PerformedBy { get; set; }
        public DateTimeOffset DatePerformed { get; set; }
        public string UpdatedBy { get; set; }
        public DateTimeOffset? DateUpdated { get; set; }
        public string Remarks { get; set; }
    }
}
