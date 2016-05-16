using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Filters;
using AFT.RegoV2.Core.Game.Services;
using AFT.RegoV2.GameApi.Interface.Classes;
using AFT.RegoV2.GameApi.Interface.ServiceContracts;
using AFT.RegoV2.GameApi.Interface.Services;
using AFT.RegoV2.Infrastructure.Providers;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.GameApi.Interface.Attributes
{
    public abstract class BaseErrorAttribute : ActionFilterAttribute
    {
        [Dependency]
        public IGameProviderLog Log { get; set; }
        [Dependency]
        public IErrorManager ErrorManager { get; set; }
        [Dependency]
        public IJsonSerializationProvider Json { get; set; }
        [Dependency]
        public IUnityContainer Container { get; set; }

        protected Type GetReturnType(HttpActionExecutedContext context)
        {
            Type returnType = null;
            if (context.ActionContext != null &&
                context.ActionContext.ActionDescriptor != null &&
                context.ActionContext.ActionDescriptor.ReturnType != null)
            {
                returnType = context.ActionContext.ActionDescriptor.ReturnType;
            }
            return returnType;
        }
        protected HttpResponseMessage GetResponseByException(HttpActionExecutedContext context)
        {
            string description;
            var code = ErrorManager.GetErrorCodeByException(context.Exception, out description);

            if(code == GameApiErrorCode.SystemError) Log.LogError(description, context.Exception);
            else Log.LogWarn(description);

            var returnType = GetReturnType(context);
            
            if (returnType != null)
            {
                if (typeof (IGameApiErrorDetails).IsAssignableFrom(returnType) && 
                    returnType.GetConstructor(Type.EmptyTypes) != null)
                {
                    var error = (IGameApiErrorDetails)Activator.CreateInstance(returnType);
                    error.ErrorCode = code;
                    error.ErrorDescription = description;

                    EnsureBalance(context, returnType, error);

                    return context.Request.CreateResponse(HttpStatusCode.InternalServerError, error);
                }
            }

            return new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = 
                    new StringContent(Json.SerializeToString(new GameApiResponseBase
                        {
                            ErrorCode = code,
                            ErrorDescription = description
                        }))
            };
        }

        private void EnsureBalance(HttpActionExecutedContext context, Type returnType, IGameApiErrorDetails error)
        {
            var requestObj =
                context.ActionContext.ActionArguments.Values.FirstOrDefault(v => v is GameApiRequestBase) as GameApiRequestBase;
            if (requestObj != null)
            {
                try
                {
                    var balanceProp = returnType.GetProperty("Balance");
                    if (balanceProp != null && balanceProp.PropertyType == typeof (decimal))
                    {
                        var result = Container.Resolve<IGamesCommonOperationsProvider>().GetBalance(new GetBalance
                        {
                            AuthToken = requestObj.AuthToken,
                        });
                        balanceProp.SetValue(error, result.Balance);
                    }
                }
                // ReSharper disable once EmptyGeneralCatchClause
                catch
                {
                }
            }
        }
    }
}