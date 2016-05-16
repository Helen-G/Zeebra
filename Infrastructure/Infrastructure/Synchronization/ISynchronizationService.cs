using System;

namespace AFT.RegoV2.Infrastructure.Synchronization
{
    public interface ISynchronizationService
    {
        void Execute(string sectionName, Action action);
    }
}
