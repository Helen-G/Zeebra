using AFT.RegoV2.AdminApi.Controllers.Base;
using AFT.RegoV2.Core.Security.Data.Users;
using AFT.RegoV2.Core.Security.Interfaces;

namespace AFT.RegoV2.AdminApi.Provider
{
    public class UserInfoProvider : BaseApiController, IUserInfoProvider
    {
        private readonly UserInfo _user;

        public UserInfoProvider()
        {
            _user = new UserInfo()
            {
                Username = Username,
                UserId = UserId
            };
        }

        public new UserInfo User
        {
            get { return _user; }
        }

        public bool IsUserAvailable
        {
            get { return _user != null; }
        }
    }
}