using System;
using System.Collections.Concurrent;
using System.Security.Claims;
using System.Threading.Tasks;
using AFT.RegoV2.AdminApi.Filters;
using AFT.RegoV2.AdminApi.Interface.Common;
using AFT.RegoV2.Core.Security.ApplicationServices;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Infrastructure;
using Microsoft.Owin.Security.OAuth;
using Microsoft.Practices.Unity;
using Newtonsoft.Json;
using ValidationErrorField = AFT.RegoV2.AdminApi.Interface.Common.ValidationErrorField;

namespace AFT.RegoV2.AdminApi.Provider
{
    public class AuthServerProvider : OAuthAuthorizationServerProvider
    {
        private readonly IUnityContainer _container;
        public AuthServerProvider(IUnityContainer container)
        {
            _container = container;
        }

        public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            // TODO: Implement this when each member website has their own ClientID and Client Secret
            // For now - validate everyone
            
            if (context.ClientId == null)
            {
                context.Validated();
            }

            return Task.FromResult<object>(null);
        }

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            var securityService = _container.Resolve<UserService>();

            AdminApiException exception = null;

            try
            {
                if (string.IsNullOrEmpty(context.UserName) || string.IsNullOrEmpty(context.Password))
                    throw new Exception("Invalid username or password");

                if (!securityService.ValidateLogin(context.UserName, context.Password))
                    throw new Exception("Incorrect username or password");

                var user = securityService.GetUserByName(context.UserName);
                if (user == null)
                    throw new Exception("User not found");

                var identity = new ClaimsIdentity(context.Options.AuthenticationType);

                identity.AddClaim(new Claim(ClaimTypes.Name, context.UserName));
                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));

                context.Validated(identity);
                context.Request.Context.Authentication.SignIn(identity);
                return;
            }
            catch (Exception ex)
            {
                exception = new AdminApiException
                {
                    ErrorCode = ex.Message,
                    ErrorMessage = ex.Message,
                    Violations = new ValidationErrorField[]
                    {
                        new ValidationErrorField
                        {
                            ErrorCode = ex.Message,
                            ErrorMessage = ex.Message,
                            FieldName = string.Empty
                        }
                    }
                };
            }
            context.Rejected();

            var sError = JsonConvert.SerializeObject(exception);
            context.SetError("Login error", sError);
            context.Response.Headers.Add(InvalidLoginOwinMiddleware.InvalidLoginHeader, new[] { sError });
        }

        public override async Task GrantRefreshToken(OAuthGrantRefreshTokenContext context)
        {
            // TODO: Implement this when each member website has their own ClientID
            //var originalClient = context.Ticket.Properties.Dictionary["as:client_id"];
            //var currentClient = context.ClientId;

            //// enforce client binding of refresh token
            //if (originalClient != currentClient)
            //{
            //    context.Rejected();
            //    return;
            //}

            var identity = new ClaimsIdentity(context.Ticket.Identity);
            var newTicket = new AuthenticationTicket(identity, context.Ticket.Properties);
            context.Validated(newTicket);
        }
    }

    public class RefreshTokenProvider : IAuthenticationTokenProvider
    {
        // TODO: Later store refresh tokens to DB
        private static ConcurrentDictionary<string, AuthenticationTicket> _refreshTokens =
            new ConcurrentDictionary<string, AuthenticationTicket>();

        public async Task CreateAsync(AuthenticationTokenCreateContext context)
        {
            var guid = Guid.NewGuid().ToString();

            // maybe only create a handle the first time, then re-use for same client
            // copy properties and set the desired lifetime of refresh token
            var refreshTokenProperties = new AuthenticationProperties(context.Ticket.Properties.Dictionary)
            {
                IssuedUtc = context.Ticket.Properties.IssuedUtc,
                ExpiresUtc = DateTime.UtcNow.AddYears(1)
            };
            var refreshTokenTicket = new AuthenticationTicket(context.Ticket.Identity, refreshTokenProperties);

            _refreshTokens.TryAdd(guid, refreshTokenTicket);

            // consider storing only the hash of the handle
            context.SetToken(guid);
        }

        public async Task ReceiveAsync(AuthenticationTokenReceiveContext context)
        {
            AuthenticationTicket ticket;
            if (_refreshTokens.TryRemove(context.Token, out ticket))
            {
                context.SetTicket(ticket);
            }
            else
            {
                throw new AdminApiException
                {
                    ErrorCode = "500",
                    ErrorMessage = "Refresh token not found",
                    Violations = new ValidationErrorField[0]
                };
            }
        }

        public void Create(AuthenticationTokenCreateContext context)
        {
            throw new NotImplementedException();
        }

        public void Receive(AuthenticationTokenReceiveContext context)
        {
            throw new NotImplementedException();
        }
    }
}