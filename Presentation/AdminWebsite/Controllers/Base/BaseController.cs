using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using AFT.RegoV2.AdminApi.Interface;
using AFT.RegoV2.AdminApi.Interface.Proxy;
using AFT.RegoV2.AdminWebsite.Common;
using AFT.RegoV2.AdminWebsite.Common.ErrorHandling;
using AFT.RegoV2.AdminWebsite.Common.IpFiltering;
using AFT.RegoV2.AdminWebsite.Resources;
using AFT.RegoV2.Core.ApplicationServices.Player;
using AFT.RegoV2.Core.Security.ApplicationServices;
using AFT.RegoV2.Infrastructure.Constants;
using FluentValidation;
using FluentValidation.Results;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using ServiceStack.Validation;

namespace AFT.RegoV2.AdminWebsite.Controllers
{
    [AuthorizeIpAddress]
    [SessionExpire]
    public class BaseController : Controller
    {
        public AuthUser CurrentUser
        {
            get
            {
                AuthUser user = null;
                var cookie = System.Web.HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
                if (cookie != null)
                {
                    var ticket = FormsAuthentication.Decrypt(cookie.Value);
                    user = JsonConvert.DeserializeObject<AuthUser>(ticket.UserData);
                }
                return user;
            }
        }

        public IEnumerable<Guid> CurrentBrands
        {
            get
            {
                if (CurrentUser == null)
                {
                    return null;
                }

                var securityService = DependencyResolver.Current.GetService<UserService>();
                var user = securityService.GetUserById(CurrentUser.UserId);

                return user.AllowedBrands.Select(b => b.Id);
            }
        }

        public async Task<bool> LoginUser(string username, string password, bool rememberMe)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                return false;

            var securityService = DependencyResolver.Current.GetService<UserService>();

            if (!securityService.ValidateLogin(username, password))
                throw new ValidationException(new List<ValidationFailure> { new ValidationFailure(string.Empty, ValidationErrors.IncorrectUsernameOrPassword) });

            var user = securityService.GetUserByName(username);
            if (user == null)
                return false;

            securityService.SignInUser(user);

            var result = await GetAdminApiProxy(Request).Login(new LoginRequest()
            {
                Username = username,
                Password = password
            });

            var authUser = new AuthUser
            {
                UserId = user.Id,
                UserName = user.Username,
                Token = result.AccessToken,
                RefreshToken = result.RefreshToken
            };

            SetAuthCookie(username, rememberMe, authUser);

            return true;
        }

        public void LogoutUser()
        {
            var securityService = DependencyResolver.Current.GetService<UserService>();
            securityService.SignOutUser();
            FormsAuthentication.SignOut();
        }

        private void SetAuthCookie(string username, bool remeberMe, AuthUser user)
        {
            var cookie = FormsAuthentication.GetAuthCookie(username, remeberMe);
            var ticket = FormsAuthentication.Decrypt(cookie.Value);
            var newTicket = new FormsAuthenticationTicket(ticket.Version, ticket.Name, ticket.IssueDate,
                ticket.Expiration,
                ticket.IsPersistent, JsonConvert.SerializeObject(user), ticket.CookiePath);

            string encryptedTicket = FormsAuthentication.Encrypt(newTicket);
            cookie.Value = encryptedTicket;
            System.Web.HttpContext.Current.Response.Cookies.Add(cookie);
        }

        protected string SerializeJson(object response)
        {
            Response.ContentType = "application/json";
            var jsonSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            return JsonConvert.SerializeObject(response, jsonSettings);
        }

        protected static string ToCamelCase(string str)
        {
            return Char.ToLowerInvariant(str[0]) + str.Substring(1);
        }

        public string ErrorResponse()
        {
            var fields = ModelState.Where(p => p.Value.Errors.Count > 0)
                .Select(x => new { Name = ToCamelCase(x.Key), Errors = x.Value.Errors.Select(e => e.ErrorMessage) });
            return SerializeJson(new { Result = "failed", Fields = fields });
        }

        public string ValidationErrorResponse(ValidationError e)
        {
            var fields = e.Violations
                .Select(x => new { FieldName = ToCamelCase(x.FieldName), x.ErrorMessage });

            return SerializeJson(new { Result = "failed", Fields = fields.Select(f => new { Errors = new[] { f } }) });
        }

        public ActionResult ValidationErrorResponseActionResult(ValidationError e)
        {
            var fields = e.Violations
                .GroupBy(x => x.FieldName)
                .Select(x => new
                {
                    Name = ToCamelCase(x.Key),
                    Errors = x.Select(y => y.ErrorMessage).ToArray()
                }).ToArray();

            var jsonString = SerializeJson(new { Result = "failed", Fields = fields });

            return Content(jsonString, "application/json");
        }

        public ActionResult ValidationErrorResponseActionResult(IList<ValidationFailure> e)
        {
            var fields = e
                .GroupBy(x => x.PropertyName)
                .Select(x => new
                {
                    Name = ToCamelCase(x.Key),
                    Errors = x.Select(y => y.ErrorMessage).ToArray()
                }).ToArray();

            var jsonString = SerializeJson(new { Result = "failed", Fields = fields });

            return Content(jsonString, "application/json");
        }

        private AdminApiProxy GetAdminApiProxy(HttpRequestBase request)
        {
            return new AdminApiProxy(ConfigurationManager.AppSettings["AdminApiUrl"], request.AccessToken());
        }
    }
}