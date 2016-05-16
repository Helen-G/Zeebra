using System.Linq;
using AFT.RegoV2.Core.Security.Data;
using AFT.RegoV2.Core.Security.Interfaces;
using AFT.RegoV2.Core.Services.Security;
using AFT.RegoV2.Domain.Security.Data;
using AFT.RegoV2.Domain.Security.Interfaces;

namespace AFT.RegoV2.Infrastructure.Providers.Security.Base
{
    public abstract class SessionProviderBase : ISessionProvider
    {
        protected abstract void Set<T>(string key, T value);

        protected abstract T Get<T>(string key);

        public AuthUser User
        {
            get
            {
                var user = Get<AuthUser>("User");

                return user;
            }

            set { Set("User", value); }
        }

        public void SetUser(User user)
        {
            var auth = new AuthUser
            {
                UserId = user.Id,
                UserName = user.Username
            };

            User = auth;
        }

        public void ClearUser()
        {
            User = null;
        }
    }
}
