using System;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using AFT.RegoV2.GameApi.Interface.Extensions;
using AFT.RegoV2.GameApi.Interface.Services;
using AFT.RegoV2.Infrastructure.OAuth2;
using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OAuth2;
using DotNetOpenAuth.OAuth2.Messages;
using Newtonsoft.Json;

namespace AFT.RegoV2.GameApi.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class WebApiRequireOAuth2Scope : AuthorizationFilterAttribute
    {
        private readonly string[] _oauth2Scopes;

        public WebApiRequireOAuth2Scope(params string[] oauth2Scopes)
        {
            _oauth2Scopes = oauth2Scopes;
        }

        public override void OnAuthorization(HttpActionContext actionContext)
        {
            try
            {
                var container = GameApiFactory.Default.Container;
                var authServerKeys = (ICryptoKeyPair)container.Resolve(typeof(ICryptoKeyPair), "authServer");
                var dataServerKeys = (ICryptoKeyPair)container.Resolve(typeof(ICryptoKeyPair), "dataServer");
                var tokenAnalyzer = 
                    new StandardAccessTokenAnalyzer(
                            authServerKeys.PublicSigningKey,
                            dataServerKeys.PrivateEncryptionKey);
                var oauth2ResourceServer = new DotNetOpenAuth.OAuth2.ResourceServer(tokenAnalyzer);
                ((ApiController)actionContext.ControllerContext.Controller).User = 
                    oauth2ResourceServer.GetPrincipal(actionContext.Request.GetRequestBase(), _oauth2Scopes) as PlayerPrincipal;
            }
            catch (ProtocolFaultResponseException ex)
            {
                HandleUnauthorizedRequest(actionContext, ex);
            }
        }

        protected virtual void HandleUnauthorizedRequest(HttpActionContext actionContext, ProtocolFaultResponseException ex)
        {
            var response = ex.CreateErrorResponse();
            UnauthorizedResponse error = ex.ErrorResponseMessage as UnauthorizedResponse;
            if (error != null)
            {
                response.Body = JsonConvert.SerializeObject(error.ToOAuth2JsonResponse());
                response.Headers[System.Net.HttpResponseHeader.ContentType] = "application/json";
            }
            var context = actionContext.Request.GetHttpContext();
            response.Respond(context);
            context.Response.End();
        }
    }
}