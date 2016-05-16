namespace AFT.RegoV2.Core.Common.Interfaces
{
    /// <summary>
    /// This interface to mark classes that are to be initialized in the WinService
    /// </summary>
    public interface ISeedable
    {
        void Seed();
    }
}
