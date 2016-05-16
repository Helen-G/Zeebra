using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AFT.RegoV2.Core.Game.Data
{
    public class WalletTemplateGameProvider
    {
        public Guid Id { get; set; }

        public Guid WalletTemplateId { get; set; }
        public WalletTemplate WalletTemplate { get; set; }

        public Guid GameProviderId { get; set; }
        public GameProvider GameProvider { get; set; }

    }
}
