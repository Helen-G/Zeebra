using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Filters;
using AFT.RegoV2.Core.Game.ApplicationServices;
using AFT.RegoV2.Core.Game.Exceptions;
using AFT.RegoV2.GameApi.Interface.Attributes;
using AFT.RegoV2.GameApi.Interface.ServiceContracts.VictorBlue;
using AFT.RegoV2.GameApi.VictorBlue.Controllers;

namespace AFT.RegoV2.GameApi.VictorBlue.Attributes
{
    public class ProcessVictorBlueErrorAttribute : BaseErrorAttribute
    {
        private static readonly IDictionary<Type, int> ReturnCodeByExceptionType = 
            new Dictionary<Type, int>
            {
                {typeof(InvalidPlayerIpInTokenException), (int) VictorBlueCodes.InvalidLoginToken},
                {typeof(ExpiredTokenException), (int) VictorBlueCodes.LoginAuthenticationFailed},
                {typeof(PlayerNotFoundException), (int) VictorBlueCodes.PlayerNotExist},
                {typeof(InsufficientFundsException), (int) VictorBlueCodes.InsufficientBalance},
                {typeof(InvalidAmountException), (int) VictorBlueCodes.InvalidTransaction},
            };


        public override void OnActionExecuted(HttpActionExecutedContext context)
        {
            if(context.Exception == null) return;

            var message = context.ActionContext.ActionArguments.Values.FirstOrDefault() as CommonMessage;

            var hasMessage = message != null;
            var content = !hasMessage ? "" : Json.SerializeToString(message);

            var headers = Log.HeadersAsString(context.Request);
            Log.LogError(context.Exception.Message + " for request:\n" + headers + "\n" + content, context.Exception);

            context.Response = 
                context.Request.CreateResponse(
                    HttpStatusCode.OK, 
                    new ErrorResponse
                    {
                        Result = GetCodeByException(context.Exception)
                    });
        }

        private static int GetCodeByException(Exception ex)
        {
            var type = ex.GetType();

            int code;
            if (ReturnCodeByExceptionType.TryGetValue(type, out code))
            {
                return code;
            }
            
            // VictorBlueCodes.Unknown would cause their server to repetedly try resubmit
            return (int) VictorBlueCodes.InvalidRequestToken;
        }
    }
}