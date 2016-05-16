using AFT.RegoV2.Core.Security.Data.Users;
using AFT.RegoV2.Core.Security.Interfaces;
using AFT.RegoV2.Domain.Security.Interfaces;

namespace AFT.RegoV2.Infrastructure.Providers
{
    public class UserInfoProvider : IUserInfoProvider
    {
        private readonly UserInfo _user;

        public UserInfoProvider(ISecurityProvider securityProvider)
        {
            _user = new UserInfo();

            if (securityProvider.IsUserAvailable)
            {
                _user.Username = securityProvider.User.UserName;
                _user.UserId = securityProvider.User.UserId;
            }
        }

        public UserInfo User
        {
            get { return _user; }
        }

        public bool IsUserAvailable
        {
            get { return !string.IsNullOrEmpty(_user.Username); }
        }
    }
}