using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using AFT.RegoV2.ApplicationServices.Security;
using AFT.RegoV2.Core.Services.Security;
using Newtonsoft.Json;

namespace AFT.RegoV2.Domain.BoundedContexts.Security.Helpers
{
    public class AuthUser
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; } 
        public ICollection<Guid> BrandIds { get; set; }
    }

    public class SharedData : IDisposable
    {
        public Guid? UserId { get; private set; }
        public string Username { get; private set; }
        public List<Guid> Brands { get; private set;}

        private static SharedData _sharedData;

        private static AuthUser _testUser;

        public static AuthUser User
        {
            get
            {
                if (_testUser != null)
                    return _testUser;

                AuthUser user = null;
                var cookie = HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
                if (cookie != null)
                {
                    var ticket = FormsAuthentication.Decrypt(cookie.Value);
                    user = JsonConvert.DeserializeObject<AuthUser>(ticket.UserData);
                }

                if (user == null)
                {
                    throw new SessionExpiredException();
                }

                return user;
            }
        }

        /// <summary>
        /// Setup user for unit tests. Example of usage
        ///     using(SharedData.CreateTestUserSession(user))
        ///     {
        ///         //Logic specific for user passed as argument
        ///     }
        /// After using is finished user data is cleared
        /// TODO: If it would be necessary, it is possible to implement sessions mechanism in such way.
        /// TODO: Then it would be possible to write nested using to override shared data for some piece of code. 
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static SharedData CreateTestUserSession(User user)
        {
            var auth = new AuthUser
            {
                UserId = user.Id,
                UserName = user.Username
            };

            _testUser = auth;

            Current.SetUser(user);

            return Current;
        }

        public static SharedData Current {
            get
            {
                //Temporary storage for testing purposes
                if (HttpContext.Current == null)
                {
                    if (_sharedData == null)
                    {
                        _sharedData = new SharedData();
                    }
                }
                else if (HttpContext.Current.Session["SharedData"] == null)
                {   
                    HttpContext.Current.Session["SharedData"] = new SharedData();
                }

                return _sharedData ?? (SharedData)HttpContext.Current.Session["SharedData"];
             }
        }

        private SharedData()
        {
        }

        public void SetUser(User user)
        {
            UserId = user.Id;
            Username = user.Username;            
            Brands = new List<Guid>();
            Brands.AddRange(user.AllowedBrands.Select(b => b.Id));
            //User = user;
        }

        public void ClearUser()
        {
            UserId = null;
            Username = null;
            Brands = null;
            //User = null;
        }

        public void Dispose()
        {
            ClearUser();
        }
    }
}
