using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Results;
using System.Web.UI.WebControls;
using AFT.RegoV2.MemberApi.Interface;
using AFT.RegoV2.MemberApi.Interface.Proxy;
using Newtonsoft.Json;

namespace AFT.RegoV2.MemberWebsite.Common
{
    public class MemberApiExceptionHandler : ExceptionHandler
    {
        
        public override void Handle(ExceptionHandlerContext context)
        {
            base.Handle(context);

            var exception = context.Exception;

            if (exception is MemberApiProxyException)
            {
                var proxyException = exception as MemberApiProxyException;

                if (proxyException.StatusCode == HttpStatusCode.BadRequest)
                {
                    
                }

                context.Result = new ErrorResult
                {
                    Request = context.ExceptionContext.Request,
                    
                    Content = JsonConvert.SerializeObject(new ErrorResponseObject
                    {
                        Unexpected = proxyException.StatusCode == HttpStatusCode.BadRequest, // we may expect some validation errors
                        Errors = proxyException.Exception.Violations,
                        Message = proxyException.Message,
                        Unauthorized =  proxyException.StatusCode == HttpStatusCode.Unauthorized
                    })
                };
            }
            else
            {
                context.Result = new ErrorResult
                {
                    Request = context.ExceptionContext.Request,
                    Content = JsonConvert.SerializeObject(new
                    {
                        Unexpected = true,
                        // Errors = exception.GetFieldErrors(),
                        Message = exception.Message,
                        Unauthorized =  false
                    })
                };
            }
        }

        private class ErrorResponseObject
        {
            [JsonProperty(PropertyName = "unexpected")]
            public bool Unexpected { get; set; }
            [JsonProperty(PropertyName = "errors")]
            public IList<ValidationErrorField> Errors { get; set; }
            [JsonProperty(PropertyName = "message")]
            public string Message { get; set; }
            [JsonProperty(PropertyName = "unauthorized")]
            public bool Unauthorized { get; set; }
        }

        private class ErrorResult : IHttpActionResult
        {
            public HttpRequestMessage Request { get; set; }
            public string Content { get; set; }

            public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
            {
                
                var response = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(Content),
                    RequestMessage = Request
                };

                return Task.FromResult(response);
            }
        }
    }
}