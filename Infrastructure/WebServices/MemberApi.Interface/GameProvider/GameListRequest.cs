using System;
using System.Collections.Generic;

namespace AFT.RegoV2.MemberApi.Interface.GameProvider
{
    public class GameListRequest
    {
        public Guid PlayerId { get; set; }
    }

    public class GameListResponse
    {
        public List<GameProviderData> GameProviders { get; set; }
    }

    public class GameProviderData
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public List<GameData> Games { get; set; }
    }

    public class GameData
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}
