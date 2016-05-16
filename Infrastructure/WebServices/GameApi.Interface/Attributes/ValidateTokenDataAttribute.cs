using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using AFT.RegoV2.Core.Game.Services;
using AFT.RegoV2.GameApi.Interface.Classes;
using AFT.RegoV2.GameApi.Interface.ServiceContracts;
using AFT.RegoV2.GameApi.Interface.Services;
using AFT.RegoV2.Infrastructure.Providers;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.GameApi.Interface.Attributes
{
    public class ValidateTokenDataAttribute : BaseErrorAttribute
    {
        [Dependency]
        internal ITokenProvider TokenProvider { get; set; }
        [Dependency]
        internal ITokenValidationProvider TokenValidation { get; set; }

        public override void OnActionExecuting(HttpActionContext context)
        {
            Exception exception = null;
            try
            {
                foreach (var arg in context.ActionArguments.Values)
                {
                    var req = arg as IGameApiRequest;
                    if (req != null)
                    {
                        var tokenData = TokenProvider.Decrypt(req.AuthToken);

                        var validateToken = req as ValidateToken;

                        var ip = validateToken == null ? null : validateToken.PlayerIpAddress;

                        TokenValidation.ValidateToken(tokenData, ip, req.GetType());

                        return;
                    }

                }
            }
            catch (Exception current)
            {
                exception = current;
            }

            var executedContext = 
                new HttpActionExecutedContext(context,
                    exception ?? new ValidationException("Authentication token not found"));
            
            context.Response = GetResponseByException(executedContext);
        }
    }
}