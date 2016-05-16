using AFT.RegoV2.Core.Security.Data.Users;

namespace AFT.RegoV2.Core.Security.Interfaces
{
    public interface IUserInfoProvider
    {
        UserInfo User { get; }
        bool IsUserAvailable { get; }
    }
}