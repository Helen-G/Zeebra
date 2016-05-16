using System;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Game.Data;

namespace AFT.RegoV2.Core.Game.Interfaces
{
    public interface IGameManagement : IApplicationService
    {
        void CreateGame(GameDTO game);
        void UpdateGame(GameDTO game);
        void DeleteGame(Guid id);

        //void CreateGameProviderConfiguration(GameProviderConfigurationDTO gameProviderConfiguration);
        //void UpdateGameProviderConfiguration(GameProviderConfigurationDTO gameProviderConfiguration);
        //void DeleteGameProviderConfiguration(Guid id);
        
        void UpdateProductSettings(BrandProductSettingsData viewModel);

        void CreateGameProvider(GameProvider gameProvider);
        void UpdateGameProvider(GameProvider gameProvider);
    }
}
