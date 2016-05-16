using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AFT.RegoV2.ApplicationServices.Player;
using AFT.RegoV2.Core.ApplicationServices.Player;
using AFT.RegoV2.Core.Bonus.Data;
using AFT.RegoV2.Core.Player.Data;
using AFT.RegoV2.Core.Security.Data;
using AFT.RegoV2.Core.Security.Events;
using AFT.RegoV2.Infrastructure;
using AFT.RegoV2.MemberApi.Filters;
using AFT.RegoV2.MemberApi.Interface;
using Microsoft.Owin.Security.OAuth;
using Microsoft.Practices.Unity;
using Newtonsoft.Json;
using ValidationError = ServiceStack.Validation.ValidationError;

namespace AFT.RegoV2.MemberApi.Provider
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
            var commands = _container.Resolve<PlayerCommands>();

            // read additional data from the request body
            var requestData = await context.Request.ReadFormAsync();

            var brandId = Guid.Parse(requestData["BrandId"]);
            var ipAddress = requestData["IpAddress"];
            var jsonHeaders = requestData["BrowserHeaders"];

            var headers = string.IsNullOrWhiteSpace(jsonHeaders) ?
                new Dictionary<string, string>() :
                JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonHeaders);

            
            MemberApiException exception = null;
            try
            {
                var result = commands.Login(context.UserName, context.Password, new LoginRequestContext
                {
                    BrandId = brandId,
                    IpAddress = ipAddress,
                    BrowserHeaders = headers
                });

                if (result.Success)
                {
                    var identity = new ClaimsIdentity(context.Options.AuthenticationType);

                    identity.AddClaim(new Claim("username", context.UserName));
                    identity.AddClaim(new Claim("playerId", result.Player.Id.ToString()));

                    context.Validated(identity);
                    context.Request.Context.Authentication.SignIn(identity);
                    return;
                }

                exception = new MemberApiException
                {
                    ErrorCode = result.ValidationResult.Errors[0].ErrorMessage,
                    ErrorMessage = result.ValidationResult.Errors[0].ErrorMessage,
                    Violations = result.ValidationResult.Errors.Select(
                        x => new ValidationErrorField
                        {
                            ErrorCode = x.ErrorCode,
                            ErrorMessage = x.ErrorMessage,
                            FieldName = x.PropertyName
                        }
                    ).ToList()
                };
            }
            catch (Exception ex)
            {
                exception = new MemberApiException
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
            context.Response.Headers.Add(InvalidLoginOwinMiddleware.InvalidLoginHeader, new[] {sError});
            
        }
    }
}