using AFT.RegoV2.Core.Services.Security;
using AFT.RegoV2.Domain.Security.Data;

namespace AFT.RegoV2.Core.Security.Interfaces
{
    public interface ISessionProvider
    {
        AuthUser User { get; set; }
        void SetUser(Data.User user);
        void ClearUser();
    }
}
