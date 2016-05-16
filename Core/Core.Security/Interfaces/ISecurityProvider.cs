using AFT.RegoV2.Core.Security.Interfaces;
using AFT.RegoV2.Domain.Security.Data;

namespace AFT.RegoV2.Domain.Security.Interfaces
{
    public interface ISecurityProvider
    {
        AuthUser User { get; }
        bool IsUserAvailable { get; }
        ISessionProvider Session { get; } 
    }
}
