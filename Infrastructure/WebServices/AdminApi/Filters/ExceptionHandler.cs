﻿using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using AFT.RegoV2.AdminApi.Interface.Common;
using AFT.RegoV2.Shared;
using Newtonsoft.Json;

namespace AFT.RegoV2.AdminApi.Filters
{
    public class AdminApiExceptionHandler : ExceptionHandler
    {
        public override void Handle(ExceptionHandlerContext context)
        {
            base.Handle(context);

            //var content = new AdminApiException
            //{
            //    ErrorCode = context.Exception.Message,
            //    ErrorMessage = context.Exception.Message,
            //    StackTrace = context.Exception.StackTrace
            //};

            //if (context.Exception is ServiceStack.Validation.ValidationError)
            //{
            //    content = CreateValidationError(context);
            //}

            //context.Result = new ErrorResult
            //{
            //    Request = context.ExceptionContext.Request,
            //    Content = JsonConvert.SerializeObject(content)
            //};
        }

        //public override bool ShouldHandle(ExceptionHandlerContext context)
        //{
        //    return true;
        //}

        private AdminApiException CreateValidationError(ExceptionHandlerContext context)
        {
            if (!(context.Exception is ServiceStack.Validation.ValidationError))
            {
                return null;
            }

            var srcValidationException = context.Exception as ServiceStack.Validation.ValidationError;

            return srcValidationException.ToAdminApiException();
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
               
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                return Task.FromResult(response);
            }
        }
    }

    public static class ValidationErrorExtension
    {
        public static AdminApiException ToAdminApiException(this ServiceStack.Validation.ValidationError errorException)
        {
            return new AdminApiException
            {
                ErrorMessage = errorException.ErrorMessage,
                ErrorCode = errorException.ErrorCode,
                StackTrace = errorException.StackTrace,
                Violations = (from v in errorException.Violations
                              select new ValidationErrorField
                              {
                                  ErrorCode = v.ErrorCode,
                                  ErrorMessage = v.ErrorMessage,
                                  FieldName = v.FieldName
                              }).ToList()
            };
        }
    }
}