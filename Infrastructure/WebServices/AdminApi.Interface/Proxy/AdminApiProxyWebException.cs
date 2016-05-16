using System;
using System.Net;
using AFT.RegoV2.AdminApi.Interface.Common;

namespace AFT.RegoV2.AdminApi.Interface.Proxy
{
    public class AdminApiProxyException : Exception
    {
        public HttpStatusCode StatusCode { get; private set; }

        public AdminApiException Exception { get; private set; }

        public AdminApiProxyException(AdminApiException exception, HttpStatusCode code) : base(exception.ErrorMessage)
        {
            StatusCode = code;
            Exception = exception;
        }
    }
}
